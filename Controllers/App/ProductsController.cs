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
    public async Task<ActionResult<PagedResult<ProductListDto>>> List(
        [FromQuery] string? category,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string lang = "tr")
    {
        var query = _db.Products.AsNoTracking()
            .Include(p => p.Translations)
            .Include(p => p.Prices).ThenInclude(pr => pr.Campaign)
            .Include(p => p.Images)
            .Include(p => p.Features).ThenInclude(f => f.Header)
            .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
            .Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
        {
            var selected = await _db.Categories.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Slug == category && c.IsActive);
            if (selected is null)
            {
                query = query.Where(p =>
                    p.Category == category ||
                    p.ProductCategories.Any(pc => pc.Category.Slug == category));
            }
            else
            {
                var categoryIds = new List<int> { selected.Id };
                if (selected.IsMainCategory)
                {
                    categoryIds.AddRange(await _db.Categories.AsNoTracking()
                        .Where(c => c.MainCategoryId == selected.Id && c.IsActive)
                        .Select(c => c.Id)
                        .ToListAsync());
                }

                query = query.Where(p =>
                    p.Category == category ||
                    p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
            }
        }

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(p =>
                p.Sku.Contains(term) ||
                p.Manufacturer.Contains(term) ||
                p.Translations.Any(t => t.Name.Contains(term) || (t.ShortDescription != null && t.ShortDescription.Contains(term))));
        }

        var total = await query.CountAsync();
        var safeSize = Math.Clamp(pageSize, 1, 48);
        var safePage = Math.Max(1, page);
        var items = await query
            .OrderBy(p => p.SortOrder)
            .ThenBy(p => p.Id)
            .Skip((safePage - 1) * safeSize)
            .Take(safeSize)
            .ToListAsync();

        return Ok(new PagedResult<ProductListDto>
        {
            Items = items.Select(p => p.ToListDto(lang)).ToList(),
            Total = total,
            Page = safePage,
            PageSize = safeSize
        });
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
            .ThenBy(c => c.Name)
            .ToListAsync();
        var links = await _db.ProductCategories.AsNoTracking()
            .Where(pc => pc.Product.IsActive)
            .Select(pc => new { pc.CategoryId, pc.ProductId })
            .ToListAsync();
        var byCategory = links
            .GroupBy(x => x.CategoryId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.ProductId).ToHashSet());

        CategoryDto Map(Category c)
        {
            var dto = c.ToDto();
            dto.MainCategoryName = cats.FirstOrDefault(x => x.Id == c.MainCategoryId)?.Name;
            return dto;
        }

        var tree = cats.Where(c => c.IsMainCategory).Select(main =>
        {
            var dto = Map(main);
            var children = cats.Where(c => !c.IsMainCategory && c.MainCategoryId == main.Id).Select(Map).ToList();
            dto.Children = children;
            var productIds = new HashSet<int>();
            if (byCategory.TryGetValue(main.Id, out var own)) productIds.UnionWith(own);
            foreach (var child in children)
            {
                if (byCategory.TryGetValue(child.Id, out var childIds))
                {
                    child.ProductCount = childIds.Count;
                    productIds.UnionWith(childIds);
                }
            }
            dto.ProductCount = productIds.Count;
            return dto;
        }).ToList();

        return Ok(tree);
    }

    private IQueryable<Product> Query() =>
        _db.Products.AsNoTracking()
            .Include(p => p.Translations)
            .Include(p => p.Prices).ThenInclude(pr => pr.Campaign)
            .Include(p => p.Images)
            .Include(p => p.Features).ThenInclude(f => f.Header)
            .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category);
}
