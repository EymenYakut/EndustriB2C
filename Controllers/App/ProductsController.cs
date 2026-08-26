using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.App;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductListDto>>> List([FromQuery] string? category, [FromQuery] string? q, [FromQuery] string lang = "tr")
    {
        var query = _db.Products.AsNoTracking()
            .Include(p => p.Translations)
            .Include(p => p.Prices).ThenInclude(pr => pr.Campaign)
            .Include(p => p.Images)
            .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
        {
            query = query.Where(p =>
                p.Category == category ||
                p.ProductCategories.Any(pc => pc.Category.Slug == category));
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                p.Sku.Contains(term) ||
                p.Manufacturer.Contains(term) ||
                p.Translations.Any(t => t.Name.Contains(term) || (t.ShortDescription != null && t.ShortDescription.Contains(term))));
        }

        var list = await query.OrderBy(p => p.SortOrder).ThenBy(p => p.Id).ToListAsync();
        return Ok(list.Select(p => p.ToListDto(lang)));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Get(int id, [FromQuery] string lang = "tr")
    {
        var product = await Query().FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        return product is null ? NotFound() : Ok(product.ToDetailDto(lang));
    }

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<ProductDetailDto>> GetBySlug(string slug, [FromQuery] string lang = "tr")
    {
        var product = await Query().FirstOrDefaultAsync(p => p.IsActive && p.Translations.Any(t => t.Slug == slug));
        return product is null ? NotFound() : Ok(product.ToDetailDto(lang));
    }

    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> Categories()
    {
        var cats = await _db.Categories.AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.SortOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                ProductCount = c.ProductCategories.Count
            })
            .ToListAsync();
        return Ok(cats);
    }

    private IQueryable<Product> Query() =>
        _db.Products.AsNoTracking()
            .Include(p => p.Translations)
            .Include(p => p.Prices).ThenInclude(pr => pr.Campaign)
            .Include(p => p.Images)
            .Include(p => p.Features).ThenInclude(f => f.Header)
            .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category);
}
