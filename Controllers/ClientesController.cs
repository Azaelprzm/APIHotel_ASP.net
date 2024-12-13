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
    [Authorize] // Requiere autenticación para todos los métodos del controlador
    public class ClientesController : ControllerBase
    {
        private readonly GestionHotelContext _context;

        public ClientesController(GestionHotelContext context)
        {
            _context = context;
        }

        // Obtener todos los clientes
        [HttpGet]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Clientes
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Apellido,
                    c.Email,
                    c.Telefono,
                    c.DocumentoIdentidad
                })
                .ToListAsync();

            return Ok(clientes);
        }

        // Obtener un cliente por ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> GetClienteById(int id)
        {
            var cliente = await _context.Clientes
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Apellido,
                    c.Email,
                    c.Telefono,
                    c.DocumentoIdentidad
                })
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound($"Cliente con ID {id} no encontrado.");
            }

            return Ok(cliente);
        }

        // Crear un nuevo cliente
        [HttpPost]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> CreateCliente([FromBody] CreateClienteRequest createRequest)
        {
            if (string.IsNullOrEmpty(createRequest.Nombre) ||
                string.IsNullOrEmpty(createRequest.Apellido) ||
                string.IsNullOrEmpty(createRequest.Email) ||
                string.IsNullOrEmpty(createRequest.Telefono) ||
                string.IsNullOrEmpty(createRequest.DocumentoIdentidad))
            {
                return BadRequest("Todos los campos son obligatorios.");
            }

            // Verificar si ya existe un cliente con el mismo email o documento
            if (await _context.Clientes.AnyAsync(c => c.Email == createRequest.Email || c.DocumentoIdentidad == createRequest.DocumentoIdentidad))
            {
                return BadRequest("Ya existe un cliente con el mismo email o documento de identidad.");
            }

            var nuevoCliente = new Cliente
            {
                Nombre = createRequest.Nombre,
                Apellido = createRequest.Apellido,
                Email = createRequest.Email,
                Telefono = createRequest.Telefono,
                DocumentoIdentidad = createRequest.DocumentoIdentidad
            };

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClienteById), new { id = nuevoCliente.Id }, nuevoCliente);
        }

        // Actualizar un cliente
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] UpdateClienteRequest updateRequest)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound($"Cliente con ID {id} no encontrado.");
            }

            cliente.Nombre = updateRequest.Nombre ?? cliente.Nombre;
            cliente.Apellido = updateRequest.Apellido ?? cliente.Apellido;
            cliente.Email = updateRequest.Email ?? cliente.Email;
            cliente.Telefono = updateRequest.Telefono ?? cliente.Telefono;
            cliente.DocumentoIdentidad = updateRequest.DocumentoIdentidad ?? cliente.DocumentoIdentidad;

            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();

            return Ok("Cliente actualizado exitosamente.");
        }

        // Eliminar un cliente
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound($"Cliente con ID {id} no encontrado.");
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return Ok("Cliente eliminado exitosamente.");
        }

        // Buscar clientes por nombre, apellido o documento de identidad
        [HttpGet("buscar")]
        [Authorize(Roles = "Administrador,Recepcionista")]
        public async Task<IActionResult> BuscarClientes([FromQuery] string nombre, [FromQuery] string apellido, [FromQuery] string documentoIdentidad)
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(c => c.Nombre.ToLower().Contains(nombre.ToLower()));
            }

            if (!string.IsNullOrEmpty(apellido))
            {
                query = query.Where(c => c.Apellido.ToLower().Contains(apellido.ToLower()));
            }

            if (!string.IsNullOrEmpty(documentoIdentidad))
            {
                query = query.Where(c => c.DocumentoIdentidad == documentoIdentidad);
            }

            var clientes = await query
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    c.Apellido,
                    c.Email,
                    c.Telefono,
                    c.DocumentoIdentidad
                })
                .ToListAsync();

            if (!clientes.Any())
            {
                return NotFound("No se encontraron clientes que coincidan con los criterios de búsqueda.");
            }

            return Ok(clientes);
        }
    }

    // Modelos para las solicitudes
    public class CreateClienteRequest
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string DocumentoIdentidad { get; set; }
    }

    public class UpdateClienteRequest
    {
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Email { get; set; }
        public string Telefono { get; set; }
        public string DocumentoIdentidad { get; set; }
    }
}
