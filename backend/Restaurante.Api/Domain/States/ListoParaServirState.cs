using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.States;

public class ListoParaServirState : EstadoPedidoBase
{
    public override EstadoPedido Estado => EstadoPedido.ListoParaServir;
    public override string Nombre => "Listo para Servir";

    public override bool PuedeCancelar() => false;
    public override bool PuedeModificar() => false;

    public override IEstadoPedido SiguienteEstado() => this; // Ya está en el tope
}