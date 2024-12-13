using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using HotelAPI.Models;

namespace GestionHotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")] // Controlador accesible solo para administradores
    public class UsuariosController : ControllerBase
    {
        private readonly GestionHotelContext _context;

        public UsuariosController(GestionHotelContext context)
        {
            _context = context;
        }

        // Obtener todos los usuarios
        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Email,
                    u.Rol,
                    u.Estado,
                    u.CreadoEn,
                    u.ActualizadoEn
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        // Obtener un usuario por su ID
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetUsuarioById(int id)
        {
            var usuario = await _context.Usuarios
                .Select(u => new
                {
                    u.Id,
                    u.Nombre,
                    u.Email,
                    u.Rol,
                    u.Estado,
                    u.CreadoEn,
                    u.ActualizadoEn
                })
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound($"Usuario con ID {id} no encontrado.");
            }

            return Ok(usuario);
        }

        // Actualizar un usuario
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateUsuario(int id, [FromBody] UpdateUsuarioRequest updateRequest)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound($"Usuario con ID {id} no encontrado.");
            }

            // Actualizar los campos permitidos
            usuario.Nombre = updateRequest.Nombre ?? usuario.Nombre;
            usuario.Rol = updateRequest.Rol ?? usuario.Rol;
            usuario.Estado = updateRequest.Estado ?? usuario.Estado;
            usuario.ActualizadoEn = DateTime.Now;

            _context.Usuarios.Update(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario actualizado exitosamente.");
        }

        // Eliminar un usuario
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
            {
                return NotFound($"Usuario con ID {id} no encontrado.");
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            return Ok("Usuario eliminado exitosamente.");
        }
    }

    // Modelo para actualizar usuarios
    public class UpdateUsuarioRequest
    {
        public string Nombre { get; set; } // Nombre del usuario
        public string Rol { get; set; } // Rol del usuario (Opcional)
        public bool? Estado { get; set; } // Estado activo/inactivo (Opcional)
    }
}
