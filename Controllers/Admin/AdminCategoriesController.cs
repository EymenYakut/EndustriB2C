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
[Route("api/admin/categories")]
public class AdminCategoriesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FileStorageService _files;

    public AdminCategoriesController(AppDbContext db, FileStorageService files)
    {
        _db = db;
        _files = files;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryDto>>> List()
    {
        var list = await _db.Categories.AsNoTracking()
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
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var entity = new Category { CreatedAt = DateTimeOffset.UtcNow };
        Apply(entity, request);
        if (await _db.Categories.AnyAsync(c => c.Slug == entity.Slug))
        {
            return BadRequest(new { message = "Bu kategori slug zaten kullanılıyor." });
        }
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(entity.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoryDto>> Update(int id, [FromBody] CategoryRequest request)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity is null) return NotFound();
        Apply(entity, request);
        if (await _db.Categories.AnyAsync(c => c.Slug == entity.Slug && c.Id != id))
        {
            return BadRequest(new { message = "Bu kategori slug zaten kullanılıyor." });
        }
        await _db.SaveChangesAsync();
        return Ok(entity.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity is null) return NotFound();
        if (await _db.ProductCategories.AnyAsync(pc => pc.CategoryId == id))
        {
            return BadRequest(new { message = "Kategori ürünlerde kullanıldığı için silinemez." });
        }
        _files.DeleteIfLocal(entity.ImageUrl);
        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/image")]
    public async Task<ActionResult<CategoryDto>> Upload(int id, IFormFile file)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity is null) return NotFound();
        if (file is null) return BadRequest(new { message = "Dosya gerekli." });
        try
        {
            _files.DeleteIfLocal(entity.ImageUrl);
            entity.ImageUrl = await _files.SaveCategoryImageAsync(file);
            await _db.SaveChangesAsync();
            return Ok(entity.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static void Apply(Category entity, CategoryRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.Slug = string.IsNullOrWhiteSpace(request.Slug)
            ? FileStorageService.Slugify(entity.Name)
            : FileStorageService.Slugify(request.Slug);
        entity.Description = request.Description?.Trim();
        entity.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? entity.ImageUrl : request.ImageUrl.Trim();
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
    }
}
