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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            
        }
    }

    private async Task RotateKeyRotationAsync()
    {
        
    }
}