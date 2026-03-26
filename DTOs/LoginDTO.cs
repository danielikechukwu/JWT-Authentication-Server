using System.ComponentModel.DataAnnotations;

namespace JWTAuthenticationServer.DTOs;

public class LoginDTO
{
    [EmailAddress]
    [Required(ErrorMessage = "Email is required")]
    [MaxLength(100, ErrorMessage = "Email must be less than or equal to 100 characters.")]
    public string Email { get; set; }
    
    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [MaxLength(100, ErrorMessage = "Password must be less than or equal to 100 characters.")]
    public string Password { get; set; }
    
    [Required(ErrorMessage = "Client ID is required")]
    public string ClientId { get; set; }
    
}