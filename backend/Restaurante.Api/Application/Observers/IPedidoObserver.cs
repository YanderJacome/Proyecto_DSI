namespace Restaurante.Api.Application.Observers;

public interface IPedidoObserver
{
    Task NotificarNuevoPedidoAsync(int idPedido);
    Task NotificarCambioEstadoAsync(int idPedido, string nuevoEstado);
}