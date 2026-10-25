namespace Restaurante.Api.Domain.Enums;

public enum EstadoPedido
{
    EnviadoACocina = 0,
    EnPreparacion = 1,
    ListoParaServir = 2,
    Facturado = 3,
    Cancelado = 4
}