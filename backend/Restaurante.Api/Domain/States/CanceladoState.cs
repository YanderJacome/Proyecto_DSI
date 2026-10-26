using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.States;

public class CanceladoState : EstadoPedidoBase
{
    public override EstadoPedido Estado => EstadoPedido.Cancelado;
    public override string Nombre => "Cancelado";

    public override bool PuedeCancelar() => false;
    public override bool PuedeModificar() => false;

    public override IEstadoPedido SiguienteEstado() => this;
}