using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.States;

public static class EstadoPedidoFactory
{
    public static IEstadoPedido Crear(EstadoPedido estado) => estado switch
    {
        EstadoPedido.EnviadoACocina => new EnviadoACocinaState(),
        EstadoPedido.EnPreparacion => new EnPreparacionState(),
        EstadoPedido.ListoParaServir => new ListoParaServirState(),
        EstadoPedido.Facturado => new FacturadoState(),
        EstadoPedido.Cancelado => new CanceladoState(),
        _ => throw new ArgumentOutOfRangeException(nameof(estado), estado, "Estado de pedido no válido")
    };
}