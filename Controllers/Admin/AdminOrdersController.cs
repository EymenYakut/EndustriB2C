using EndustriB2C.Data;
using EndustriB2C.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/orders")]
public class AdminOrdersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminOrdersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderListDto>>> List([FromQuery] byte? status)
    {
        var query = _db.OrderHeaders.AsNoTracking().Include(o => o.Details).AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(o => (byte)o.Status == status.Value);
        }

        var list = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return Ok(list.Select(o => o.ToListDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDetailDto>> Get(int id)
    {
        var order = await _db.OrderHeaders.AsNoTracking()
            .Include(o => o.Details)
            .Include(o => o.Campaign)
            .FirstOrDefaultAsync(o => o.Id == id);
        return order is null ? NotFound() : Ok(order.ToDetailDto());
    }

    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<OrderDetailDto>> UpdateStatus(int id, [FromBody] OrderStatusRequest request)
    {
        var order = await _db.OrderHeaders.Include(o => o.Details).Include(o => o.Campaign).FirstOrDefaultAsync(o => o.Id == id);
        if (order is null) return NotFound();
        order.Status = request.Status;
        order.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(order.ToDetailDto());
    }
}
