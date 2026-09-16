using EndustriB2C.Data;
using EndustriB2C.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.App;

[ApiController]
[Route("api/sliders")]
public class SlidersController : ControllerBase
{
    private readonly AppDbContext _db;

    public SlidersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SliderDto>>> Active()
    {
        var list = await _db.HomeSliders.AsNoTracking()
            .Where(s => s.IsActive)
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Id)
            .ToListAsync();
        return Ok(list.Select(s => s.ToDto()));
    }
}
