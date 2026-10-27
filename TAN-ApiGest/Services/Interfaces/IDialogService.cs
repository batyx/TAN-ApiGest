using System.Windows.Controls;

namespace TanApiGest.Services.Interfaces;

/// <summary>
/// Affichage de fenêtres modales (dialogues Material Design). Le ViewModel
/// appelant ne connaît que le ViewModel du dialogue, jamais sa vue.
/// </summary>
public interface IDialogService
{
    /// <summary>Associe un ViewModel de dialogue à sa vue (appelé dans App.xaml.cs).</summary>
    void Register<TViewModel, TDialog>() where TDialog : UserControl;

    /// <summary>
    /// Ouvre le dialogue associé à TViewModel et attend sa fermeture.
    /// configure permet à l'appelant de renseigner le ViewModel du dialogue
    /// avant l'affichage (ex. : l'arrêt concerné). Renvoie null si l'utilisateur annule.
    /// </summary>
    Task<TResult?> ShowAsync<TViewModel, TResult>(Action<TViewModel>? configure = null) where TResult : class, IDialogResult;
}
