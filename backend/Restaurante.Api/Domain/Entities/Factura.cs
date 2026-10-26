using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.Entities;

public class Factura
{
    public int IdFactura { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }
    public MetodoPago MetodoPago { get; set; }

    public int IdPedido { get; set; }
    public Pedido? Pedido { get; set; }

    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}