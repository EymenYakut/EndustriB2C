using System.Diagnostics;

namespace EndustriB2C;

public class SpaStartService : IHostedService
{
    private readonly ILogger<SpaStartService> _logger;
    private readonly IWebHostEnvironment _env;

    public SpaStartService(ILogger<SpaStartService> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (await IsSpaRunning(cancellationToken))
        {
            _logger.LogInformation("Angular development server already running on https://localhost:44408");
            return;
        }

        var clientApp = Path.Combine(_env.ContentRootPath, "ClientApp");
        var startInfo = new ProcessStartInfo
        {
            FileName = OperatingSystem.IsWindows() ? "cmd.exe" : "npm",
            Arguments = OperatingSystem.IsWindows() ? "/c npm start" : "start",
            WorkingDirectory = clientApp,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(startInfo);
        _logger.LogInformation("Angular development server starting (pid {Pid})", process?.Id);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task<bool> IsSpaRunning(CancellationToken cancellationToken)
    {
        try
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true
            };
            using var client = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(2) };
            using var response = await client.GetAsync("https://localhost:44408", cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
