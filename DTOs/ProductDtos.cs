using System.ComponentModel.DataAnnotations;
using EndustriB2C.Entities;

namespace EndustriB2C.DTOs;

public class ProductListDto
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Manufacturer { get; set; } = string.Empty;
    public List<CategoryDto> Categories { get; set; } = new();
    public List<int> CategoryIds { get; set; } = new();
    public int Stock { get; set; }
    public bool IsActive { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public string Currency { get; set; } = "TRY";
    public string? ImageUrl { get; set; }
    public string? CampaignName { get; set; }
}

public class ProductDetailDto : ProductListDto
{
    public string? Description { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
    public List<ProductFeatureValueDto> Features { get; set; } = new();
    public List<ProductPriceDto> Prices { get; set; } = new();
    public List<ProductTranslationDto> Translations { get; set; } = new();
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public int SortOrder { get; set; }
    public bool IsPrimary { get; set; }
}

public class ProductFeatureValueDto
{
    public int Id { get; set; }
    public int HeaderId { get; set; }
    public string HeaderName { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class ProductPriceDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "TRY";
    public int? CampaignId { get; set; }
    public string? CampaignName { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsCurrent { get; set; }
}

public class ProductTranslationDto
{
    public int Id { get; set; }
    public string LanguageCode { get; set; } = "tr";
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string Slug { get; set; } = string.Empty;
}

public class ProductSaveRequest
{
    [Required, MaxLength(50)]
    public string Sku { get; set; } = string.Empty;

    [MaxLength(120)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(160)]
    public string Manufacturer { get; set; } = string.Empty;

    public List<int> CategoryIds { get; set; } = new();

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }

    [MinLength(1)]
    public List<ProductTranslationDto> Translations { get; set; } = new();

    [MinLength(1)]
    public List<ProductPriceSaveDto> Prices { get; set; } = new();

    public List<ProductFeatureSaveDto> Features { get; set; } = new();
}

public class ProductPriceSaveDto
{
    [Range(0.01, 9999999)]
    public decimal Amount { get; set; }

    [MaxLength(3)]
    public string Currency { get; set; } = "TRY";

    public int? CampaignId { get; set; }
    public DateTimeOffset? ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsCurrent { get; set; } = true;
}

public class ProductFeatureSaveDto
{
    [Required]
    public int HeaderId { get; set; }

    [Required, MaxLength(300)]
    public string Value { get; set; } = string.Empty;
}

public class FeatureHeaderDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Unit { get; set; }
    public int SortOrder { get; set; }
}

public class FeatureHeaderRequest
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Unit { get; set; }

    public int SortOrder { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

public class CategoryRequest
{
    [Required, MaxLength(160)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(160)]
    public string? Slug { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageUrl { get; set; }

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public static class CategoryMaps
{
    public static CategoryDto ToDto(this Category c, int productCount = 0) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        SortOrder = c.SortOrder,
        IsActive = c.IsActive,
        ProductCount = productCount
    };
}

public static class ProductMaps
{
    public static ProductListDto ToListDto(this Product p, string lang = "tr")
    {
        var tr = p.Translations.FirstOrDefault(t => t.LanguageCode == lang)
                 ?? p.Translations.FirstOrDefault();
        var price = p.Prices.Where(x => x.IsCurrent).OrderByDescending(x => x.Id).FirstOrDefault();
        var image = p.Images.OrderByDescending(x => x.IsPrimary).ThenBy(x => x.SortOrder).FirstOrDefault();
        decimal? discounted = null;
        string? campaignName = null;
        if (price?.Campaign is { IsActive: true } c && c.StartDate <= DateTimeOffset.UtcNow && c.EndDate >= DateTimeOffset.UtcNow)
        {
            campaignName = c.Name;
            discounted = c.DiscountType == DiscountType.Percent
                ? Math.Round(price.Amount * (1 - c.DiscountValue / 100m), 2)
                : Math.Max(0, price.Amount - c.DiscountValue);
        }

        return new ProductListDto
        {
            Id = p.Id,
            Sku = p.Sku,
            Category = p.ProductCategories.Select(pc => pc.Category?.Slug).FirstOrDefault(s => !string.IsNullOrWhiteSpace(s)) ?? p.Category,
            Manufacturer = p.Manufacturer,
            Categories = p.ProductCategories
                .Where(pc => pc.Category != null)
                .OrderBy(pc => pc.Category.SortOrder)
                .Select(pc => pc.Category.ToDto())
                .ToList(),
            CategoryIds = p.ProductCategories.Select(pc => pc.CategoryId).ToList(),
            Stock = p.Stock,
            IsActive = p.IsActive,
            Name = tr?.Name ?? p.Sku,
            ShortDescription = tr?.ShortDescription,
            Slug = tr?.Slug ?? p.Sku.ToLowerInvariant(),
            Price = price?.Amount ?? 0,
            DiscountedPrice = discounted,
            Currency = price?.Currency ?? "TRY",
            ImageUrl = image?.FilePath,
            CampaignName = campaignName
        };
    }

    public static ProductDetailDto ToDetailDto(this Product p, string lang = "tr")
    {
        var list = p.ToListDto(lang);
        var tr = p.Translations.FirstOrDefault(t => t.LanguageCode == lang)
                 ?? p.Translations.FirstOrDefault();
        return new ProductDetailDto
        {
            Id = list.Id,
            Sku = list.Sku,
            Category = list.Category,
            Stock = list.Stock,
            IsActive = list.IsActive,
            Name = list.Name,
            ShortDescription = list.ShortDescription,
            Slug = list.Slug,
            Price = list.Price,
            DiscountedPrice = list.DiscountedPrice,
            Currency = list.Currency,
            ImageUrl = list.ImageUrl,
            CampaignName = list.CampaignName,
            Manufacturer = list.Manufacturer,
            Categories = list.Categories,
            CategoryIds = list.CategoryIds,
            Description = tr?.Description,
            Images = p.Images.OrderBy(x => x.SortOrder).Select(x => new ProductImageDto
            {
                Id = x.Id,
                FilePath = x.FilePath,
                AltText = x.AltText,
                SortOrder = x.SortOrder,
                IsPrimary = x.IsPrimary
            }).ToList(),
            Features = p.Features.OrderBy(x => x.Header.SortOrder).Select(x => new ProductFeatureValueDto
            {
                Id = x.Id,
                HeaderId = x.ProductFeaturesHeaderId,
                HeaderName = x.Header.Name,
                Unit = x.Header.Unit,
                Value = x.Value
            }).ToList(),
            Prices = p.Prices.Select(x => new ProductPriceDto
            {
                Id = x.Id,
                Amount = x.Amount,
                Currency = x.Currency,
                CampaignId = x.CampaignId,
                CampaignName = x.Campaign?.Name,
                ValidFrom = x.ValidFrom,
                ValidTo = x.ValidTo,
                IsCurrent = x.IsCurrent
            }).ToList(),
            Translations = p.Translations.Select(x => new ProductTranslationDto
            {
                Id = x.Id,
                LanguageCode = x.LanguageCode,
                Name = x.Name,
                ShortDescription = x.ShortDescription,
                Description = x.Description,
                Slug = x.Slug
            }).ToList()
        };
    }
}
