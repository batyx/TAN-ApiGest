using System.Windows.Media;

namespace TanApiGest.Services.Interfaces;

/// <summary>
/// Affichage de messages courts à l'utilisateur. Les ViewModels n'appellent
/// que ShowSuccess / ShowError / ShowInfo, sans connaître le contrôle graphique.
/// </summary>
public interface ISnackbarService
{
    /// <summary>Exposée en object pour que cette interface ne dépende pas de MaterialDesignThemes.</summary>
    object MessageQueue { get; }

    /// <summary>Prévient la fenêtre principale de la couleur du dernier message.</summary>
    event Action<Brush>? MessageBrushChanged;

    void ShowMessage(string message, SnackbarMessageType type);
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowInfo(string message);
}
