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
public class PagosController : ControllerBase
{
    private readonly GestionHotelContext _context;

    public PagosController(GestionHotelContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetPagos()
    {
        var pagos = await ProjectPagos(_context.Pagos.AsNoTracking()).ToListAsync();
        return Ok(pagos);
    }

    [HttpGet("reserva/{reservaId:int}")]
    public async Task<IActionResult> GetPagosPorReserva(int reservaId)
    {
        var pagos = await ProjectPagos(
                _context.Pagos.AsNoTracking().Where(p => p.ReservaId == reservaId))
            .ToListAsync();

        return Ok(pagos);
    }

    [HttpPost]
    public async Task<IActionResult> CrearPago([FromBody] CreatePagoRequest request)
    {
        if (request.FechaPago == default)
        {
            return BadRequest(new { mensaje = "La fecha de pago es obligatoria." });
        }

        var reserva = await _context.Reservas.FindAsync(request.ReservaId);
        if (reserva == null)
        {
            return NotFound(new { mensaje = "La reserva especificada no existe." });
        }

        if (reserva.Estado.Equals(ReservationStatuses.Cancelada, StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new { mensaje = "No se pueden registrar pagos para una reserva cancelada." });
        }

        if (!await _context.MetodosPagos.AnyAsync(m => m.Id == request.MetodoPagoId))
        {
            return NotFound(new { mensaje = "El método de pago especificado no existe." });
        }

        var montoPagado = reserva.MontoPagado ?? 0;
        var saldoPendiente = reserva.Total - montoPagado;
        if (request.MontoPago > saldoPendiente)
        {
            return BadRequest(new { mensaje = "El pago supera el saldo pendiente." });
        }

        var nuevoPago = new Pago
        {
            ReservaId = request.ReservaId,
            FechaPago = DateOnly.FromDateTime(request.FechaPago),
            MontoPago = request.MontoPago,
            MetodoPagoId = request.MetodoPagoId,
            ReferenciaTransaccion = request.ReferenciaTransaccion?.Trim(),
            DetallesPago = request.DetallesPago?.Trim()
        };

        _context.Pagos.Add(nuevoPago);
        reserva.MontoPagado = montoPagado + request.MontoPago;
        reserva.EstadoPago = ReservationRules.GetPaymentStatus(reserva.MontoPagado.Value, reserva.Total);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetPagosPorReserva),
            new { reservaId = request.ReservaId },
            nuevoPago);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Administrador)]
    public async Task<IActionResult> EliminarPago(int id)
    {
        var pago = await _context.Pagos.FindAsync(id);
        if (pago == null)
        {
            return NotFound(new { mensaje = "El pago especificado no existe." });
        }

        var reserva = await _context.Reservas.FindAsync(pago.ReservaId);
        if (reserva != null)
        {
            reserva.MontoPagado = Math.Max(0, (reserva.MontoPagado ?? 0) - pago.MontoPago);
            reserva.EstadoPago = ReservationRules.GetPaymentStatus(reserva.MontoPagado.Value, reserva.Total);
        }

        _context.Pagos.Remove(pago);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private static IQueryable<PagoResponse> ProjectPagos(IQueryable<Pago> query) =>
        query.Select(p => new PagoResponse
        {
            Id = p.Id,
            ReservaId = p.ReservaId,
            FechaPago = p.FechaPago,
            MontoPago = p.MontoPago,
            MetodoPagoId = p.MetodoPagoId,
            MetodoPago = p.MetodoPago.Nombre,
            ReferenciaTransaccion = p.ReferenciaTransaccion,
            DetallesPago = p.DetallesPago
        });
}
