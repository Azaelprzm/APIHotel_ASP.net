using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HotelAPI.Models;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

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
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> CreateHabitacion([FromBody] CreateHabitacionRequest createRequest)
        {
            if (string.IsNullOrEmpty(createRequest.Numero) ||
                string.IsNullOrEmpty(createRequest.Tipo) ||
                createRequest.PrecioPorNoche <= 0)
            {
                return BadRequest("Número, tipo y precio por noche son obligatorios.");
            }

            var nuevaHabitacion = new Habitacione
            {
                Numero = createRequest.Numero,
                Tipo = createRequest.Tipo,
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
        public async Task<IActionResult> BuscarHabitaciones([FromQuery] string tipo, [FromQuery] string estado)
        {
            var query = _context.Habitaciones.AsQueryable();

            // Filtrar por tipo si se proporciona
            if (!string.IsNullOrEmpty(tipo))
            {
                query = query.Where(h => h.Tipo.ToLower().Contains(tipo.ToLower()));
            }

            // Filtrar por estado si se proporciona
            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(h => h.Estado.ToLower().Contains(estado.ToLower()));
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
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> UpdateHabitacion(int id, [FromBody] UpdateHabitacionRequest updateRequest)
        {
            var habitacion = await _context.Habitaciones.FindAsync(id);

            if (habitacion == null)
            {
                return NotFound($"Habitación con ID {id} no encontrada.");
            }

            habitacion.Numero = updateRequest.Numero ?? habitacion.Numero;
            habitacion.Tipo = updateRequest.Tipo ?? habitacion.Tipo;
            habitacion.PrecioPorNoche = updateRequest.PrecioPorNoche ?? habitacion.PrecioPorNoche;
            habitacion.Estado = updateRequest.Estado ?? habitacion.Estado;

            _context.Habitaciones.Update(habitacion);
            await _context.SaveChangesAsync();

            return Ok("Habitación actualizada exitosamente.");
        }

        // Eliminar una habitación
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
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

    // Modelos para las solicitudes
    public class CreateHabitacionRequest
    {
        public string Numero { get; set; } // Número de habitación
        public string Tipo { get; set; } // Tipo de habitación
        public decimal PrecioPorNoche { get; set; } // Precio por noche
        public string Estado { get; set; } // Estado (Opcional, por defecto será "Disponible")
    }

    public class UpdateHabitacionRequest
    {
        public string Numero { get; set; } // Número de habitación (Opcional)
        public string Tipo { get; set; } // Tipo de habitación (Opcional)
        public decimal? PrecioPorNoche { get; set; } // Precio por noche (Opcional)
        public string Estado { get; set; } // Estado (Opcional)
    }
}
