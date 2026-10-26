using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Infrastructure.Data;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/productos")]
[Authorize(Roles = "Administrador,Mesero")]
public class ProductosController : ControllerBase
{
    private readonly RestauranteDbContext _db;

    public ProductosController(RestauranteDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetProductos()
    {
        var productos = await _db.Productos
            .Where(p => p.Disponible)
            .OrderBy(p => p.Categoria)
            .ThenBy(p => p.Nombre)
            .Select(p => new
            {
                p.IdProducto,
                p.Nombre,
                p.Precio,
                p.Categoria
            })
            .ToListAsync();

        return Ok(productos);
    }
}