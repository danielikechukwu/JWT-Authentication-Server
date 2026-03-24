using System.ComponentModel.DataAnnotations;

namespace JWTAuthenticationServer.Models;

public class SigningKey
{
    [Key]
    public int Id { get; set; }
    
    // RSA Private key
    [Required]
    public string KeyId { get; set; }
    
    [Required]
    public string PrivateKey { get; set; }
    
    [Required]
    public string PublicKey { get; set; }
    
    public bool IsActive { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    // Date when the key is set to expire
    [Required]
    public DateTime ExpiresAt { get; set; }
}