using System.ComponentModel.DataAnnotations;

namespace HotelAPI.Contracts;

public sealed class CreateHabitacionRequest
{
    [Required, MaxLength(10)]
    public string Numero { get; init; } = string.Empty;

    [Required, MaxLength(50)]
    public string Tipo { get; init; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal PrecioPorNoche { get; init; }

    [MaxLength(20)]
    public string? Estado { get; init; }
}
public sealed class UpdateHabitacionRequest
{
    [MaxLength(10)]
    public string? Numero { get; init; }

    [MaxLength(50)]
    public string? Tipo { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal? PrecioPorNoche { get; init; }

    [MaxLength(20)]
    public string? Estado { get; init; }
}
