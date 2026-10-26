using System.ComponentModel.DataAnnotations;

namespace Restaurante.Api.DTOs.Auth;

public class LoginRequest
{
    [Required, EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required, MinLength(6)]
    public string Contrasena { get; set; } = string.Empty;
}