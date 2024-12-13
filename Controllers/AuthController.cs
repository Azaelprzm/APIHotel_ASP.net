using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System;
using HotelAPI.Models;
using HotelAPI.Services;

namespace HotelAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly GestionHotelContext _context;
        private readonly AuthService _authService;

        public AuthController(GestionHotelContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        // Endpoint para iniciar sesión
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (string.IsNullOrEmpty(loginRequest.Email) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest("Email y contraseña son requeridos.");
            }

            var usuario = await _context.Usuarios
                .Where(u => u.Email == loginRequest.Email)
                .FirstOrDefaultAsync();

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, usuario.PasswordHash))
            {
                return Unauthorized("Credenciales inválidas.");
            }

            var token = _authService.GenerateJwtToken(usuario.Email, usuario.Rol);

            return Ok(new { Token = token });
        }

        // Endpoint para registrar un nuevo usuario (solo para Administradores)
        [Authorize(Roles = "Administrador")]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            // Validar los datos de entrada
            if (string.IsNullOrEmpty(registerRequest.Email) ||
                string.IsNullOrEmpty(registerRequest.Password) ||
                string.IsNullOrEmpty(registerRequest.Nombre) ||
                string.IsNullOrEmpty(registerRequest.Rol))
            {
                return BadRequest("Nombre, email, contraseña y rol son requeridos.");
            }

            // Verificar si el email ya está registrado
            if (await _context.Usuarios.AnyAsync(u => u.Email == registerRequest.Email))
            {
                return Conflict("El email ya está registrado.");
            }

            // Crear un nuevo usuario con la contraseña hasheada
            var nuevoUsuario = new Usuario
            {
                Nombre = registerRequest.Nombre,
                Email = registerRequest.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                Rol = registerRequest.Rol,
                Estado = true,
                CreadoEn = DateTime.Now
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok($"Usuario con rol '{registerRequest.Rol}' registrado exitosamente.");
        }
    }

    // Modelos para las solicitudes de autenticación y registro
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RegisterRequest
    {
        public string Nombre { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Rol { get; set; } // Este campo ahora es obligatorio
    }
}
