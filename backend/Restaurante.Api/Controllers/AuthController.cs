using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Authentication;
using Restaurante.Api.DTOs.Auth;
using Restaurante.Api.Infrastructure.Data;
using System.Security.Claims;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private const int MaxIntentos = 3;
    private const int MinutosBloqueo = 5;

    private readonly RestauranteDbContext _db;
    private readonly JwtService _jwt;

    public AuthController(RestauranteDbContext db, JwtService jwt)
    {
        _db = db;
        _jwt = jwt;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuario = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Correo == request.Correo);

        if (usuario == null || usuario.Rol == null)
            return Unauthorized(new { mensaje = "Usuario o contraseña incorrectos." });

        // Bloqueo activo
        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.UtcNow)
        {
            var restante = (int)Math.Ceiling((usuario.BloqueadoHasta.Value - DateTime.UtcNow).TotalSeconds);
            return StatusCode(StatusCodes.Status423Locked, new
            {
                mensaje = $"Cuenta bloqueada. Intente nuevamente en {restante} segundos.",
                segundosRestantes = restante
            });
        }

        // Bloqueo expirado: resetear contadores
        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value <= DateTime.UtcNow)
        {
            usuario.BloqueadoHasta = null;
            usuario.IntentosFallidos = 0;
        }

        if (!usuario.Activo)
            return Unauthorized(new { mensaje = "Usuario inactivo. Contacte al administrador." });

        bool passwordOk = BCrypt.Net.BCrypt.Verify(request.Contrasena, usuario.ContrasenaHash);

        if (!passwordOk)
        {
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= MaxIntentos)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(MinutosBloqueo);
                usuario.IntentosFallidos = 0;
                await _db.SaveChangesAsync();
                return StatusCode(StatusCodes.Status423Locked, new
                {
                    mensaje = $"Cuenta bloqueada durante {MinutosBloqueo} minutos por {MaxIntentos} intentos fallidos."
                });
            }

            await _db.SaveChangesAsync();
            int restantes = MaxIntentos - usuario.IntentosFallidos;
            return Unauthorized(new
            {
                mensaje = $"Usuario o contraseña incorrectos. Le quedan {restantes} intentos."
            });
        }

        // Login exitoso
        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;
        await _db.SaveChangesAsync();

        var (token, expira) = _jwt.GenerateToken(usuario, usuario.Rol.NombreRol);

        return Ok(new LoginResponse
        {
            Token = token,
            ExpiresAt = expira,
            Nombre = usuario.Nombre,
            Rol = usuario.Rol.NombreRol
        });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(idClaim, out var id))
            return Unauthorized();

        var user = await _db.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);

        if (user == null || user.Rol == null)
            return Unauthorized();

        return Ok(new
        {
            idUsuario = user.IdUsuario,
            nombre = user.Nombre,
            correo = user.Correo,
            rol = user.Rol.NombreRol
        });
    }
}