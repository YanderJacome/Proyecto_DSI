using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.Entities;

public class Pedido
{
    public int IdPedido { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public EstadoPedido Estado { get; set; } = EstadoPedido.EnviadoACocina;
    public decimal Total { get; set; }

    public int IdMesa { get; set; }
    public Mesa? Mesa { get; set; }

    public int IdUsuario { get; set; }
    public Usuario? Usuario { get; set; }

    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    public Factura? Factura { get; set; }
}