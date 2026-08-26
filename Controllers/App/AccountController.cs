using EndustriB2C.Auth;
using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Controllers.App;

[ApiController]
[Authorize]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _current;

    public AccountController(AppDbContext db, ICurrentUser current)
    {
        _db = db;
        _current = current;
    }

    [HttpGet]
    public async Task<ActionResult<UserDto>> Get()
    {
        var user = await GetUserAsync();
        return user is null ? Unauthorized() : Ok(user.ToDto());
    }

    [HttpGet("addresses")]
    public async Task<ActionResult<IEnumerable<UserAddressDto>>> Addresses()
    {
        if (_current.UserId is not int userId) return Unauthorized();
        var list = await _db.UserAddresses.AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync();
        return Ok(list.Select(a => a.ToDto()));
    }

    [HttpPost("addresses")]
    public async Task<ActionResult<UserAddressDto>> CreateAddress([FromBody] UserAddressRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        if (_current.UserId is not int userId) return Unauthorized();

        if (request.IsDefault)
        {
            await _db.UserAddresses.Where(a => a.UserId == userId).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
        }

        var entity = new UserAddress
        {
            UserId = userId,
            Title = request.Title.Trim(),
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            City = request.City.Trim(),
            District = request.District.Trim(),
            AddressLine = request.AddressLine.Trim(),
            PostalCode = request.PostalCode?.Trim(),
            IsDefault = request.IsDefault
        };
        _db.UserAddresses.Add(entity);
        await _db.SaveChangesAsync();
        return Ok(entity.ToDto());
    }

    [HttpPut("addresses/{id:int}")]
    public async Task<ActionResult<UserAddressDto>> UpdateAddress(int id, [FromBody] UserAddressRequest request)
    {
        if (_current.UserId is not int userId) return Unauthorized();
        var entity = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (entity is null) return NotFound();

        if (request.IsDefault)
        {
            await _db.UserAddresses.Where(a => a.UserId == userId && a.Id != id).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false));
        }

        entity.Title = request.Title.Trim();
        entity.FullName = request.FullName.Trim();
        entity.Phone = request.Phone.Trim();
        entity.City = request.City.Trim();
        entity.District = request.District.Trim();
        entity.AddressLine = request.AddressLine.Trim();
        entity.PostalCode = request.PostalCode?.Trim();
        entity.IsDefault = request.IsDefault;
        await _db.SaveChangesAsync();
        return Ok(entity.ToDto());
    }

    [HttpDelete("addresses/{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        if (_current.UserId is not int userId) return Unauthorized();
        var entity = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
        if (entity is null) return NotFound();
        _db.UserAddresses.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private Task<User?> GetUserAsync()
    {
        if (_current.UserId is not int userId) return Task.FromResult<User?>(null);
        return _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }
}
