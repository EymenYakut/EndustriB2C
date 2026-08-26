using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminUsersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> List()
    {
        var users = await _db.Users.AsNoTracking().OrderBy(u => u.UserType).ThenBy(u => u.Email).ToListAsync();
        return Ok(users.Select(u => u.ToDto()));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult> Dashboard()
    {
        var now = DateTimeOffset.UtcNow;
        return Ok(new
        {
            customers = await _db.Users.CountAsync(u => u.UserType == UserType.User),
            products = await _db.Products.CountAsync(p => p.IsActive),
            orders = await _db.OrderHeaders.CountAsync(),
            pendingOrders = await _db.OrderHeaders.CountAsync(o => o.Status == OrderStatus.Pending),
            revenue = await _db.OrderHeaders.Where(o => o.Status != OrderStatus.Cancelled).SumAsync(o => (decimal?)o.Total) ?? 0,
            activeCampaigns = await _db.Campaigns.CountAsync(c => c.IsActive && c.StartDate <= now && c.EndDate >= now)
        });
    }
}
