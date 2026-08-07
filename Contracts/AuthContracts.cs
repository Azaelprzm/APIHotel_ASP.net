using System.ComponentModel.DataAnnotations;

namespace HotelAPI.Contracts;

public sealed class LoginRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public sealed class RegisterRequest
{
    [Required, MaxLength(50)]
    public string Nombre { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; init; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; init; } = string.Empty;

    [Required]
    public string Rol { get; init; } = string.Empty;
}

public sealed record LoginResponse(string Token, DateTime ExpiresAtUtc);
