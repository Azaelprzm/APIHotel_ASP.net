using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelAPI.Models;

namespace HotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PagosController : ControllerBase
    {
        private readonly GestionHotelContext _context;

        public PagosController(GestionHotelContext context)
        {
            _context = context;
        }

        // GET: api/pagos
        [HttpGet]
        public async Task<IActionResult> GetPagos()
        {
            var pagos = await _context.Pagos
                .Include(p => p.Reserva)
                .Include(p => p.MetodoPago)
                .ToListAsync();

            return Ok(pagos);
        }

        // GET: api/pagos/{reservaId}
        [HttpGet("{reservaId}")]
        public async Task<IActionResult> GetPagosPorReserva(int reservaId)
        {
            var pagos = await _context.Pagos
                .Where(p => p.ReservaId == reservaId)
                .Include(p => p.MetodoPago)
                .ToListAsync();

            if (!pagos.Any())
            {
                return NotFound(new { mensaje = "No se encontraron pagos para esta reserva." });
            }

            return Ok(pagos);
        }

        // POST: api/pagos
        [HttpPost]
        public async Task<IActionResult> CrearPago([FromBody] CreatePagoRequest request)
        {
            var reserva = await _context.Reservas.FindAsync(request.ReservaId);
            if (reserva == null)
            {
                return NotFound(new { mensaje = "La reserva especificada no existe." });
            }

            var metodoPago = await _context.MetodosPagos.FindAsync(request.MetodoPagoId);
            if (metodoPago == null)
            {
                return NotFound(new { mensaje = "El método de pago especificado no existe." });
            }

            if (request.MontoPago <= 0 || request.MontoPago > reserva.SaldoPendiente)
            {
                return BadRequest(new { mensaje = "El monto de pago es inválido." });
            }

            // Crear el nuevo pago
            var nuevoPago = new Pago
            {
                ReservaId = request.ReservaId,
                FechaPago = DateOnly.FromDateTime(request.FechaPago),
                MontoPago = request.MontoPago,
                MetodoPagoId = request.MetodoPagoId,
                ReferenciaTransaccion = request.ReferenciaTransaccion,
                DetallesPago = request.DetallesPago
            };

            _context.Pagos.Add(nuevoPago);

            // Actualizar el monto pagado en la reserva
            reserva.MontoPagado += request.MontoPago;
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPagosPorReserva), new { reservaId = request.ReservaId }, nuevoPago);
        }

        // DELETE: api/pagos/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
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
                reserva.MontoPagado -= pago.MontoPago;
            }

            _context.Pagos.Remove(pago);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class CreatePagoRequest
    {
        public int ReservaId { get; set; }
        public DateTime FechaPago { get; set; }
        public decimal MontoPago { get; set; }
        public int MetodoPagoId { get; set; }
        public string? ReferenciaTransaccion { get; set; }
        public string? DetallesPago { get; set; }
    }
}

