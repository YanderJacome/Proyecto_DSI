using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.Domain.Entities;

public class Pago
{
    public int IdPago { get; set; }
    public decimal Monto { get; set; }
    public MetodoPago MetodoPago { get; set; }
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    public int IdFactura { get; set; }
    public Factura? Factura { get; set; }
}