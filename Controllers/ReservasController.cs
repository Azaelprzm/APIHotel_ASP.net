using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelAPI.Models;
using System.Linq;
using System.Threading.Tasks;

namespace HotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Requiere autenticación para acceder a este controlador
    public class ReservasController : ControllerBase
    {
        private readonly GestionHotelContext _context;

        public ReservasController(GestionHotelContext context)
        {
            _context = context;
        }

        // Obtener todas las reservas
        [HttpGet]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> GetReservas()
        {
            var reservas = await _context.Reservas
                .Include(r => r.Habitacion)
                .Include(r => r.Cliente)
                .Select(r => new
                {
                    r.Id,
                    r.FechaInicio,
                    r.FechaFin,
                    r.Estado,
                    Habitacion = new { r.Habitacion.Id, r.Habitacion.Numero, r.Habitacion.Tipo },
                    Cliente = new { r.Cliente.Id, r.Cliente.Nombre, r.Cliente.Apellido },
                    r.Total,
                    r.MontoPagado,
                    r.SaldoPendiente,
                    r.EstadoPago
                })
                .ToListAsync();

            return Ok(reservas);
        }

        // Obtener una reserva por ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> GetReservaById(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Habitacion)
                .Include(r => r.Cliente)
                .Select(r => new
                {
                    r.Id,
                    r.FechaInicio,
                    r.FechaFin,
                    r.Estado,
                    Habitacion = new { r.Habitacion.Id, r.Habitacion.Numero, r.Habitacion.Tipo },
                    Cliente = new { r.Cliente.Id, r.Cliente.Nombre, r.Cliente.Apellido },
                    r.Total,
                    r.MontoPagado,
                    r.SaldoPendiente,
                    r.EstadoPago
                })
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null)
            {
                return NotFound($"Reserva con ID {id} no encontrada.");
            }

            return Ok(reserva);
        }

        // Crear una nueva reserva
        [HttpPost]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> CreateReserva([FromBody] CreateReservaRequest createRequest)
        {
            // Validar que la habitación existe y está disponible
            var habitacion = await _context.Habitaciones.FindAsync(createRequest.HabitacionId);
            if (habitacion == null || habitacion.Estado != "Disponible")
            {
                return BadRequest("La habitación especificada no existe o no está disponible.");
            }

            // Validar que el cliente existe
            var cliente = await _context.Clientes.FindAsync(createRequest.ClienteId);
            if (cliente == null)
            {
                return BadRequest("El cliente especificado no existe.");
            }

            var nuevaReserva = new Reserva
            {
                FechaInicio = DateOnly.FromDateTime(createRequest.FechaInicio),
                FechaFin = DateOnly.FromDateTime(createRequest.FechaFin),
                Estado = "Confirmada",
                HabitacionId = createRequest.HabitacionId,
                ClienteId = createRequest.ClienteId,
                Total = createRequest.Total,
                MontoPagado = createRequest.MontoPagado,
                EstadoPago = createRequest.MontoPagado >= createRequest.Total ? "Pagado" : "Pendiente"
            };


            // Actualizar el estado de la habitación a "Ocupada"
            habitacion.Estado = "Ocupada";

            _context.Reservas.Add(nuevaReserva);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReservaById), new { id = nuevaReserva.Id }, nuevaReserva);
        }

        // Actualizar una reserva
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> UpdateReserva(int id, [FromBody] UpdateReservaRequest updateRequest)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound($"Reserva con ID {id} no encontrada.");
            }

            reserva.FechaInicio = updateRequest.FechaInicio.HasValue
                ? DateOnly.FromDateTime(updateRequest.FechaInicio.Value)
                : reserva.FechaInicio;
            reserva.FechaFin = updateRequest.FechaFin.HasValue
                ? DateOnly.FromDateTime(updateRequest.FechaFin.Value)
                : reserva.FechaFin;
            reserva.Estado = updateRequest.Estado ?? reserva.Estado;
            reserva.MontoPagado = updateRequest.MontoPagado ?? reserva.MontoPagado;
            reserva.EstadoPago = reserva.MontoPagado >= reserva.Total ? "Pagado" : "Pendiente";

            _context.Reservas.Update(reserva);
            await _context.SaveChangesAsync();

            return Ok("Reserva actualizada exitosamente.");
        }

        // Eliminar una reserva
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteReserva(int id)
        {
            var reserva = await _context.Reservas.FindAsync(id);

            if (reserva == null)
            {
                return NotFound($"Reserva con ID {id} no encontrada.");
            }

            var habitacion = await _context.Habitaciones.FindAsync(reserva.HabitacionId);
            if (habitacion != null)
            {
                habitacion.Estado = "Disponible"; // Liberar la habitación
            }

            _context.Reservas.Remove(reserva);
            await _context.SaveChangesAsync();

            return Ok("Reserva eliminada exitosamente.");
        }

        // Buscar reservas por fecha, estado, cliente o habitación
        [HttpGet("buscar")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> BuscarReservas([FromQuery] string estado, [FromQuery] int? clienteId, [FromQuery] int? habitacionId)
        {
            var query = _context.Reservas.AsQueryable();

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(r => r.Estado.ToLower().Contains(estado.ToLower()));
            }

            if (clienteId.HasValue)
            {
                query = query.Where(r => r.ClienteId == clienteId.Value);
            }

            if (habitacionId.HasValue)
            {
                query = query.Where(r => r.HabitacionId == habitacionId.Value);
            }

            var reservas = await query
                .Include(r => r.Habitacion)
                .Include(r => r.Cliente)
                .Select(r => new
                {
                    r.Id,
                    r.FechaInicio,
                    r.FechaFin,
                    r.Estado,
                    Habitacion = new { r.Habitacion.Id, r.Habitacion.Numero, r.Habitacion.Tipo },
                    Cliente = new { r.Cliente.Id, r.Cliente.Nombre, r.Cliente.Apellido },
                    r.Total,
                    r.MontoPagado,
                    r.SaldoPendiente,
                    r.EstadoPago
                })
                .ToListAsync();

            if (!reservas.Any())
            {
                return NotFound("No se encontraron reservas que coincidan con los criterios de búsqueda.");
            }

            return Ok(reservas);
        }
    }

    // Modelos para solicitudes
    public class CreateReservaRequest
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public int HabitacionId { get; set; }
        public int ClienteId { get; set; }
        public decimal Total { get; set; }
        public decimal MontoPagado { get; set; }
    }

    public class UpdateReservaRequest
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string Estado { get; set; }
        public decimal? MontoPagado { get; set; }
    }

}
