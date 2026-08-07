# Hotel API

API REST para administrar clientes, habitaciones, reservas y pagos de un hotel. El proyecto fue desarrollado con ASP.NET Core y conserva su propósito académico original, con una estructura y configuración preparadas para ejecutarse de forma reproducible y segura.

## Funcionalidades

- Autenticación con JWT y contraseñas protegidas con BCrypt.
- Roles de `Administrador` y `Recepcionista`.
- Gestión de usuarios, clientes y habitaciones.
- Reservas con validación de fechas y disponibilidad.
- Cálculo automático del total según noches y tarifa de la habitación.
- Registro de pagos y actualización del saldo y su estado.
- Documentación interactiva mediante Swagger.

## Tecnologías

- .NET 8 y ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Bearer y BCrypt
- Swagger / OpenAPI
- xUnit

## Estructura

```text
├── Contracts/       Solicitudes y respuestas de la API
├── Controllers/     Endpoints HTTP
├── Domain/          Estados y reglas compartidas del negocio
├── Models/          Entidades y contexto de Entity Framework
├── Options/         Configuración tipada
├── Security/        Roles admitidos
├── Services/        JWT y reglas del dominio
├── database/        Script para crear la base de datos
└── tests/           Pruebas unitarias
```

## Requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server o SQL Server Express
- SQL Server Management Studio, Azure Data Studio o `sqlcmd`

## Configuración local

1. Clona el repositorio y entra en su carpeta.

2. Ejecuta [`database/schema.sql`](database/schema.sql) en SQL Server. El script crea la base `GestionHotel`, sus tablas y los métodos de pago iniciales.

3. Guarda la conexión y la clave JWT fuera del repositorio mediante *user secrets*:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=GestionHotel;Trusted_Connection=True;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "reemplaza-esto-por-un-secreto-aleatorio-de-al-menos-32-caracteres"
```

Los nombres usados por variables de entorno son:

```text
ConnectionStrings__DefaultConnection
Jwt__Key
Jwt__Issuer
Jwt__Audience
```

4. Restaura y ejecuta la API:

```powershell
dotnet restore
dotnet run
```

Swagger estará disponible en `http://localhost:5044/swagger` durante desarrollo.

## Primer administrador

Cuando la tabla `Usuarios` está vacía, `POST /api/auth/register` permite crear únicamente el primer usuario con rol `Administrador`. Después de ese registro, el endpoint exige un JWT de administrador para crear más usuarios.

Ejemplo:

```json
{
  "nombre": "Administrador",
  "email": "admin@hotel.local",
  "password": "cambia-esta-clave",
  "rol": "Administrador"
}
```

## Autenticación en Swagger

1. Inicia sesión mediante `POST /api/auth/login`.
2. Copia el token de la respuesta.
3. Pulsa **Authorize** en Swagger e introduce el token.

## Pruebas

```powershell
dotnet test tests/HotelAPI.Tests/HotelAPI.Tests.csproj
```

Las pruebas cubren rangos y solapamientos de reservas, cálculo de noches y estados de pago.

## Reglas principales

- La fecha final debe ser posterior a la inicial.
- Dos reservas activas de una habitación no pueden solaparse.
- Las reservas consecutivas sí están permitidas.
- El total se calcula en el servidor; el cliente no puede establecerlo.
- Los pagos no pueden superar el saldo pendiente.
- Una reserva con pagos no puede eliminarse directamente.

## Seguridad

- No guardes conexiones, contraseñas ni claves JWT en archivos versionados.
- Los usuarios desactivados no pueden iniciar sesión.
- La clave JWT debe contener al menos 32 caracteres.
- En producción configura un emisor, una audiencia y HTTPS adecuados para el entorno.
