using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelAPI.Models;

namespace HotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MetodosPagoController : ControllerBase
    {
        private readonly GestionHotelContext _context;

        public MetodosPagoController(GestionHotelContext context)
        {
            _context = context;
        }

        // GET: api/metodos-pago
        [HttpGet]
        public async Task<IActionResult> GetMetodosPago()
        {
            var metodosPago = await _context.MetodosPagos.ToListAsync();
            return Ok(metodosPago);
        }

        // POST: api/metodos-pago
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CrearMetodoPago([FromBody] CreateMetodoPagoRequest request)
        {
            if (await _context.MetodosPagos.AnyAsync(m => m.Nombre == request.Nombre))
            {
                return BadRequest(new { mensaje = "El método de pago ya existe." });
            }

            var nuevoMetodo = new MetodosPago
            {
                Nombre = request.Nombre
            };

            _context.MetodosPagos.Add(nuevoMetodo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMetodosPago), new { id = nuevoMetodo.Id }, nuevoMetodo);
        }

        // PUT: api/metodos-pago/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> ActualizarMetodoPago(int id, [FromBody] UpdateMetodoPagoRequest request)
        {
            var metodo = await _context.MetodosPagos.FindAsync(id);
            if (metodo == null)
            {
                return NotFound(new { mensaje = "El método de pago no existe." });
            }

            if (await _context.MetodosPagos.AnyAsync(m => m.Nombre == request.Nombre && m.Id != id))
            {
                return BadRequest(new { mensaje = "Otro método de pago con el mismo nombre ya existe." });
            }

            metodo.Nombre = request.Nombre;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/metodos-pago/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarMetodoPago(int id)
        {
            var metodo = await _context.MetodosPagos.FindAsync(id);
            if (metodo == null)
            {
                return NotFound(new { mensaje = "El método de pago no existe." });
            }

            // Verificar si está asociado a algún pago
            if (await _context.Pagos.AnyAsync(p => p.MetodoPagoId == id))
            {
                return BadRequest(new { mensaje = "El método de pago no puede eliminarse porque está asociado a pagos existentes." });
            }

            _context.MetodosPagos.Remove(metodo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    public class CreateMetodoPagoRequest
    {
        public string Nombre { get; set; }
    }

    public class UpdateMetodoPagoRequest
    {
        public string Nombre { get; set; }
    }
}
