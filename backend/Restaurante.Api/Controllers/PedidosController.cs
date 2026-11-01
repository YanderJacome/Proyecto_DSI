using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Application.Facades;
using Restaurante.Api.Domain.Enums;
using Restaurante.Api.DTOs.Pedidos;
using Restaurante.Api.Infrastructure.Data;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize(Roles = "Administrador,Mesero")]
public class PedidosController : ControllerBase
{
    private readonly PedidoFacade _pedidoFacade;
    private readonly RestauranteDbContext _db; // <-- NUEVO

    public PedidosController(PedidoFacade pedidoFacade, RestauranteDbContext db) // <-- NUEVO
    {
        _pedidoFacade = pedidoFacade;
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> CrearPedido([FromBody] CrearPedidoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var idUsuarioClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(idUsuarioClaim, out var idUsuario))
                return Unauthorized();

            var pedido = await _pedidoFacade.RegistrarPedidoAsync(request, idUsuario);

            return CreatedAtAction(nameof(CrearPedido), new { id = pedido.IdPedido }, new
            {
                mensaje = "Pedido registrado exitosamente.",
                idPedido = pedido.IdPedido,
                total = pedido.Total
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

       [HttpGet("mesa/{idMesa}")]
    public async Task<IActionResult> GetPedidosPorMesa(int idMesa)
    {
        var pedidos = await _db.Pedidos
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Where(p => p.IdMesa == idMesa && 
                        p.Estado != EstadoPedido.Facturado && 
                        p.Estado != EstadoPedido.Cancelado)
            .Select(p => new
            {
                p.IdPedido,
                p.IdMesa, // <-- ESTA ES LA LÍNEA QUE FALTABA
                Estado = p.Estado.ToString(),
                Total = p.Total,
                Detalles = p.Detalles.Select(d => new { d.Producto!.Nombre, d.Cantidad })
            })
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpPost("{id}/cancelar")]
    public async Task<IActionResult> CancelarPedido(int id)
    {
        try
        {
            await _pedidoFacade.CancelarPedidoAsync(id);
            return Ok(new { mensaje = "Pedido cancelado exitosamente." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPedido(int id)
    {
        var pedido = await _db.Pedidos
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Where(p => p.IdPedido == id)
            .Select(p => new
            {
                p.IdPedido,
                p.IdMesa,
                p.Estado,
                p.Total,
                Detalles = p.Detalles.Select(d => new
                {
                    d.IdProducto,
                    d.Cantidad,
                    d.Observaciones,
                    d.PrecioUnitario
                })
            })
            .FirstOrDefaultAsync();

        if (pedido == null) return NotFound();
        return Ok(pedido);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> ModificarPedido(int id, [FromBody] ModificarPedidoRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        try
        {
            var pedido = await _pedidoFacade.ModificarPedidoAsync(id, request);
            return Ok(new { mensaje = "Pedido modificado exitosamente.", total = pedido.Total });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = ex.Message });
        }
    }


}