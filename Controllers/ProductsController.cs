using JWTResource_server.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JWTResource_server.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Secures all endpoints in this controller
public class ProductsController : ControllerBase
{
    // In-memory list to store products
    private static readonly List<Product> Products = new List<Product>
    {
        new Product { Id = 1, Name = "Product A", Price = 10.0M, Description = "Test Product A" },
        new Product { Id = 2, Name = "Product B", Price = 20.0M, Description = "Test Product B"  },
        new Product { Id = 3, Name = "Product C", Price = 30.0M, Description = "Test Product C"  }
    };

    private static int _nextId = 4; // To auto increment product id.
    
    // Retrieve all products
    [Authorize]
    [HttpGet("GetAllProducts")]
    public ActionResult<IEnumerable<Product>> GetAllProducts()
    {
        return Ok(Products);
    }
    
    // Retrieves a specific product by ID.
    [Authorize(Roles = "Admin")]
    [HttpGet("GetProductById/{id}",  Name = "GetProductById")]
    public ActionResult<Product> GetProductById(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);

        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found"});
        }
        
        return Ok(product);
    }
    
    // Create new product
    [Authorize(Roles = "User")]
    [HttpPost("AddProduct")]
    public ActionResult<Product> AddProduct([FromBody] Product product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        product.Id = _nextId++;
        Products.Add(product);
        return CreatedAtRoute("GetProductById", new { id = product.Id },  product);
    }

    // Authorize with either "Admin" AND "User" Roles
    // Specifying multiple roles in [Authorize(Roles = "Admin,User")]
    // means the user must be in EITHER the "Admin" OR the "User" role (logical OR).
    [Authorize(Roles = "Admin,User")]
    [HttpPut("UpdateProduct")]
    public IActionResult UpdateProduct(int id, [FromBody] Product product)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingProduct = Products.FirstOrDefault(p => p.Id == id);

        if (existingProduct == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found"});
        }
        
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;

        return NoContent();
    }

    // Authorize with both Admin or User Role
    // There are two separate [Authorize] attributes with "Admin" and "User" roles which means
    // that users having both roles can access this endpoint.
    [Authorize(Roles = "Admin")]
    [Authorize(Roles = "User")]
    [HttpDelete("DeleteProduct/{id}")]
    public IActionResult DeleteProduct(int id)
    {
        var product = Products.FirstOrDefault(p => p.Id == id);
        
        if (product == null)
        {
            return NotFound(new { message = $"Product with ID {id} not found"});
        }
        
        Products.Remove(product);
        return NoContent();
    }
}