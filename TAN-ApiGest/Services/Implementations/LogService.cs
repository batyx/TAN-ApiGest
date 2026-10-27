using Serilog;
using TanApiGest.Services.Interfaces;

namespace TanApiGest.Services.Implementations;

/// <summary>Simple délégation vers le logger Serilog configuré dans App.xaml.cs.</summary>
public class LogService : ILogService
{
    private readonly ILogger _logger = Log.Logger;

    public void Debug(string message) => _logger.Debug(message);
    public void Information(string message) => _logger.Information(message);
    public void Warning(string message) => _logger.Warning(message);

    public void Error(string message, Exception? exception = null)
    {
        if (exception is null) _logger.Error(message);
        else _logger.Error(exception, message);
    }

    public void Fatal(string message, Exception? exception = null)
    {
        if (exception is null) _logger.Fatal(message);
        else _logger.Fatal(exception, message);
    }
}
