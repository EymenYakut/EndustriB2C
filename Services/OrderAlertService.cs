using System.Globalization;
using System.Text;
using EndustriB2C.DTOs;
using Microsoft.Extensions.Options;

namespace EndustriB2C.Services;

public class OrderAlertOptions
{
    public bool Enabled { get; set; } = true;
    public string Phone { get; set; } = "905534380409";
}

public class OrderAlertService
{
    private static readonly CultureInfo Tr = CultureInfo.GetCultureInfo("tr-TR");
    private readonly OrderAlertOptions _options;

    public OrderAlertService(IOptions<OrderAlertOptions> options)
    {
        _options = options.Value;
    }

    public string? BuildWhatsAppUrl(OrderDetailDto order)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        var phone = NormalizePhone(_options.Phone);
        var text = Uri.EscapeDataString(BuildMessage(order));
        return $"https://wa.me/{phone}?text={text}";
    }

    private static string BuildMessage(OrderDetailDto order)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Yakut Hortum yeni sipariş");
        sb.AppendLine($"No: {order.OrderNumber}");
        sb.AppendLine($"Tutar: {order.Total.ToString("N2", Tr)} TRY");
        if (order.DiscountAmount > 0)
        {
            sb.AppendLine($"İndirim: {order.DiscountAmount.ToString("N2", Tr)} TRY");
        }
        sb.AppendLine();
        sb.AppendLine("Müşteri");
        sb.AppendLine(order.CustomerName);
        sb.AppendLine($"Tel: {order.CustomerPhone}");
        sb.AppendLine(order.CustomerEmail);
        sb.AppendLine();
        sb.AppendLine("Gönderim");
        sb.AppendLine($"{order.ShippingFullName} / {order.ShippingPhone}");
        sb.AppendLine(order.ShippingAddressLine);
        sb.AppendLine($"{order.ShippingDistrict} / {order.ShippingCity}");
        if (!string.IsNullOrWhiteSpace(order.ShippingPostalCode))
        {
            sb.AppendLine($"PK: {order.ShippingPostalCode}");
        }
        sb.AppendLine();
        sb.AppendLine("Ürünler");
        foreach (var line in order.Lines)
        {
            sb.AppendLine($"- {line.Quantity} x {line.ProductName} ({line.ProductSku}) {line.LineTotal.ToString("N2", Tr)} TRY");
        }
        if (!string.IsNullOrWhiteSpace(order.Notes))
        {
            sb.AppendLine();
            sb.AppendLine($"Not: {order.Notes}");
        }

        var text = sb.ToString().Trim();
        return text.Length > 1600 ? text[..1597] + "..." : text;
    }

    private static string NormalizePhone(string phone)
    {
        var digits = new string((phone ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.StartsWith("90", StringComparison.Ordinal) && digits.Length >= 12)
        {
            return digits;
        }
        if (digits.StartsWith("0", StringComparison.Ordinal) && digits.Length == 11)
        {
            return "90" + digits[1..];
        }
        if (digits.Length == 10)
        {
            return "90" + digits;
        }
        return string.IsNullOrEmpty(digits) ? "905534380409" : digits;
    }
}
