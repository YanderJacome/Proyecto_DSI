using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Api.DTOs.Mesas;
using Restaurante.Api.Infrastructure.Data;

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
}