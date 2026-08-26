using System.ComponentModel.DataAnnotations;
using EndustriB2C.Entities;

namespace EndustriB2C.DTOs;

public class CartDto
{
    public int Id { get; set; }
    public bool IsGuest { get; set; }
    public int ItemCount { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Total { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
}

public class CartItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Quantity { get; set; }
    public int Stock { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class CartItemRequest
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, 999)]
    public int Quantity { get; set; } = 1;
}

public class CheckoutRequest
{
    [Required, MaxLength(160)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(256)]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string CustomerPhone { get; set; } = string.Empty;

    [Required, MaxLength(160)]
    public string ShippingFullName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string ShippingPhone { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string ShippingCity { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string ShippingDistrict { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string ShippingAddressLine { get; set; } = string.Empty;

    [MaxLength(16)]
    public string? ShippingPostalCode { get; set; }

    [MaxLength(40)]
    public string? CouponCode { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}

public class OrderListDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public int ItemCount { get; set; }
}

public class OrderDetailDto : OrderListDto
{
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingFullName { get; set; } = string.Empty;
    public string ShippingPhone { get; set; } = string.Empty;
    public string ShippingCity { get; set; } = string.Empty;
    public string ShippingDistrict { get; set; } = string.Empty;
    public string ShippingAddressLine { get; set; } = string.Empty;
    public string? ShippingPostalCode { get; set; }
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public string? CampaignName { get; set; }
    public string? Notes { get; set; }
    public List<OrderLineDto> Lines { get; set; } = new();
}

public class OrderLineDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string ProductSku { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderStatusRequest
{
    public OrderStatus Status { get; set; }
}

public class CampaignDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public string? CouponCode { get; set; }
    public decimal? MinOrderAmount { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsRunning { get; set; }
}

public class CampaignRequest
{
    [Required, MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DiscountType DiscountType { get; set; }

    [Range(0.01, 999999)]
    public decimal DiscountValue { get; set; }

    [MaxLength(40)]
    public string? CouponCode { get; set; }

    public decimal? MinOrderAmount { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    [MaxLength(500)]
    public string? ImageUrl { get; set; }
}

public static class OrderMaps
{
    public static OrderListDto ToListDto(this OrderHeader o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        CustomerName = o.CustomerName,
        CustomerEmail = o.CustomerEmail,
        Total = o.Total,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        ItemCount = o.Details.Sum(d => d.Quantity)
    };

    public static OrderDetailDto ToDetailDto(this OrderHeader o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        CustomerName = o.CustomerName,
        CustomerEmail = o.CustomerEmail,
        CustomerPhone = o.CustomerPhone,
        Total = o.Total,
        Status = o.Status,
        CreatedAt = o.CreatedAt,
        ItemCount = o.Details.Sum(d => d.Quantity),
        ShippingFullName = o.ShippingFullName,
        ShippingPhone = o.ShippingPhone,
        ShippingCity = o.ShippingCity,
        ShippingDistrict = o.ShippingDistrict,
        ShippingAddressLine = o.ShippingAddressLine,
        ShippingPostalCode = o.ShippingPostalCode,
        SubTotal = o.SubTotal,
        DiscountAmount = o.DiscountAmount,
        CampaignName = o.Campaign?.Name,
        Notes = o.Notes,
        Lines = o.Details.Select(d => new OrderLineDto
        {
            ProductId = d.ProductId,
            ProductName = d.ProductName,
            ProductSku = d.ProductSku,
            Quantity = d.Quantity,
            UnitPrice = d.UnitPrice,
            LineTotal = d.LineTotal
        }).ToList()
    };

    public static CampaignDto ToDto(this Campaign c)
    {
        var now = DateTimeOffset.UtcNow;
        return new CampaignDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            DiscountType = c.DiscountType,
            DiscountValue = c.DiscountValue,
            CouponCode = c.CouponCode,
            MinOrderAmount = c.MinOrderAmount,
            StartDate = c.StartDate,
            EndDate = c.EndDate,
            IsActive = c.IsActive,
            ImageUrl = c.ImageUrl,
            IsRunning = c.IsActive && c.StartDate <= now && c.EndDate >= now
        };
    }
}
