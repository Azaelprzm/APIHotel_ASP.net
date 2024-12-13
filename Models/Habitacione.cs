using System;
using System.Collections.Generic;

namespace HotelAPI.Models;

public partial class Habitacione
{
    public int Id { get; set; }

    public string Numero { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public decimal PrecioPorNoche { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
