using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Domain.Entities;
using Restaurante.Api.DTOs.Usuarios;
using Restaurante.Api.Infrastructure.Data;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize(Roles = "Administrador")] // Solo el admin puede entrar aquí
public class UsuariosController : ControllerBase
{
    private readonly RestauranteDbContext _db;

    public UsuariosController(RestauranteDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsuarios()
    {
        var usuarios = await _db.Usuarios
            .Include(u => u.Rol)
            .Select(u => new UsuarioResponseDto
            {
                IdUsuario = u.IdUsuario,
                Nombre = u.Nombre,
                TipoDocumento = u.TipoDocumento,
                NumeroDocumento = u.NumeroDocumento,
                Correo = u.Correo,
                Rol = u.Rol!.NombreRol,
                Activo = u.Activo
            })
            .ToListAsync();

        return Ok(usuarios);
    }

    [HttpPost]
    public async Task<IActionResult> CrearUsuario([FromBody] CrearUsuarioRequest request)
    {
        // Validar duplicados (RN-09)
        if (await _db.Usuarios.AnyAsync(u => u.Correo == request.Correo))
            return BadRequest(new { mensaje = "El correo ya está registrado." });
            
        if (await _db.Usuarios.AnyAsync(u => u.NumeroDocumento == request.NumeroDocumento))
            return BadRequest(new { mensaje = "El número de documento ya está registrado." });

        var rol = await _db.Roles.FindAsync(request.IdRol);
        if (rol == null) return BadRequest(new { mensaje = "El rol especificado no existe." });

        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            TipoDocumento = request.TipoDocumento,
            NumeroDocumento = request.NumeroDocumento,
            Correo = request.Correo,
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(request.Contrasena),
            Activo = true,
            IdRol = request.IdRol
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Usuario creado exitosamente." });
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var usuario = await _db.Usuarios.FindAsync(id);
        if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado." });

        usuario.Activo = !usuario.Activo; // Alternar estado
        await _db.SaveChangesAsync();

        string estadoStr = usuario.Activo ? "activado" : "inactivado";
        return Ok(new { mensaje = $"Usuario {estadoStr} exitosamente." });
    }
}