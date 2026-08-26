using EndustriB2C.DTOs;
using EndustriB2C.Services;
using Microsoft.AspNetCore.Mvc;

namespace EndustriB2C.Controllers.App;

[ApiController]
[Route("api/cart")]
public class CartController : ControllerBase
{
    private readonly CartService _carts;

    public CartController(CartService carts)
    {
        _carts = carts;
    }

    [HttpGet]
    public async Task<ActionResult<CartDto>> Get() => Ok(await _carts.GetAsync(HttpContext));

    [HttpPost("items")]
    public async Task<ActionResult<CartDto>> Add([FromBody] CartItemRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        try
        {
            return Ok(await _carts.AddAsync(HttpContext, request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("items/{id:int}")]
    public async Task<ActionResult<CartDto>> Update(int id, [FromBody] CartItemRequest request)
    {
        try
        {
            return Ok(await _carts.UpdateQtyAsync(HttpContext, id, request.Quantity));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("items/{id:int}")]
    public async Task<ActionResult<CartDto>> Remove(int id)
    {
        try
        {
            return Ok(await _carts.UpdateQtyAsync(HttpContext, id, 0));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete]
    public async Task<IActionResult> Clear()
    {
        await _carts.ClearAsync(HttpContext);
        return NoContent();
    }
}
