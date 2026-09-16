using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Services;

public class CartService
{
    public const string GuestCartCookie = "b2c_guest";

    private readonly AppDbContext _db;

    public CartService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<CartDto> GetAsync(HttpContext http)
    {
        var cart = await LoadOrCreateAsync(http, createIfMissing: false);
        return cart is null ? new CartDto { IsGuest = true } : ToDto(cart);
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
        return await ReloadDtoAsync(cart.Id);
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
        return await ReloadDtoAsync(cart.Id);
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

    private IQueryable<Cart> CartQuery() =>
        _db.Carts
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

    private async Task<CartDto> ReloadDtoAsync(int cartId)
    {
        var cart = await CartQuery().FirstAsync(c => c.Id == cartId);
        return ToDto(cart);
    }

    private async Task<Cart?> LoadOrCreateAsync(HttpContext http, bool createIfMissing)
    {
        Guid guestToken;
        if (!(http.Request.Cookies.TryGetValue(GuestCartCookie, out var raw) && Guid.TryParse(raw, out guestToken)))
        {
            guestToken = Guid.NewGuid();
        }

        // Her tarayıcıya özel kimlik: ilk ziyarette cookie yazılır, sonraki isteklerde aynı sepet kullanılır.
        WriteGuestCookie(http, guestToken);

        var cart = await CartQuery().FirstOrDefaultAsync(c => c.GuestToken == guestToken && c.UserId == null);
        if (cart is null && createIfMissing)
        {
            cart = new Cart
            {
                GuestToken = guestToken,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(30)
            };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
            cart = await CartQuery().FirstAsync(c => c.Id == cart.Id);
        }

        return cart;
    }

    private static void WriteGuestCookie(HttpContext http, Guid token)
    {
        http.Response.Cookies.Append(GuestCartCookie, token.ToString(), new CookieOptions
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
            IsGuest = true,
            ItemCount = items.Sum(x => x.Quantity),
            SubTotal = sub,
            DiscountAmount = 0,
            Total = sub,
            Items = items
        };
    }
}
