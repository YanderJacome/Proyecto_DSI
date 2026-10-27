using Microsoft.AspNetCore.SignalR;
using Restaurante.Api.Hubs;

namespace Restaurante.Api.Application.Observers;

public class SignalRPedidoObserver : IPedidoObserver
{
    private readonly IHubContext<PedidosHub> _hubContext;

    public SignalRPedidoObserver(IHubContext<PedidosHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotificarNuevoPedidoAsync(int idPedido)
    {
        // Envía un mensaje al grupo "Cocina"
        await _hubContext.Clients.Group("Cocina").SendAsync("NuevoPedidoRecibido", idPedido);
    }

    public async Task NotificarCambioEstadoAsync(int idPedido, string nuevoEstado)
    {
        // Notifica a Cocina y también a los Meseros
        await _hubContext.Clients.Group("Cocina").SendAsync("PedidoActualizado", idPedido, nuevoEstado);
        await _hubContext.Clients.Group("Meseros").SendAsync("PedidoActualizado", idPedido, nuevoEstado);
    }
}