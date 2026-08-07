using HotelAPI.Contracts;
using HotelAPI.Domain;
using HotelAPI.Models;
using HotelAPI.Security;
using HotelAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = AppRoles.PersonalHotel)]
public class ReservasController : ControllerBase
{
    private readonly GestionHotelContext _context;

    public ReservasController(GestionHotelContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetReservas()
    {
        var reservas = await ProjectReservas(_context.Reservas.AsNoTracking()).ToListAsync();
        return Ok(reservas);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetReservaById(int id)
    {
        var reserva = await ProjectReservas(_context.Reservas.AsNoTracking())
            .FirstOrDefaultAsync(r => r.Id == id);

        return reserva == null
            ? NotFound($"Reserva con ID {id} no encontrada.")
            : Ok(reserva);
    }

    [HttpPost]
    public async Task<IActionResult> CreateReserva([FromBody] CreateReservaRequest request)
    {
        var fechaInicio = DateOnly.FromDateTime(request.FechaInicio);
        var fechaFin = DateOnly.FromDateTime(request.FechaFin);

        if (!ReservationRules.HasValidDateRange(fechaInicio, fechaFin))
        {
            return BadRequest("La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        var habitacion = await _context.Habitaciones.FindAsync(request.HabitacionId);
        if (habitacion == null)
        {
            return BadRequest("La habitación especificada no existe.");
        }

        if (IsUnavailableByStatus(habitacion.Estado))
        {
            return Conflict("La habitación está fuera de servicio o en mantenimiento.");
        }

        if (!await _context.Clientes.AnyAsync(c => c.Id == request.ClienteId))
        {
            return BadRequest("El cliente especificado no existe.");
        }

        if (await HasOverlappingReservation(request.HabitacionId, fechaInicio, fechaFin))
        {
            return Conflict("La habitación ya tiene una reserva que coincide con esas fechas.");
        }

        var nuevaReserva = new Reserva
        {
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Estado = ReservationStatuses.Confirmada,
            HabitacionId = request.HabitacionId,
            ClienteId = request.ClienteId,
            Total = ReservationRules.CalculateTotal(fechaInicio, fechaFin, habitacion.PrecioPorNoche),
            MontoPagado = 0,
            EstadoPago = "Pendiente"
        };

        _context.Reservas.Add(nuevaReserva);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetReservaById), new { id = nuevaReserva.Id }, nuevaReserva);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateReserva(int id, [FromBody] UpdateReservaRequest request)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null)
        {
            return NotFound($"Reserva con ID {id} no encontrada.");
        }

        var fechaInicio = request.FechaInicio.HasValue
            ? DateOnly.FromDateTime(request.FechaInicio.Value)
            : reserva.FechaInicio;
        var fechaFin = request.FechaFin.HasValue
            ? DateOnly.FromDateTime(request.FechaFin.Value)
            : reserva.FechaFin;

        if (!ReservationRules.HasValidDateRange(fechaInicio, fechaFin))
        {
            return BadRequest("La fecha de fin debe ser posterior a la fecha de inicio.");
        }

        var status = request.Estado?.Trim() ?? reserva.Estado;
        if (!ReservationStatuses.IsValid(status))
        {
            return BadRequest("Estado inválido. Usa Pendiente, Confirmada, Cancelada o Completada.");
        }

        if (status != ReservationStatuses.Cancelada &&
            await HasOverlappingReservation(reserva.HabitacionId, fechaInicio, fechaFin, reserva.Id))
        {
            return Conflict("La habitación ya tiene una reserva que coincide con esas fechas.");
        }

        var habitacion = await _context.Habitaciones.FindAsync(reserva.HabitacionId);
        if (habitacion == null)
        {
            return Conflict("La habitación asociada ya no existe.");
        }

        var total = ReservationRules.CalculateTotal(fechaInicio, fechaFin, habitacion.PrecioPorNoche);
        if ((reserva.MontoPagado ?? 0) > total)
        {
            return Conflict("El nuevo total sería menor que el monto ya pagado.");
        }

        reserva.FechaInicio = fechaInicio;
        reserva.FechaFin = fechaFin;
        reserva.Total = total;
        reserva.Estado = status;
        reserva.EstadoPago = ReservationRules.GetPaymentStatus(reserva.MontoPagado ?? 0, total);

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Administrador)]
    public async Task<IActionResult> DeleteReserva(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null)
        {
            return NotFound($"Reserva con ID {id} no encontrada.");
        }

        if (await _context.Pagos.AnyAsync(p => p.ReservaId == id))
        {
            return Conflict("No se puede eliminar una reserva que tiene pagos registrados.");
        }

        _context.Reservas.Remove(reserva);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarReservas(
        [FromQuery] string? estado,
        [FromQuery] int? clienteId,
        [FromQuery] int? habitacionId)
    {
        var query = _context.Reservas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(r => r.Estado.Contains(estado));
        }

        if (clienteId.HasValue)
        {
            query = query.Where(r => r.ClienteId == clienteId.Value);
        }

        if (habitacionId.HasValue)
        {
            query = query.Where(r => r.HabitacionId == habitacionId.Value);
        }

        var reservas = await ProjectReservas(query).ToListAsync();
        return Ok(reservas);
    }

    private Task<bool> HasOverlappingReservation(
        int habitacionId,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        int? excludedReservationId = null) =>
        _context.Reservas.AnyAsync(r =>
            r.HabitacionId == habitacionId &&
            (!excludedReservationId.HasValue || r.Id != excludedReservationId.Value) &&
            r.Estado != ReservationStatuses.Cancelada &&
            fechaInicio < r.FechaFin &&
            fechaFin > r.FechaInicio);

    private static bool IsUnavailableByStatus(string status) =>
        status.Equals("Mantenimiento", StringComparison.OrdinalIgnoreCase) ||
        status.Equals("Fuera de servicio", StringComparison.OrdinalIgnoreCase);

    private static IQueryable<ReservaResponse> ProjectReservas(IQueryable<Reserva> query) =>
        query.Select(r => new ReservaResponse
        {
            Id = r.Id,
            FechaInicio = r.FechaInicio,
            FechaFin = r.FechaFin,
            Estado = r.Estado,
            Habitacion = new HabitacionResumen
            {
                Id = r.Habitacion.Id,
                Numero = r.Habitacion.Numero,
                Tipo = r.Habitacion.Tipo
            },
            Cliente = new ClienteResumen
            {
                Id = r.Cliente.Id,
                Nombre = r.Cliente.Nombre,
                Apellido = r.Cliente.Apellido
            },
            Total = r.Total,
            MontoPagado = r.MontoPagado ?? 0,
            SaldoPendiente = r.Total - (r.MontoPagado ?? 0),
            EstadoPago = r.EstadoPago
        });
}
