using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.States;

public class EnPreparacionState : EstadoPedidoBase
{
    public override EstadoPedido Estado => EstadoPedido.EnPreparacion;
    public override string Nombre => "En Preparación";

    public override bool PuedeCancelar() => true;
    public override bool PuedeModificar() => true;

    public override IEstadoPedido SiguienteEstado() => new ListoParaServirState();
}