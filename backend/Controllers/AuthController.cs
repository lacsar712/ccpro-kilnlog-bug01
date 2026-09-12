using KilnLog.Api.Data;
using KilnLog.Api.Dtos;
using KilnLog.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KilnLog.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;

    public AuthController(AppDbContext db, JwtTokenService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest(new { message = "用户名和密码不能为空" });

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
        if (user is null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Unauthorized(new { message = "用户名或密码错误" });

        var token = _jwt.CreateToken(user);
        return Ok(new LoginResponse(token, new UserDto(user.Id, user.Username, user.Role)));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> Me()
    {
        var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var id))
            return Unauthorized(new { message = "无效令牌" });

        var user = await _db.Users.FindAsync(id);
        if (user is null) return Unauthorized(new { message = "用户不存在" });
        return Ok(new UserDto(user.Id, user.Username, user.Role));
    }
}
