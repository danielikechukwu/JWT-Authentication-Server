namespace JWTAuthenticationServer.Models;

public class TokenStorage
{
    // Stores the current access token used for authenticated API requests.
    public string AccessToken { get; set; } = string.Empty;
    
    // Stores the current refresh token used to obtain new access tokens.
    public string RefreshToken { get; set; } = string.Empty;
    
    // Stores the client identifier associated with the tokens.
    public string ClientId { get; set; } = string.Empty;

}