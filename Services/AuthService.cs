using EndustriB2C.Auth;
using EndustriB2C.Data;
using EndustriB2C.DTOs;
using EndustriB2C.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EndustriB2C.Services;

public class AuthService
{
    public const string RefreshCookie = "b2c_refresh";
    public const string GuestCartCookie = "b2c_guest";

    private readonly AppDbContext _db;
    private readonly JwtTokenService _jwt;
    private readonly JwtOptions _jwtOptions;
    private readonly PasswordHasher<User> _hasher = new();
    private readonly IWebHostEnvironment _env;

    public AuthService(AppDbContext db, JwtTokenService jwt, JwtOptions jwtOptions, IWebHostEnvironment env)
    {
        _db = db;
        _jwt = jwt;
        _jwtOptions = jwtOptions;
        _env = env;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, HttpContext http)
    {
        ValidatePassword(request.Password);
        var email = request.Email.Trim().ToLowerInvariant();
        if (await _db.Users.AnyAsync(x => x.Email == email))
        {
            throw new InvalidOperationException("Bu e-posta adresi zaten kayıtlı.");
        }

        var user = new User
        {
            Email = email,
            Phone = request.Phone.Trim(),
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            UserType = UserType.User,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };
        user.PasswordHash = _hasher.HashPassword(user, request.Password);
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await MergeGuestCartAsync(user.Id, http);
        return await IssueAsync(user, http);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, HttpContext http)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email)
                   ?? throw new UnauthorizedAccessException("E-posta veya şifre hatalı.");

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Hesabınız pasif durumda.");
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedAccessException("Çok fazla hatalı deneme. Lütfen daha sonra tekrar deneyin.");
        }

        var verify = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (verify == PasswordVerificationResult.Failed)
        {
            user.FailedLoginCount++;
            if (user.FailedLoginCount >= 5)
            {
                user.LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(15);
                user.FailedLoginCount = 0;
            }
            await _db.SaveChangesAsync();
            throw new UnauthorizedAccessException("E-posta veya şifre hatalı.");
        }

        if (verify == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
        }

        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        user.LastLoginAt = DateTimeOffset.UtcNow;
        await MergeGuestCartAsync(user.Id, http);
        return await IssueAsync(user, http);
    }

    public async Task<AuthResponse> RefreshAsync(HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue(RefreshCookie, out var refresh) || string.IsNullOrWhiteSpace(refresh))
        {
            throw new UnauthorizedAccessException("Oturum bulunamadı.");
        }

        var hash = JwtTokenService.HashToken(refresh);
        var user = await _db.Users.FirstOrDefaultAsync(x => x.RefreshTokenHash == hash)
                   ?? throw new UnauthorizedAccessException("Oturum geçersiz.");

        if (!user.IsActive || user.RefreshTokenExpiry is null || user.RefreshTokenExpiry < DateTimeOffset.UtcNow)
        {
            throw new UnauthorizedAccessException("Oturum süresi doldu.");
        }

        return await IssueAsync(user, http);
    }

    public async Task LogoutAsync(HttpContext http, int? userId)
    {
        if (userId.HasValue)
        {
            var user = await _db.Users.FindAsync(userId.Value);
            if (user is not null)
            {
                user.RefreshTokenHash = null;
                user.RefreshTokenExpiry = null;
                await _db.SaveChangesAsync();
            }
        }

        http.Response.Cookies.Delete(RefreshCookie, CookieOptions(TimeSpan.Zero));
    }

    private async Task<AuthResponse> IssueAsync(User user, HttpContext http)
    {
        var tokens = _jwt.CreateTokens(user);
        user.RefreshTokenHash = JwtTokenService.HashToken(tokens.RefreshToken);
        user.RefreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);
        await _db.SaveChangesAsync();

        http.Response.Cookies.Append(RefreshCookie, tokens.RefreshToken, CookieOptions(TimeSpan.FromDays(_jwtOptions.RefreshTokenDays)));

        return new AuthResponse
        {
            AccessToken = tokens.AccessToken,
            ExpiresIn = tokens.ExpiresIn,
            User = user.ToDto()
        };
    }

    private async Task MergeGuestCartAsync(int userId, HttpContext http)
    {
        if (!http.Request.Cookies.TryGetValue(GuestCartCookie, out var tokenValue) || !Guid.TryParse(tokenValue, out var guestToken))
        {
            return;
        }

        var guestCart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.GuestToken == guestToken);
        if (guestCart is null)
        {
            return;
        }

        var userCart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        if (userCart is null)
        {
            guestCart.UserId = userId;
            guestCart.GuestToken = null;
            guestCart.ExpiresAt = null;
            guestCart.UpdatedAt = DateTimeOffset.UtcNow;
        }
        else
        {
            foreach (var item in guestCart.Items)
            {
                var existing = userCart.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
                if (existing is null)
                {
                    userCart.Items.Add(new CartItem { ProductId = item.ProductId, Quantity = item.Quantity });
                }
                else
                {
                    existing.Quantity += item.Quantity;
                }
            }
            userCart.UpdatedAt = DateTimeOffset.UtcNow;
            _db.Carts.Remove(guestCart);
        }

        await _db.SaveChangesAsync();
        http.Response.Cookies.Delete(GuestCartCookie, CookieOptions(TimeSpan.Zero));
    }

    private CookieOptions CookieOptions(TimeSpan maxAge) => new()
    {
        HttpOnly = true,
        Secure = !_env.IsDevelopment() || true,
        SameSite = SameSiteMode.Lax,
        Path = "/",
        MaxAge = maxAge,
        IsEssential = true
    };

    public static void ValidatePassword(string password)
    {
        if (password.Length < 8 || !password.Any(char.IsUpper) || !password.Any(char.IsLower) || !password.Any(char.IsDigit))
        {
            throw new InvalidOperationException("Şifre en az 8 karakter olmalı ve büyük harf, küçük harf ile rakam içermelidir.");
        }
    }
}
