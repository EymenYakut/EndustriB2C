namespace EndustriB2C.Entities;

public class ProductTranslation
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string LanguageCode { get; set; } = "tr";
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
