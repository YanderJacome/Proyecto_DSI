using System.ComponentModel.DataAnnotations;

namespace Restaurante.Api.DTOs.Usuarios;

public class CrearUsuarioRequest
{
    [Required]
    public string Nombre { get; set; } = string.Empty;
    
    [Required]
    public string TipoDocumento { get; set; } = string.Empty;
    
    [Required]
    public string NumeroDocumento { get; set; } = string.Empty;
    
    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;
    
    [Required, MinLength(6)]
    public string Contrasena { get; set; } = string.Empty;
    
    [Required]
    public int IdRol { get; set; }
}