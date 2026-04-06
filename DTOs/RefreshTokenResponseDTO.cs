namespace JWTAuthenticationServer.DTOs;

public class RefreshTokenResponseDTO
{
    public string Token { get; set; }
    
    public string RefreshToken { get; set; }
}