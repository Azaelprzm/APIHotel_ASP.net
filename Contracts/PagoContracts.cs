using System.ComponentModel.DataAnnotations;

namespace HotelAPI.Contracts;

public sealed class CreatePagoRequest
{
    [Range(1, int.MaxValue)]
    public int ReservaId { get; init; }

    public DateTime FechaPago { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal MontoPago { get; init; }

    [Range(1, int.MaxValue)]
    public int MetodoPagoId { get; init; }

    [MaxLength(100)]
    public string? ReferenciaTransaccion { get; init; }

    [MaxLength(255)]
    public string? DetallesPago { get; init; }
}

public sealed class PagoResponse
{
    public int Id { get; init; }
    public int ReservaId { get; init; }
    public DateOnly FechaPago { get; init; }
    public decimal MontoPago { get; init; }
    public int MetodoPagoId { get; init; }
    public string MetodoPago { get; init; } = string.Empty;
    public string? ReferenciaTransaccion { get; init; }
    public string? DetallesPago { get; init; }
}
