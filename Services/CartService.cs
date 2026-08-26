using EndustriB2C.Auth;
using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Services;

public class CartService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _current;

    public CartService(AppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    public async Task<CartDto> GetAsync(HttpContext http)
    {
        var cart = await LoadOrCreateAsync(http, createIfMissing: false);
        if (cart is null)
        {
            return new CartDto { IsGuest = !_current.IsAuthenticated };
        }

        return ToDto(cart);
    }

    public async Task<CartDto> AddAsync(HttpContext http, CartItemRequest request)
    {
        var product = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.ProductId && p.IsActive)
                      ?? throw new InvalidOperationException("Ürün bulunamadı.");

        if (product.Stock < request.Quantity)
        {
            throw new InvalidOperationException("Yeterli stok yok.");
        }

        var cart = await LoadOrCreateAsync(http, createIfMissing: true) ?? throw new InvalidOperationException("Sepet oluşturulamadı.");
        var item = cart.Items.FirstOrDefault(x => x.ProductId == request.ProductId);
        var newQty = (item?.Quantity ?? 0) + request.Quantity;
        if (newQty > product.Stock)
        {
            throw new InvalidOperationException("Yeterli stok yok.");
        }

        if (item is null)
        {
            cart.Items.Add(new CartItem { ProductId = request.ProductId, Quantity = request.Quantity });
        }
        else
        {
            item.Quantity = newQty;
        }

        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return await GetAsync(http);
    }

    public async Task<CartDto> UpdateQtyAsync(HttpContext http, int itemId, int quantity)
    {
        var cart = await LoadOrCreateAsync(http, createIfMissing: false) ?? throw new InvalidOperationException("Sepet boş.");
        var item = cart.Items.FirstOrDefault(x => x.Id == itemId) ?? throw new InvalidOperationException("Kalem bulunamadı.");
        if (quantity <= 0)
        {
            _db.CartItems.Remove(item);
        }
        else
        {
            if (item.Product.Stock < quantity)
            {
                throw new InvalidOperationException("Yeterli stok yok.");
            }
            item.Quantity = quantity;
        }

        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return await GetAsync(http);
    }

    public async Task ClearAsync(HttpContext http)
    {
        var cart = await LoadOrCreateAsync(http, createIfMissing: false);
        if (cart is null) return;
        _db.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task<Cart?> LoadTrackedAsync(HttpContext http) => await LoadOrCreateAsync(http, false);

    private async Task<Cart?> LoadOrCreateAsync(HttpContext http, bool createIfMissing)
    {
        IQueryable<Cart> q = _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Translations)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Prices)
                        .ThenInclude(pr => pr.Campaign)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images);

        Cart? cart = null;
        if (_current.UserId is int userId)
        {
            cart = await q.FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart is null && createIfMissing)
            {
                cart = new Cart { UserId = userId };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
                cart = await q.FirstAsync(c => c.Id == cart.Id);
            }
            return cart;
        }

        Guid guestToken;
        if (http.Request.Cookies.TryGetValue(AuthService.GuestCartCookie, out var raw) && Guid.TryParse(raw, out guestToken))
        {
            cart = await q.FirstOrDefaultAsync(c => c.GuestToken == guestToken);
        }
        else if (createIfMissing)
        {
            guestToken = Guid.NewGuid();
        }
        else
        {
            return null;
        }

        if (cart is null && createIfMissing)
        {
            cart = new Cart
            {
                GuestToken = guestToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
            };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
            WriteGuestCookie(http, guestToken);
            cart = await q.FirstAsync(c => c.Id == cart.Id);
        }

        return cart;
    }

    private void WriteGuestCookie(HttpContext http, Guid token)
    {
        http.Response.Cookies.Append(AuthService.GuestCartCookie, token.ToString(), new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            Path = "/",
            MaxAge = TimeSpan.FromDays(30),
            IsEssential = true
        });
    }

    public static CartDto ToDto(Cart cart)
    {
        var items = cart.Items.Select(i =>
        {
            var list = i.Product.ToListDto();
            var unit = list.DiscountedPrice ?? list.Price;
            return new CartItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Name = list.Name,
                Sku = list.Sku,
                ImageUrl = list.ImageUrl,
                Quantity = i.Quantity,
                Stock = i.Product.Stock,
                UnitPrice = unit,
                LineTotal = unit * i.Quantity
            };
        }).ToList();

        var sub = items.Sum(x => x.LineTotal);
        return new CartDto
        {
            Id = cart.Id,
            IsGuest = cart.UserId is null,
            ItemCount = items.Sum(x => x.Quantity),
            SubTotal = sub,
            DiscountAmount = 0,
            Total = sub,
            Items = items
        };
    }
}
