using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/features")]
public class AdminFeaturesController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminFeaturesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeatureHeaderDto>>> List()
    {
        var list = await _db.ProductFeaturesHeaders.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync();
        return Ok(list.Select(x => new FeatureHeaderDto
        {
            Id = x.Id,
            Name = x.Name,
            Unit = x.Unit,
            SortOrder = x.SortOrder
        }));
    }

    [HttpPost]
    public async Task<ActionResult<FeatureHeaderDto>> Create([FromBody] FeatureHeaderRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var entity = new ProductFeaturesHeader
        {
            Name = request.Name.Trim(),
            Unit = request.Unit?.Trim(),
            SortOrder = request.SortOrder
        };
        _db.ProductFeaturesHeaders.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(new FeatureHeaderDto { Id = entity.Id, Name = entity.Name, Unit = entity.Unit, SortOrder = entity.SortOrder });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<FeatureHeaderDto>> Update(int id, [FromBody] FeatureHeaderRequest request)
    {
        var entity = await _db.ProductFeaturesHeaders.FindAsync(id);
        if (entity is null) return NotFound();
        entity.Name = request.Name.Trim();
        entity.Unit = request.Unit?.Trim();
        entity.SortOrder = request.SortOrder;
        await _db.SaveChangesAsync();
        return Ok(new FeatureHeaderDto { Id = entity.Id, Name = entity.Name, Unit = entity.Unit, SortOrder = entity.SortOrder });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.ProductFeaturesHeaders.FindAsync(id);
        if (entity is null) return NotFound();
        if (await _db.ProductFeaturesDetails.AnyAsync(d => d.ProductFeaturesHeaderId == id))
        {
            return BadRequest(new { message = "Bu özellik ürünlerde kullanıldığı için silinemez." });
        }
        _db.ProductFeaturesHeaders.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
