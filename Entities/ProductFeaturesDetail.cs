namespace EndustriB2C.Entities;

public class ProductFeaturesDetail
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ProductFeaturesHeaderId { get; set; }
    public string Value { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
    public ProductFeaturesHeader Header { get; set; } = null!;
}
