using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Application.Observers;
using Restaurante.Api.Domain.Enums;
using Restaurante.Api.Domain.States;
using Restaurante.Api.Infrastructure.Data;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/cocina")]
[Authorize(Roles = "Cocina,Administrador")]
public class CocinaController : ControllerBase
{
    private readonly RestauranteDbContext _db;
    private readonly PedidoSubject _subject;

    public CocinaController(RestauranteDbContext db, PedidoSubject subject)
    {
        _db = db;
        _subject = subject;
    }

    [HttpGet("pedidos")]
    public async Task<IActionResult> GetPedidosActivos()
    {
        var pedidos = await _db.Pedidos
            .Include(p => p.Mesa)
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Where(p => p.Estado == EstadoPedido.EnviadoACocina || 
                        p.Estado == EstadoPedido.EnPreparacion || 
                        p.Estado == EstadoPedido.ListoParaServir)
            .OrderBy(p => p.Fecha)
                        .Select(p => new
            {
                p.IdPedido,
                NumeroMesa = p.Mesa!.NumeroMesa, // El ! indica que estamos seguros de que Mesa no es null
                p.Fecha,
                Estado = p.Estado.ToString(),
                Total = p.Total,
                Detalles = p.Detalles.Select(d => new
                {
                    Producto = d.Producto!.Nombre, // El ! indica que Producto no es null
                    d.Cantidad,
                    d.Observaciones
                })
            })
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpPatch("{id}/estado")]
    public async Task<IActionResult> CambiarEstado(int id)
    {
        var pedido = await _db.Pedidos.FindAsync(id);
        if (pedido == null) return NotFound(new { mensaje = "Pedido no encontrado." });

        // Usamos el Patrón State para saber a qué estado sigue
        var estadoActual = EstadoPedidoFactory.Crear(pedido.Estado);
        var siguienteEstado = estadoActual.SiguienteEstado();

        if (siguienteEstado.Estado == pedido.Estado)
            return BadRequest(new { mensaje = "El pedido ya se encuentra en el estado final." });

        pedido.Estado = siguienteEstado.Estado;
        await _db.SaveChangesAsync();

        // Notificar el cambio (Observer)
        await _subject.NotificarCambioEstadoAsync(pedido.IdPedido, siguienteEstado.Nombre);

        return Ok(new { mensaje = $"Pedido actualizado a: {siguienteEstado.Nombre}" });
    }
}