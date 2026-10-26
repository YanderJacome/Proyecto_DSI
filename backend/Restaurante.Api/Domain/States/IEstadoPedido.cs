using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.States;

public interface IEstadoPedido
{
    EstadoPedido Estado { get; }
    string Nombre { get; }
    
    bool PuedeCancelar();
    bool PuedeModificar();
    IEstadoPedido SiguienteEstado();
}