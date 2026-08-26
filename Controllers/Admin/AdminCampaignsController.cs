using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/campaigns")]
public class AdminCampaignsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminCampaignsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampaignDto>>> List()
    {
        var list = await _db.Campaigns.AsNoTracking().OrderByDescending(c => c.CreatedAt).ToListAsync();
        return Ok(list.Select(c => c.ToDto()));
    }

    [HttpPost]
    public async Task<ActionResult<CampaignDto>> Create([FromBody] CampaignRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var entity = new Campaign();
        Apply(entity, request);
        entity.CreatedAt = DateTimeOffset.UtcNow;
        _db.Campaigns.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(entity.ToDto());
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CampaignDto>> Update(int id, [FromBody] CampaignRequest request)
    {
        var entity = await _db.Campaigns.FindAsync(id);
        if (entity is null) return NotFound();
        Apply(entity, request);
        await _db.SaveChangesAsync();
        return Ok(entity.ToDto());
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Campaigns.FindAsync(id);
        if (entity is null) return NotFound();
        _db.Campaigns.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private static void Apply(Campaign entity, CampaignRequest request)
    {
        entity.Name = request.Name.Trim();
        entity.Description = request.Description?.Trim();
        entity.DiscountType = request.DiscountType;
        entity.DiscountValue = request.DiscountValue;
        entity.CouponCode = string.IsNullOrWhiteSpace(request.CouponCode) ? null : request.CouponCode.Trim().ToUpperInvariant();
        entity.MinOrderAmount = request.MinOrderAmount;
        entity.StartDate = request.StartDate;
        entity.EndDate = request.EndDate;
        entity.IsActive = request.IsActive;
        entity.ImageUrl = request.ImageUrl;
    }
}
