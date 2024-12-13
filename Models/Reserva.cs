using System;
using System.Collections.Generic;

namespace HotelAPI.Models;

public partial class Reserva
{
    public int Id { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly FechaFin { get; set; }

    public string Estado { get; set; } = null!;

    public int HabitacionId { get; set; }

    public int ClienteId { get; set; }

    public decimal Total { get; set; }

    public decimal? MontoPagado { get; set; }

    public decimal? SaldoPendiente { get; set; }

    public string EstadoPago { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Habitacione Habitacion { get; set; } = null!;

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
