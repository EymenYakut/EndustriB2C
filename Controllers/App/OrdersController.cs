using EndustriB2C.Auth;
using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.App;

[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orders;
    private readonly AppDbContext _db;
    private readonly ICurrentUser _current;

    public OrdersController(OrderService orders, AppDbContext db, ICurrentUser current)
    {
        _orders = orders;
        _db = db;
        _current = current;
    }

    [HttpPost("checkout")]
    public async Task<ActionResult<OrderDetailDto>> Checkout([FromBody] CheckoutRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            var order = await _orders.CheckoutAsync(HttpContext, request);
            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<OrderListDto>>> Mine()
    {
        if (_current.UserId is not int userId) return Unauthorized();
        var orders = await _db.OrderHeaders.AsNoTracking()
            .Include(o => o.Details)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
        return Ok(orders.Select(o => o.ToListDto()));
    }

    [Authorize]
    [HttpGet("mine/{id:int}")]
    public async Task<ActionResult<OrderDetailDto>> MineDetail(int id)
    {
        if (_current.UserId is not int userId) return Unauthorized();
        var order = await _db.OrderHeaders.AsNoTracking()
            .Include(o => o.Details)
            .Include(o => o.Campaign)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);
        return order is null ? NotFound() : Ok(order.ToDetailDto());
    }
}
