using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Restaurante.Api.Application.Facades;
using Restaurante.Api.DTOs.Pedidos;

namespace Restaurante.Api.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize(Roles = "Administrador,Mesero")]
public class PedidosController : ControllerBase
{
    private readonly PedidoFacade _pedidoFacade;

    public PedidosController(PedidoFacade pedidoFacade)
    {
        _pedidoFacade = pedidoFacade;
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

            // Devolvemos un DTO simple con la info del pedido guardado
            return CreatedAtAction(nameof(CrearPedido), new { id = pedido.IdPedido }, new
            {
                mensaje = "Pedido registrado exitosamente.",
                idPedido = pedido.IdPedido,
                total = pedido.Total
            });
        }
        catch (Exception ex)
        {
            // En producción, usar un Middleware de errores, pero para el prototipo esto es útil
            return BadRequest(new { mensaje = ex.Message });
        }
    }
}