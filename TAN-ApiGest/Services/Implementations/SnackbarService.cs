using MaterialDesignThemes.Wpf;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TanApiGest.Services.Interfaces;

namespace TanApiGest.Services.Implementations;

/// <summary>
/// Enregistré en Singleton : une seule MessageQueue partagée garantit que
/// tous les messages arrivent au même Snackbar (voir MainWindow.xaml).
/// </summary>
public class SnackbarService : ISnackbarService
{
    private readonly SnackbarMessageQueue _messageQueue = new();

    private static readonly SolidColorBrush BrushError = new(Color.FromRgb(166, 25, 46));
    private static readonly SolidColorBrush BrushSuccess = new(Color.FromRgb(4, 179, 79));
    private static readonly SolidColorBrush BrushWarning = new(Color.FromRgb(230, 140, 0));
    private static readonly SolidColorBrush BrushInfo = new(Color.FromRgb(0, 87, 184));

    public object MessageQueue => _messageQueue;

    public event Action<Brush>? MessageBrushChanged;

    public void ShowSuccess(string message) => ShowMessage(message, SnackbarMessageType.Success);
    public void ShowError(string message) => ShowMessage(message, SnackbarMessageType.Error);
    public void ShowInfo(string message) => ShowMessage(message, SnackbarMessageType.Info);

    public void ShowMessage(string message, SnackbarMessageType type)
    {
        SolidColorBrush brush = type switch
        {
            SnackbarMessageType.Error => BrushError,
            SnackbarMessageType.Success => BrushSuccess,
            SnackbarMessageType.Warning => BrushWarning,
            _ => BrushInfo
        };

        string prefix = type switch
        {
            SnackbarMessageType.Error => "⛔ ",
            SnackbarMessageType.Success => "✅ ",
            SnackbarMessageType.Warning => "⚠️ ",
            _ => "ℹ️ "
        };

        // Les contrôles WPF doivent être créés sur le thread UI : seul ce service
        // connaît ce détail technique, pas les ViewModels qui l'appellent.
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBrushChanged?.Invoke(brush);

            TextBlock content = new() { Text = prefix + message, Foreground = brush, FontWeight = FontWeights.Medium };
            _messageQueue.Enqueue(
                content: content,
                actionContent: "",
                actionHandler: () => { },
                promote: type == SnackbarMessageType.Error);
        });
    }
}
