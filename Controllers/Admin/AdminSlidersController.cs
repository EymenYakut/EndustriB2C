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
[Route("api/admin/sliders")]
public class AdminSlidersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly FileStorageService _files;

    public AdminSlidersController(AppDbContext db, FileStorageService files)
    {
        _db = db;
        _files = files;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SliderDto>>> List()
    {
        var list = await _db.HomeSliders.AsNoTracking()
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .ToListAsync();
        return Ok(list.Select(s => s.ToDto()));
    }

    [HttpPost]
    public async Task<ActionResult<SliderDto>> Create([FromBody] SliderRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var entity = new HomeSlider { CreatedAt = DateTimeOffset.UtcNow };
        Apply(entity, request);
        _db.HomeSliders.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(entity.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<SliderDto>> Update(int id, [FromBody] SliderRequest request)
    {
        var entity = await _db.HomeSliders.FindAsync(id);
        if (entity is null) return NotFound();
        Apply(entity, request);
        await _db.SaveChangesAsync();
        return Ok(entity.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.HomeSliders.FindAsync(id);
        if (entity is null) return NotFound();
        _files.DeleteIfLocal(entity.ImageUrl);
        _db.HomeSliders.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/image")]
    public async Task<ActionResult<SliderDto>> Upload(int id, IFormFile file)
    {
        var entity = await _db.HomeSliders.FindAsync(id);
        if (entity is null) return NotFound();
        if (file is null) return BadRequest(new { message = "Dosya gerekli." });
        try
        {
            _files.DeleteIfLocal(entity.ImageUrl);
            entity.ImageUrl = await _files.SaveSliderImageAsync(file);
            await _db.SaveChangesAsync();
            return Ok(entity.ToDto());
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private static void Apply(HomeSlider entity, SliderRequest request)
    {
        entity.Title = request.Title.Trim();
        entity.Subtitle = string.IsNullOrWhiteSpace(request.Subtitle) ? null : request.Subtitle.Trim();
        entity.ButtonText = string.IsNullOrWhiteSpace(request.ButtonText) ? null : request.ButtonText.Trim();
        entity.ButtonUrl = string.IsNullOrWhiteSpace(request.ButtonUrl) ? null : request.ButtonUrl.Trim();
        if (!string.IsNullOrWhiteSpace(request.ImageUrl))
        {
            entity.ImageUrl = request.ImageUrl.Trim();
        }
        entity.SortOrder = request.SortOrder;
        entity.IsActive = request.IsActive;
    }
}
