using System;
using System.Collections.Generic;

namespace HotelAPI.Models;

public partial class Pago
{
    public int Id { get; set; }

    public int ReservaId { get; set; }

    public DateOnly FechaPago { get; set; }

    public decimal MontoPago { get; set; }

    public int MetodoPagoId { get; set; }

    public string? ReferenciaTransaccion { get; set; }

    public string? DetallesPago { get; set; }

    public virtual MetodosPago MetodoPago { get; set; } = null!;

    public virtual Reserva Reserva { get; set; } = null!;
}
