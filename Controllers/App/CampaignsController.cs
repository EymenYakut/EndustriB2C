using EndustriB2C.Data;
using EndustriB2C.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.App;

[ApiController]
[Route("api/campaigns")]
public class CampaignsController : ControllerBase
{
    private readonly AppDbContext _db;

    public CampaignsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CampaignDto>>> Active()
    {
        var now = DateTimeOffset.UtcNow;
        var list = await _db.Campaigns.AsNoTracking()
            .Where(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
            .OrderByDescending(c => c.StartDate)
            .ToListAsync();
        return Ok(list.Select(c => c.ToDto()));
    }
}
