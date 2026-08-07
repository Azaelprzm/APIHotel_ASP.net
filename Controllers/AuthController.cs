using System;
using System.Linq;
using System.Threading.Tasks;
using HotelAPI.Contracts;
using HotelAPI.Models;
using HotelAPI.Security;
using HotelAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
            var normalizedEmail = loginRequest.Email.Trim().ToLowerInvariant();

            var usuario = await _context.Usuarios
                .AsNoTracking()
                .Where(u => u.Email.ToLower() == normalizedEmail)
                .FirstOrDefaultAsync();

            if (usuario == null ||
                !usuario.Estado ||
                !BCrypt.Net.BCrypt.Verify(loginRequest.Password, usuario.PasswordHash))
            {
                return Unauthorized("Credenciales inválidas.");
            }

            var token = _authService.GenerateJwtToken(usuario.Email, usuario.Rol);

            return Ok(new LoginResponse(token.Value, token.ExpiresAtUtc));
        }

        // Endpoint para registrar un nuevo usuario (solo para Administradores)
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            var role = registerRequest.Rol.Trim();
            if (!AppRoles.IsValid(role))
            {
                return BadRequest($"Rol inválido. Usa {AppRoles.Administrador} o {AppRoles.Recepcionista}.");
            }

            var normalizedEmail = registerRequest.Email.Trim().ToLowerInvariant();

            var hasRegisteredUsers = await _context.Usuarios.AnyAsync();
            if (!hasRegisteredUsers && role != AppRoles.Administrador)
            {
                return BadRequest("El primer usuario debe tener el rol Administrador.");
            }

            if (hasRegisteredUsers && !User.IsInRole(AppRoles.Administrador))
            {
                return Forbid();
            }

            // Verificar si el email ya está registrado
            if (await _context.Usuarios.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            {
                return Conflict("El email ya está registrado.");
            }

            // Crear un nuevo usuario con la contraseña hasheada
            var nuevoUsuario = new Usuario
            {
                Nombre = registerRequest.Nombre.Trim(),
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                Rol = role,
                Estado = true,
                CreadoEn = DateTime.UtcNow
            };

            _context.Usuarios.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            return Ok($"Usuario con rol '{role}' registrado exitosamente.");
        }
    }
}
