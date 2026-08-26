using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using EndustriB2C.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/products")]
public class AdminProductsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FileStorageService _files;

    public AdminProductsController(AppDbContext db, FileStorageService files)
    {
        _db = db;
        _files = files;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDetailDto>>> List()
    {
        var list = await Query().OrderBy(p => p.SortOrder).ToListAsync();
        return Ok(list.Select(p => p.ToDetailDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Get(int id)
    {
        var product = await Query().FirstOrDefaultAsync(p => p.Id == id);
        return product is null ? NotFound() : Ok(product.ToDetailDto());
    }

    [HttpPost]
    public async Task<ActionResult<ProductDetailDto>> Create([FromBody] ProductSaveRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (await _db.Products.AnyAsync(p => p.Sku == request.Sku))
        {
            return BadRequest(new { message = "SKU zaten kullanılıyor." });
        }

        var product = new Product();
        Apply(product, request);
        _db.Products.Add(product);
        await _db.SaveChangesAsync();
        product = await Query().FirstAsync(p => p.Id == product.Id);
        return Ok(product.ToDetailDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductDetailDto>> Update(int id, [FromBody] ProductSaveRequest request)
    {
        var product = await Query().FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        if (await _db.Products.AnyAsync(p => p.Sku == request.Sku && p.Id != id))
        {
            return BadRequest(new { message = "SKU zaten kullanılıyor." });
        }

        Apply(product, request);
        product.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        product = await Query().FirstAsync(p => p.Id == id);
        return Ok(product.ToDetailDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        if (await _db.OrderDetails.AnyAsync(d => d.ProductId == id))
        {
            product.IsActive = false;
            await _db.SaveChangesAsync();
            return Ok(new { message = "Ürünün siparişi olduğu için pasife alındı." });
        }

        foreach (var img in product.Images)
        {
            _files.DeleteIfLocal(img.FilePath);
        }

        _db.Products.Remove(product);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/images")]
    public async Task<ActionResult<ProductImageDto>> Upload(int id, IFormFile file, [FromForm] bool isPrimary = false)
    {
        var product = await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id);
        if (product is null) return NotFound();
        if (file is null) return BadRequest(new { message = "Dosya gerekli." });

        try
        {
            var path = await _files.SaveProductImageAsync(file);
            if (isPrimary)
            {
                foreach (var img in product.Images) img.IsPrimary = false;
            }

            var entity = new ProductImage
            {
                ProductId = id,
                FilePath = path,
                AltText = file.FileName,
                SortOrder = product.Images.Count,
                IsPrimary = isPrimary || product.Images.Count == 0
            };
            _db.ProductImages.Add(entity);
            await _db.SaveChangesAsync();
            return Ok(new ProductImageDto
            {
                Id = entity.Id,
                FilePath = entity.FilePath,
                AltText = entity.AltText,
                SortOrder = entity.SortOrder,
                IsPrimary = entity.IsPrimary
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}/images/{imageId:int}")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var image = await _db.ProductImages.FirstOrDefaultAsync(i => i.Id == imageId && i.ProductId == id);
        if (image is null) return NotFound();
        _files.DeleteIfLocal(image.FilePath);
        _db.ProductImages.Remove(image);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<Product> Query() =>
        _db.Products
            .Include(p => p.Translations)
            .Include(p => p.Prices).ThenInclude(pr => pr.Campaign)
            .Include(p => p.Images)
            .Include(p => p.Features).ThenInclude(f => f.Header)
            .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category);

    private void Apply(Product product, ProductSaveRequest request)
    {
        product.Sku = request.Sku.Trim();
        product.Manufacturer = request.Manufacturer?.Trim() ?? string.Empty;
        product.Stock = request.Stock;
        product.IsActive = request.IsActive;
        product.SortOrder = request.SortOrder;

        var categoryIds = (request.CategoryIds ?? new List<int>()).Where(id => id > 0).Distinct().ToList();
        product.ProductCategories.Clear();
        foreach (var id in categoryIds)
        {
            product.ProductCategories.Add(new ProductCategory { CategoryId = id });
        }

        var firstSlug = _db.Categories.AsNoTracking()
            .Where(c => categoryIds.Contains(c.Id))
            .OrderBy(c => c.SortOrder)
            .Select(c => c.Slug)
            .FirstOrDefault();
        product.Category = firstSlug ?? request.Category?.Trim() ?? string.Empty;

        product.Translations.Clear();
        foreach (var t in request.Translations)
        {
            var name = t.Name.Trim();
            product.Translations.Add(new ProductTranslation
            {
                LanguageCode = string.IsNullOrWhiteSpace(t.LanguageCode) ? "tr" : t.LanguageCode.Trim(),
                Name = name,
                ShortDescription = t.ShortDescription?.Trim(),
                Description = t.Description?.Trim(),
                Slug = string.IsNullOrWhiteSpace(t.Slug) ? FileStorageService.Slugify(name) : t.Slug.Trim()
            });
        }

        product.Prices.Clear();
        foreach (var p in request.Prices)
        {
            product.Prices.Add(new ProductPrice
            {
                Amount = p.Amount,
                Currency = string.IsNullOrWhiteSpace(p.Currency) ? "TRY" : p.Currency,
                CampaignId = p.CampaignId,
                ValidFrom = p.ValidFrom,
                ValidTo = p.ValidTo,
                IsCurrent = p.IsCurrent
            });
        }

        product.Features.Clear();
        foreach (var f in request.Features.Where(x => x.HeaderId > 0 && !string.IsNullOrWhiteSpace(x.Value)))
        {
            product.Features.Add(new ProductFeaturesDetail
            {
                ProductFeaturesHeaderId = f.HeaderId,
                Value = f.Value.Trim()
            });
        }
    }
}
