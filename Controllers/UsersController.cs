using JWTAuthenticationServer.Data;
using JWTAuthenticationServer.DTOs;
using JWTAuthenticationServer.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JWTAuthenticationServer.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly JWTDbContext _jwtDbContext;

    public UsersController(JWTDbContext jwtDbContext)
    {
        _jwtDbContext = jwtDbContext;
    }

    // Register a new user
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDto)
    {
        // Validate the incoming model
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if email already exist
        var existingUser =
            await _jwtDbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == registerDto.Email.ToLower());

        if (existingUser != null)
        {
            return Conflict(new { message = "Email is already registered" });
        }

        string hashPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

        // Create a new user entity 
        var newUser = new User()
        {
            FirstName = registerDto.Firstname,
            LastName = registerDto.Lastname,
            Email = registerDto.Email,
            Password = hashPassword,
        };

        await _jwtDbContext.Users.AddAsync(newUser);

        await _jwtDbContext.SaveChangesAsync();

        // Optionally, assign a default role to the new user.
        // For example, assign the "User" role.
        var userRole = await _jwtDbContext.Roles.FirstOrDefaultAsync(r => r.Name == "User");

        if (userRole != null)
        {
            var newUserRole = new UserRole()
            {
                UserId = newUser.Id,
                RoleId = userRole.Id
            };

            await _jwtDbContext.UserRoles.AddAsync(newUserRole);

            await _jwtDbContext.SaveChangesAsync();
        }

        return CreatedAtAction(nameof(GetProfile), new { id = newUser.Id },
            new { message = "User registered successfully" });
    }

    [HttpGet("GetProfile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        // Extract the user email from the jwt claim
        var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);

        if (emailClaim == null)
        {
            return Unauthorized(new { message = "Invalid Token: Email claim missing" });
        }

        string userEmail = emailClaim.Value;

        // Retrieve the user from the database, including roles.
        var user = await _jwtDbContext.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());

        if (user == null)
        {
            return NotFound(new { message = "User not found." });
        }

        // Map the user entity to ProfileDTO.
        var profile = new ProfileDTO()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList(),
        };

        return Ok(profile);
    }
}