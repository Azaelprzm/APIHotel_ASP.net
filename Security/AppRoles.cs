namespace HotelAPI.Security;

public static class AppRoles
{
    public const string Administrador = "Administrador";
    public const string Recepcionista = "Recepcionista";
    public const string PersonalHotel = Administrador + "," + Recepcionista;

    public static bool IsValid(string role) =>
        role is Administrador or Recepcionista;
}
