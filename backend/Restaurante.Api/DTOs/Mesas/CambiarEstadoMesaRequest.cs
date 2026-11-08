using Restaurante.Api.Domain.Enums;

namespace Restaurante.Api.DTOs.Mesas;

public class CambiarEstadoMesaRequest
{
    public EstadoMesa NuevoEstado { get; set; }
}