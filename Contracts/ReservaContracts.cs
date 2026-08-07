using System.ComponentModel.DataAnnotations;

namespace HotelAPI.Contracts;

public sealed class CreateReservaRequest
{
    public DateTime FechaInicio { get; init; }

    public DateTime FechaFin { get; init; }

    [Range(1, int.MaxValue)]
    public int HabitacionId { get; init; }

    [Range(1, int.MaxValue)]
    public int ClienteId { get; init; }
}

public sealed class UpdateReservaRequest
{
    public DateTime? FechaInicio { get; init; }

    public DateTime? FechaFin { get; init; }

    [MaxLength(20)]
    public string? Estado { get; init; }
}

public sealed class ReservaResponse
{
    public int Id { get; init; }
    public DateOnly FechaInicio { get; init; }
    public DateOnly FechaFin { get; init; }
    public string Estado { get; init; } = string.Empty;
    public HabitacionResumen Habitacion { get; init; } = new();
    public ClienteResumen Cliente { get; init; } = new();
    public decimal Total { get; init; }
    public decimal MontoPagado { get; init; }
    public decimal SaldoPendiente { get; init; }
    public string EstadoPago { get; init; } = string.Empty;
}

public sealed class HabitacionResumen
{
    public int Id { get; init; }
    public string Numero { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
}

public sealed class ClienteResumen
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string Apellido { get; init; } = string.Empty;
}
