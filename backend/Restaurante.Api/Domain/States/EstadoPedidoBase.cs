using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.States;

public abstract class EstadoPedidoBase : IEstadoPedido
{
    public abstract EstadoPedido Estado { get; }
    public abstract string Nombre { get; }

    public virtual bool PuedeCancelar() => false;
    public virtual bool PuedeModificar() => false;
    public abstract IEstadoPedido SiguienteEstado();
}