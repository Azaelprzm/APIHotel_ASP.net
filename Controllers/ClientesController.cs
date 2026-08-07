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
        [Authorize(Roles = AppRoles.PersonalHotel)]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _context.Clientes
                .AsNoTracking()
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
        [Authorize(Roles = AppRoles.PersonalHotel)]
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
        [Authorize(Roles = AppRoles.PersonalHotel)]
        public async Task<IActionResult> CreateCliente([FromBody] CreateClienteRequest createRequest)
        {
            var normalizedEmail = createRequest.Email.Trim().ToLowerInvariant();
            var normalizedDocument = createRequest.DocumentoIdentidad.Trim();

            // Verificar si ya existe un cliente con el mismo email o documento
            if (await _context.Clientes.AnyAsync(c =>
                    c.Email == normalizedEmail || c.DocumentoIdentidad == normalizedDocument))
            {
                return BadRequest("Ya existe un cliente con el mismo email o documento de identidad.");
            }

            var nuevoCliente = new Cliente
            {
                Nombre = createRequest.Nombre.Trim(),
                Apellido = createRequest.Apellido.Trim(),
                Email = normalizedEmail,
                Telefono = createRequest.Telefono.Trim(),
                DocumentoIdentidad = normalizedDocument
            };

            _context.Clientes.Add(nuevoCliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetClienteById), new { id = nuevoCliente.Id }, nuevoCliente);
        }

        // Actualizar un cliente
        [HttpPut("{id:int}")]
        [Authorize(Roles = AppRoles.PersonalHotel)]
        public async Task<IActionResult> UpdateCliente(int id, [FromBody] UpdateClienteRequest updateRequest)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound($"Cliente con ID {id} no encontrado.");
            }

            var normalizedEmail = updateRequest.Email?.Trim().ToLowerInvariant();
            var normalizedDocument = updateRequest.DocumentoIdentidad?.Trim();
            var duplicateExists = await _context.Clientes.AnyAsync(c =>
                c.Id != id &&
                ((normalizedEmail != null && c.Email == normalizedEmail) ||
                 (normalizedDocument != null && c.DocumentoIdentidad == normalizedDocument)));

            if (duplicateExists)
            {
                return Conflict("Ya existe otro cliente con el mismo email o documento de identidad.");
            }

            cliente.Nombre = updateRequest.Nombre?.Trim() ?? cliente.Nombre;
            cliente.Apellido = updateRequest.Apellido?.Trim() ?? cliente.Apellido;
            cliente.Email = normalizedEmail ?? cliente.Email;
            cliente.Telefono = updateRequest.Telefono?.Trim() ?? cliente.Telefono;
            cliente.DocumentoIdentidad = normalizedDocument ?? cliente.DocumentoIdentidad;

            await _context.SaveChangesAsync();

            return Ok("Cliente actualizado exitosamente.");
        }

        // Eliminar un cliente
        [HttpDelete("{id:int}")]
        [Authorize(Roles = AppRoles.Administrador)]
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
        [Authorize(Roles = AppRoles.PersonalHotel)]
        public async Task<IActionResult> BuscarClientes(
            [FromQuery] string? nombre,
            [FromQuery] string? apellido,
            [FromQuery] string? documentoIdentidad)
        {
            var query = _context.Clientes.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(c => c.Nombre.Contains(nombre));
            }

            if (!string.IsNullOrEmpty(apellido))
            {
                query = query.Where(c => c.Apellido.Contains(apellido));
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

}
