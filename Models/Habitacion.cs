namespace HotelAPI.Models;

public partial class Habitacion
{
    public int Id { get; set; }

    public string Numero { get; set; } = null!;

    public string Tipo { get; set; } = null!;

    public decimal PrecioPorNoche { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
}
