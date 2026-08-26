using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/customers")]
public class AdminCustomersController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminCustomersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> List()
    {
        var users = await _db.Users.AsNoTracking()
            .Where(u => u.UserType == UserType.User)
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();
        return Ok(users.Select(u => u.ToDto()));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Get(int id)
    {
        var user = await _db.Users.AsNoTracking()
            .Include(u => u.Addresses)
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();

        var orders = await _db.OrderHeaders.AsNoTracking()
            .Include(o => o.Details)
            .Where(o => o.UserId == id)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return Ok(new
        {
            user = user.ToDto(),
            addresses = user.Addresses.Select(a => a.ToDto()),
            orders = orders.Select(o => o.ToListDto())
        });
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UserDto>> Update(int id, [FromBody] AdminUserUpdateRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user is null) return NotFound();
        user.FirstName = request.FirstName.Trim();
        user.LastName = request.LastName.Trim();
        user.Phone = request.Phone.Trim();
        user.UserType = request.UserType;
        user.IsActive = request.IsActive;
        await _db.SaveChangesAsync();
        return Ok(user.ToDto());
    }
}
