namespace Restaurante.Api.Application.Observers;

public class PedidoSubject
{
    private readonly List<IPedidoObserver> _observers = new();

    public PedidoSubject(IEnumerable<IPedidoObserver> observers)
    {
        _observers.AddRange(observers);
    }

    public async Task NotificarNuevoPedidoAsync(int idPedido)
    {
        foreach (var observer in _observers)
        {
            await observer.NotificarNuevoPedidoAsync(idPedido);
        }
    }

    public async Task NotificarCambioEstadoAsync(int idPedido, string nuevoEstado)
    {
        foreach (var observer in _observers)
        {
            await observer.NotificarCambioEstadoAsync(idPedido, nuevoEstado);
        }
    }
}