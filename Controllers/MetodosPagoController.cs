using HotelAPI.Contracts;
using HotelAPI.Models;
using HotelAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var metodosPago = await _context.MetodosPagos.AsNoTracking().ToListAsync();
            return Ok(metodosPago);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMetodoPagoById(int id)
        {
            var metodo = await _context.MetodosPagos.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
            return metodo == null ? NotFound() : Ok(metodo);
        }

        // POST: api/metodos-pago
        [HttpPost]
        [Authorize(Roles = AppRoles.Administrador)]
        public async Task<IActionResult> CrearMetodoPago([FromBody] CreateMetodoPagoRequest request)
        {
            var normalizedName = request.Nombre.Trim();
            if (await _context.MetodosPagos.AnyAsync(m => m.Nombre == normalizedName))
            {
                return BadRequest(new { mensaje = "El método de pago ya existe." });
            }

            var nuevoMetodo = new MetodoPago
            {
                Nombre = normalizedName
            };

            _context.MetodosPagos.Add(nuevoMetodo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMetodoPagoById), new { id = nuevoMetodo.Id }, nuevoMetodo);
        }

        // PUT: api/metodos-pago/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = AppRoles.Administrador)]
        public async Task<IActionResult> ActualizarMetodoPago(int id, [FromBody] UpdateMetodoPagoRequest request)
        {
            var metodo = await _context.MetodosPagos.FindAsync(id);
            if (metodo == null)
            {
                return NotFound(new { mensaje = "El método de pago no existe." });
            }

            var normalizedName = request.Nombre.Trim();
            if (await _context.MetodosPagos.AnyAsync(m => m.Nombre == normalizedName && m.Id != id))
            {
                return BadRequest(new { mensaje = "Otro método de pago con el mismo nombre ya existe." });
            }

            metodo.Nombre = normalizedName;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/metodos-pago/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = AppRoles.Administrador)]
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

}
