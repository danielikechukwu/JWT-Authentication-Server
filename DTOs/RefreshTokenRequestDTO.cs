using System.ComponentModel.DataAnnotations;

namespace JWTAuthenticationServer.DTOs;

public class RefreshTokenRequestDTO
{
    [Required]
    public string RefreshToken { get; set; }
    
    [Required]
    public string ClientId { get; set; }
}