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
            .Include(c => c.MainCategory)
            .OrderByDescending(c => c.IsMainCategory)
            .ThenBy(c => c.MainCategoryId)
            .ThenBy(c => c.SortOrder)
            .Select(c => new CategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                Description = c.Description,
                ImageUrl = c.ImageUrl,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                IsMainCategory = c.IsMainCategory,
                MainCategoryId = c.MainCategoryId,
                MainCategoryName = c.MainCategory != null ? c.MainCategory.Name : null,
                ProductCount = c.ProductCategories.Count
            })
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create([FromBody] CategoryRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var hierarchyError = await ValidateHierarchyAsync(request);
        if (hierarchyError is not null) return BadRequest(new { message = hierarchyError });
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
        var hierarchyError = await ValidateHierarchyAsync(request, id);
        if (hierarchyError is not null) return BadRequest(new { message = hierarchyError });
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
        if (await _db.Categories.AnyAsync(c => c.MainCategoryId == id))
        {
            return BadRequest(new { message = "Alt kategorileri olan ana kategori silinemez." });
        }
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
        entity.IsMainCategory = request.IsMainCategory;
        entity.MainCategoryId = request.IsMainCategory ? null : request.MainCategoryId;
    }

    private async Task<string?> ValidateHierarchyAsync(CategoryRequest request, int? id = null)
    {
        if (request.IsMainCategory)
        {
            return null;
        }

        if (request.MainCategoryId is not int parentId || parentId <= 0)
        {
            return "Alt kategori için ana kategori seçilmelidir.";
        }

        if (id.HasValue && parentId == id.Value)
        {
            return "Kategori kendisine bağlanamaz.";
        }

        var parent = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == parentId);
        if (parent is null)
        {
            return "Ana kategori bulunamadı.";
        }

        if (!parent.IsMainCategory)
        {
            return "Alt kategori yalnızca bir ana kategoriye bağlanabilir.";
        }

        if (id.HasValue && await _db.Categories.AnyAsync(c => c.MainCategoryId == id.Value))
        {
            return "Alt kategorisi olan kayıt ana kategori olarak kalmalıdır.";
        }

        return null;
    }
}
