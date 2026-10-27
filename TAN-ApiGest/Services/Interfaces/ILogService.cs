namespace TanApiGest.Services.Interfaces;

/// <summary>
/// Abstraction de la journalisation : les classes dépendent de cette
/// interface et non de Serilog, que l'on peut remplacer sans les modifier.
/// </summary>
public interface ILogService
{
    void Debug(string message);
    void Information(string message);
    void Warning(string message);
    void Error(string message, Exception? exception = null);
    void Fatal(string message, Exception? exception = null);
}
