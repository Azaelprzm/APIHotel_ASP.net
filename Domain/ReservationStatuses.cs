namespace HotelAPI.Domain;

public static class ReservationStatuses
{
    public const string Pendiente = "Pendiente";
    public const string Confirmada = "Confirmada";
    public const string Cancelada = "Cancelada";
    public const string Completada = "Completada";

    public static bool IsValid(string status) =>
        status is Pendiente or Confirmada or Cancelada or Completada;
}
