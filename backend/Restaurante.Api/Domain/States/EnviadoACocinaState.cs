using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.States;

public class EnviadoACocinaState : EstadoPedidoBase
{
    public override EstadoPedido Estado => EstadoPedido.EnviadoACocina;
    public override string Nombre => "Enviado a Cocina";

    public override bool PuedeCancelar() => true;
    public override bool PuedeModificar() => true;

    public override IEstadoPedido SiguienteEstado() => new EnPreparacionState();
}