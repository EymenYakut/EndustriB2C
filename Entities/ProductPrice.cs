namespace EndustriB2C.Entities;

public class ProductPrice
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? CampaignId { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsCurrent { get; set; } = true;

    public Product Product { get; set; } = null!;
    public Campaign? Campaign { get; set; }
}
