using EndustriB2C.Auth;
using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Services;

public class OrderService
{
    private readonly AppDbContext _db;
    private readonly CartService _carts;
    private readonly ICurrentUser _current;

    public OrderService(AppDbContext db, CartService carts, ICurrentUser current)
    {
        _db = db;
        _carts = carts;
        _current = current;
    }

    public async Task<OrderDetailDto> CheckoutAsync(HttpContext http, CheckoutRequest request)
    {
        var cart = await _carts.LoadTrackedAsync(http) ?? throw new InvalidOperationException("Sepetiniz boş.");
        if (cart.Items.Count == 0)
        {
            throw new InvalidOperationException("Sepetiniz boş.");
        }

        Campaign? campaign = null;
        if (!string.IsNullOrWhiteSpace(request.CouponCode))
        {
            var code = request.CouponCode.Trim().ToUpperInvariant();
            campaign = await _db.Campaigns.FirstOrDefaultAsync(c => c.CouponCode == code);
            if (campaign is null || !campaign.IsActive || campaign.StartDate > DateTimeOffset.UtcNow || campaign.EndDate < DateTimeOffset.UtcNow)
            {
                throw new InvalidOperationException("Kupon kodu geçersiz veya süresi dolmuş.");
            }
        }

        decimal subTotal = 0;
        var details = new List<OrderDetail>();
        foreach (var item in cart.Items)
        {
            if (!item.Product.IsActive)
            {
                throw new InvalidOperationException($"{item.Product.Sku} satışta değil.");
            }
            if (item.Product.Stock < item.Quantity)
            {
                throw new InvalidOperationException($"{item.Product.Sku} için yeterli stok yok.");
            }

            var list = item.Product.ToListDto();
            var unit = list.DiscountedPrice ?? list.Price;
            var line = unit * item.Quantity;
            subTotal += line;
            details.Add(new OrderDetail
            {
                ProductId = item.ProductId,
                ProductName = list.Name,
                ProductSku = item.Product.Sku,
                Quantity = item.Quantity,
                UnitPrice = unit,
                LineTotal = line
            });
            item.Product.Stock -= item.Quantity;
        }

        decimal discount = 0;
        if (campaign is not null)
        {
            if (campaign.MinOrderAmount.HasValue && subTotal < campaign.MinOrderAmount.Value)
            {
                throw new InvalidOperationException($"Bu kupon en az {campaign.MinOrderAmount:0.##} TL sepet tutarında geçerlidir.");
            }

            discount = campaign.DiscountType == DiscountType.Percent
                ? Math.Round(subTotal * campaign.DiscountValue / 100m, 2)
                : Math.Min(subTotal, campaign.DiscountValue);
        }

        var order = new OrderHeader
        {
            OrderNumber = $"TF{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(10, 99)}",
            UserId = _current.UserId,
            CustomerName = request.CustomerName.Trim(),
            CustomerEmail = request.CustomerEmail.Trim().ToLowerInvariant(),
            CustomerPhone = request.CustomerPhone.Trim(),
            ShippingFullName = request.ShippingFullName.Trim(),
            ShippingPhone = request.ShippingPhone.Trim(),
            ShippingCity = request.ShippingCity.Trim(),
            ShippingDistrict = request.ShippingDistrict.Trim(),
            ShippingAddressLine = request.ShippingAddressLine.Trim(),
            ShippingPostalCode = request.ShippingPostalCode?.Trim(),
            SubTotal = subTotal,
            DiscountAmount = discount,
            Total = subTotal - discount,
            CampaignId = campaign?.Id,
            Status = OrderStatus.Pending,
            Notes = request.Notes,
            CreatedAt = DateTimeOffset.UtcNow,
            Details = details
        };

        _db.OrderHeaders.Add(order);
        _db.CartItems.RemoveRange(cart.Items);
        cart.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();

        order = await _db.OrderHeaders.Include(o => o.Details).Include(o => o.Campaign).FirstAsync(o => o.Id == order.Id);
        return order.ToDetailDto();
    }
}
