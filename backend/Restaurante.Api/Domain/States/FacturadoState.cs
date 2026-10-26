using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.States;

public class FacturadoState : EstadoPedidoBase
{
    public override EstadoPedido Estado => EstadoPedido.Facturado;
    public override string Nombre => "Facturado";

    public override bool PuedeCancelar() => false;
    public override bool PuedeModificar() => false;

    public override IEstadoPedido SiguienteEstado() => this;
}