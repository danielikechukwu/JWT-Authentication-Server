using System.Security.Cryptography;
using JWTAuthenticationServer.Data;
using JWTAuthenticationServer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace JWTAuthenticationServer.Services;

public class KeyRotationService : BackgroundService
{
    // Service provider is used to create a scoped service lifetime.
    private readonly IServiceProvider _serviceProvider;

    // Sets how frequently keys should be rotated; here it’s every 7 days.
    private readonly TimeSpan _rotationInterval = TimeSpan.FromDays(7);

    public KeyRotationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // This method is executed when the background service starts.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Perform the key rotate logic.
            await RotateKeyRotationAsync();

            await Task.Delay(_rotationInterval, stoppingToken);
        }
    }

    private async Task RotateKeyRotationAsync()
    {
        // Create a new scope of dependency injection
        using var scope = _serviceProvider.CreateScope();

        // Retrieve the database service from the service provider
        var context = scope.ServiceProvider.GetRequiredService<JWTDbContext>();

        // Query the database context for the currently active signing key
        var activeKey = await context.SigningKeys.FirstOrDefaultAsync(k => k.IsActive);

        if (activeKey == null || activeKey.ExpiresAt < DateTime.UtcNow.AddDays(10))
        {
            // If there's an active key, mark it as inactive.
            if (activeKey != null)
            {
                // Mark the current key as inactive since it’s about to be replaced.
                activeKey.IsActive = false;

                // Update the current key in database
                context.SigningKeys.Update(activeKey);
            }

            // Generate a new RSA key pair
            using var rsa = RSA.Create(2048);

            // Export the private key as a Base64-encoded
            var privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());

            // Export the public key as a Base64-encoded string.
            var publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

            // Generate a unique identifier for the new key
            var newKeyId = Guid.NewGuid().ToString();

            // Create a new SigningKey entity with the new RSA key details.
            var newKey = new SigningKey
            {
                KeyId = newKeyId,
                PrivateKey = privateKey,
                PublicKey = publicKey,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddYears(1)
            };
            
            // Add new key to the database
            await context.SigningKeys.AddAsync(newKey);
            
            // Save the change to the database
            await context.SaveChangesAsync();
        }
    }
}