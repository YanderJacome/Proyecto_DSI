using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Api.DTOs.Mesas;
using Restaurante.Api.Infrastructure.Data;
using Restaurante.Api.Domain.Enums;


namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/mesas")]
[Authorize] 
public class MesasController : ControllerBase
{
    private readonly RestauranteDbContext _db;

    public MesasController(RestauranteDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    [Authorize(Roles = "Administrador,Mesero,Cajero")] 
    public async Task<IActionResult> GetMesas()
    {
        var mesas = await _db.Mesas
            .OrderBy(m => m.NumeroMesa)
            .Select(m => new MesaResponseDto
            {
                IdMesa = m.IdMesa,
                NumeroMesa = m.NumeroMesa,
                Estado = m.Estado.ToString()
            })
            .ToListAsync();

        return Ok(mesas);
    }

    [HttpPatch("{id}/estado")]
    [Authorize(Roles = "Administrador,Mesero")]
    public async Task<IActionResult> CambiarEstado(int id, [FromBody] CambiarEstadoMesaRequest request)
    {
        var mesa = await _db.Mesas.FindAsync(id);
        if (mesa == null) return NotFound(new { mensaje = "Mesa no encontrada." });

        // Validar que no se intente reservar una mesa ocupada
        if (mesa.Estado == EstadoMesa.Ocupada && request.NuevoEstado == EstadoMesa.Reservada)
            return BadRequest(new { mensaje = "No se puede reservar una mesa que ya está ocupada." });

        mesa.Estado = request.NuevoEstado;
        await _db.SaveChangesAsync();

        return Ok(new { mensaje = $"Mesa actualizada a: {mesa.Estado}" });
    }

}

