using System.Security.Claims;
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

    private readonly ILogger<UsersController> _logger;

    public UsersController(JWTDbContext jwtDbContext, ILogger<UsersController> logger)
    {
        _jwtDbContext = jwtDbContext;

        _logger = logger;
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

        _logger.LogInformation($"Email Claim {emailClaim}");

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

    // Updates the authenticated user's profile.
    [HttpPut("UpdateProfile")]
    [Authorize]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDTO updateProfileDto)
    {
        // Validate incoming model
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Extract the user's email from the JWT claims.
        var emailClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email);

        if (emailClaim == null)
        {
            return Unauthorized(new { message = "Invalid token: Email claim missing" });
        }

        string userEmail = emailClaim.Value;

        // Retrieve the user from the database.
        var user = await _jwtDbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == userEmail.ToLower());

        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        if (!string.IsNullOrEmpty(updateProfileDto.FirstName))
        {
            user.FirstName = updateProfileDto.FirstName;
        }

        if (!string.IsNullOrEmpty(updateProfileDto.LastName))
        {
            user.LastName = updateProfileDto.LastName;
        }

        if (!string.IsNullOrEmpty(updateProfileDto.Email))
        {
            // Check if the new email is already taken by another user.
            var emailExists = await _jwtDbContext.Users.AnyAsync(u =>
                u.Email.ToLower() == updateProfileDto.Email.ToLower() && u.Id != user.Id);

            if (emailExists)
            {
                return Conflict(new { message = "Email is already registered" });
            }

            user.Email = updateProfileDto.Email;
        }

        if (!string.IsNullOrEmpty(updateProfileDto.Password))
        {
            // Hash the new password before storing.
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(updateProfileDto.Password);

            user.Password = hashedPassword;
        }

        // Save the changes to the database.
        _jwtDbContext.Users.Update(user);

        await _jwtDbContext.SaveChangesAsync();

        return Ok(new { message = "Profile updated successfully" });
    }
}