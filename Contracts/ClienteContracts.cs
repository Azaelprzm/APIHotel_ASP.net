using System.ComponentModel.DataAnnotations;

namespace HotelAPI.Contracts;

public sealed class CreateClienteRequest
{
    [Required, MaxLength(50)]
    public string Nombre { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string Apellido { get; init; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(15)]
    public string Telefono { get; init; } = string.Empty;

    [Required, MaxLength(20)]
    public string DocumentoIdentidad { get; init; } = string.Empty;
}

public sealed class UpdateClienteRequest
{
    [MaxLength(50)]
    public string? Nombre { get; init; }

    [MaxLength(50)]
    public string? Apellido { get; init; }

    [EmailAddress, MaxLength(100)]
    public string? Email { get; init; }

    [MaxLength(15)]
    public string? Telefono { get; init; }

    [MaxLength(20)]
    public string? DocumentoIdentidad { get; init; }
}
