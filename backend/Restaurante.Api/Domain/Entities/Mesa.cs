using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.Entities;

public class Mesa
{
    public int IdMesa { get; set; }
    public int NumeroMesa { get; set; }
    public EstadoMesa Estado { get; set; } = EstadoMesa.Disponible;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}