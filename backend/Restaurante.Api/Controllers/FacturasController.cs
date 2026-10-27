using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurante.Api.Domain.Entities;
using Restaurante.Api.Domain.Enums;
using Restaurante.Api.Infrastructure.Data;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/facturas")]
[Authorize(Roles = "Cajero,Administrador")]
public class FacturasController : ControllerBase
{
    private readonly RestauranteDbContext _db;

    public FacturasController(RestauranteDbContext db)
    {
        _db = db;
    }

    [HttpGet("pendientes")]
    public async Task<IActionResult> GetPendientes()
    {
        // Mostramos los pedidos que están listos para servir o en preparación
        var pedidos = await _db.Pedidos
            .Include(p => p.Mesa)
            .Include(p => p.Detalles).ThenInclude(d => d.Producto)
            .Where(p => p.Estado == EstadoPedido.ListoParaServir || p.Estado == EstadoPedido.EnPreparacion)
            .OrderBy(p => p.Fecha)
            .Select(p => new
            {
                p.IdPedido,
                NumeroMesa = p.Mesa!.NumeroMesa,
                p.Total,
                Estado = p.Estado.ToString(),
                Detalles = p.Detalles.Select(d => new { d.Producto!.Nombre, d.Cantidad, d.Subtotal })
            })
            .ToListAsync();

        return Ok(pedidos);
    }

    [HttpPost]
    public async Task<IActionResult> GenerarFactura([FromBody] GenerarFacturaRequest request)
    {
        var pedido = await _db.Pedidos.Include(p => p.Mesa).FirstOrDefaultAsync(p => p.IdPedido == request.IdPedido);
        
        if (pedido == null) return NotFound(new { mensaje = "Pedido no encontrado." });
        if (pedido.Estado == EstadoPedido.Facturado) return BadRequest(new { mensaje = "Este pedido ya fue facturado." });

        // 1. Crear la Factura
        var factura = new Factura
        {
            IdPedido = pedido.IdPedido,
            Fecha = DateTime.UtcNow,
            Total = pedido.Total,
            MetodoPago = request.MetodoPago
        };
        _db.Facturas.Add(factura);

        // 2. Registrar el Pago
        var pago = new Pago
        {
            Monto = pedido.Total,
            MetodoPago = request.MetodoPago,
            Fecha = DateTime.UtcNow
        };
        factura.Pagos.Add(pago);

        // 3. Actualizar Estado del Pedido (Patrón State)
        pedido.Estado = EstadoPedido.Facturado;

        // 4. Liberar la Mesa
        if (pedido.Mesa != null)
        {
            pedido.Mesa.Estado = EstadoMesa.Disponible;
        }

        await _db.SaveChangesAsync();

        return Ok(new { mensaje = "Factura generada exitosamente.", idFactura = factura.IdFactura });
    }
}

public class GenerarFacturaRequest
{
    public int IdPedido { get; set; }
    public MetodoPago MetodoPago { get; set; }
}