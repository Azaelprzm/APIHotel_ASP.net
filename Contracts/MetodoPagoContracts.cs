using System.ComponentModel.DataAnnotations;

namespace HotelAPI.Contracts;

public sealed class CreateMetodoPagoRequest
{
    [Required, MaxLength(50)]
    public string Nombre { get; init; } = string.Empty;
}
public sealed class UpdateMetodoPagoRequest
{
    [Required, MaxLength(50)]
    public string Nombre { get; init; } = string.Empty;
}
