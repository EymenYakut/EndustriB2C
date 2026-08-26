namespace EndustriB2C.Entities;

public class ProductFeaturesHeader
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public int SortOrder { get; set; }

    public ICollection<ProductFeaturesDetail> Details { get; set; } = new List<ProductFeaturesDetail>();
}
