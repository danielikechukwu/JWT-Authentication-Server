namespace JWTResource_server.models;
using System.ComponentModel.DataAnnotations;

public class Product
{
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
    
    [StringLength(100)]
    public string Description { get; set; }
    
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }
}