namespace JWTAuthenticationServer.DTOs;

public class LoginResponseDTO
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}