using System.ComponentModel.DataAnnotations;
using EndustriB2C.Entities;

namespace EndustriB2C.DTOs;

public class RegisterRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8), MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserType UserType { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
}

public class UserAddressDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public string AddressLine { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public bool IsDefault { get; set; }
}

public class UserAddressRequest
{
    [Required, MaxLength(60)]
    public string Title { get; set; } = "Adres";

    [Required, MaxLength(160)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string City { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string District { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string AddressLine { get; set; } = string.Empty;

    [MaxLength(16)]
    public string? PostalCode { get; set; }

    public bool IsDefault { get; set; }
}

public class AdminUserUpdateRequest
{
    [Required, MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    public UserType UserType { get; set; }
    public bool IsActive { get; set; }
}

public static class UserMaps
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Phone = user.Phone,
        FirstName = user.FirstName,
        LastName = user.LastName,
        FullName = user.FullName,
        UserType = user.UserType,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt
    };

    public static UserAddressDto ToDto(this UserAddress a) => new()
    {
        Id = a.Id,
        Title = a.Title,
        FullName = a.FullName,
        Phone = a.Phone,
        City = a.City,
        District = a.District,
        AddressLine = a.AddressLine,
        PostalCode = a.PostalCode,
        IsDefault = a.IsDefault
    };
}
