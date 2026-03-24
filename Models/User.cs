using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthenticationServer.Models;

[Index(nameof(Email), Name = "IX_Unique_Email", IsUnique = true)]
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [EmailAddress]
    public string Email { get; set; }  
    
    [Required]
    public string FirstName { get; set; }
    
    public string? LastName { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Password { get; set; }
    
    public ICollection<UserRole> UserRoles { get; set; }
    
}