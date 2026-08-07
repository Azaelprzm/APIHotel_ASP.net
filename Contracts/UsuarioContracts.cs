using System.ComponentModel.DataAnnotations;

namespace HotelAPI.Contracts;

public sealed class UpdateUsuarioRequest
{
    [MaxLength(50)]
    public string? Nombre { get; init; }

    public string? Rol { get; init; }

    public bool? Estado { get; init; }
}
