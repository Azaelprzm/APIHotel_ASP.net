using System.Linq;
using System.Threading.Tasks;
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
    [Authorize] // Requiere autenticación para acceder a cualquier método
    public class HabitacionesController : ControllerBase
    {
        private readonly GestionHotelContext _context;

        public HabitacionesController(GestionHotelContext context)
        {
            _context = context;
        }

        // Obtener todas las habitaciones
        [HttpGet]
        [AllowAnonymous] // Permite que todos los usuarios (autenticados o no) puedan listar habitaciones
        public async Task<IActionResult> GetHabitaciones()
        {
            var habitaciones = await _context.Habitaciones
                .Select(h => new
                {
                    h.Id,
                    h.Numero,
                    h.Tipo,
                    h.PrecioPorNoche,
                    h.Estado
                })
                .ToListAsync();

            return Ok(habitaciones);
        }

        // Obtener una habitación por su ID
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHabitacionById(int id)
        {
            var habitacion = await _context.Habitaciones
                .Select(h => new
                {
                    h.Id,
                    h.Numero,
                    h.Tipo,
                    h.PrecioPorNoche,
                    h.Estado
                })
                .FirstOrDefaultAsync(h => h.Id == id);

            if (habitacion == null)
            {
                return NotFound($"Habitación con ID {id} no encontrada.");
            }

            return Ok(habitacion);
        }

        // Crear una nueva habitación
        [HttpPost]
        [Authorize(Roles = AppRoles.Administrador)]
        public async Task<IActionResult> CreateHabitacion([FromBody] CreateHabitacionRequest createRequest)
        {
            var normalizedNumber = createRequest.Numero.Trim();
            if (await _context.Habitaciones.AnyAsync(h => h.Numero == normalizedNumber))
            {
                return Conflict("Ya existe una habitación con ese número.");
            }

            var nuevaHabitacion = new Habitacion
            {
                Numero = normalizedNumber,
                Tipo = createRequest.Tipo.Trim(),
                PrecioPorNoche = createRequest.PrecioPorNoche,
                Estado = createRequest.Estado ?? "Disponible"
            };

            _context.Habitaciones.Add(nuevaHabitacion);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHabitacionById), new { id = nuevaHabitacion.Id }, nuevaHabitacion);
        }

        // Buscar habitaciones por tipo o estado
        [HttpGet("buscar")]
        [AllowAnonymous] // Permite que cualquier usuario pueda realizar búsquedas
        public async Task<IActionResult> BuscarHabitaciones(
            [FromQuery] string? tipo,
            [FromQuery] string? estado)
        {
            var query = _context.Habitaciones.AsQueryable();

            // Filtrar por tipo si se proporciona
            if (!string.IsNullOrEmpty(tipo))
            {
                query = query.Where(h => h.Tipo.Contains(tipo));
            }

            // Filtrar por estado si se proporciona
            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(h => h.Estado.Contains(estado));
            }

            var habitaciones = await query
                .Select(h => new
                {
                    h.Id,
                    h.Numero,
                    h.Tipo,
                    h.PrecioPorNoche,
                    h.Estado
                })
                .ToListAsync();

            if (!habitaciones.Any())
            {
                return NotFound("No se encontraron habitaciones que coincidan con los criterios de búsqueda.");
            }

            return Ok(habitaciones);
        }


        // Actualizar una habitación
        [HttpPut("{id:int}")]
        [Authorize(Roles = AppRoles.Administrador)]
        public async Task<IActionResult> UpdateHabitacion(int id, [FromBody] UpdateHabitacionRequest updateRequest)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);

            if (habitacion == null)
            {
                return NotFound($"Habitación con ID {id} no encontrada.");
            }

            var normalizedNumber = updateRequest.Numero?.Trim();
            if (normalizedNumber != null &&
                await _context.Habitaciones.AnyAsync(h => h.Id != id && h.Numero == normalizedNumber))
            {
                return Conflict("Ya existe otra habitación con ese número.");
            }

            habitacion.Numero = normalizedNumber ?? habitacion.Numero;
            habitacion.Tipo = updateRequest.Tipo?.Trim() ?? habitacion.Tipo;
            habitacion.PrecioPorNoche = updateRequest.PrecioPorNoche ?? habitacion.PrecioPorNoche;
            habitacion.Estado = updateRequest.Estado?.Trim() ?? habitacion.Estado;

            await _context.SaveChangesAsync();

            return Ok("Habitación actualizada exitosamente.");
        }

        // Eliminar una habitación
        [HttpDelete("{id:int}")]
        [Authorize(Roles = AppRoles.Administrador)]
        public async Task<IActionResult> DeleteHabitacion(int id)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);

            if (habitacion == null)
            {
                return NotFound($"Habitación con ID {id} no encontrada.");
            }

            _context.Habitaciones.Remove(habitacion);
            await _context.SaveChangesAsync();

            return Ok("Habitación eliminada exitosamente.");
        }
    }

}
