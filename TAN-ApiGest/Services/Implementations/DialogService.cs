using MaterialDesignThemes.Wpf;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using TanApiGest.Services.Interfaces;

namespace TanApiGest.Services.Implementations;

/// <summary>
/// Même principe que dans AliPurchase : un dictionnaire ViewModel → type de
/// vue de dialogue. ShowAsync résout la vue via le conteneur DI et l'affiche
/// dans le DialogHost « RootDialog » de l'écran courant (voir ArretsView.xaml).
/// </summary>
public class DialogService : IDialogService
{
    /// <summary>Identifiant du DialogHost déclaré dans les vues qui ouvrent des dialogues.</summary>
    private const string DialogHostIdentifier = "RootDialog";

    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _map = new();

    public DialogService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Register<TViewModel, TDialog>() where TDialog : UserControl
        => _map[typeof(TViewModel)] = typeof(TDialog);

    public async Task<TResult?> ShowAsync<TViewModel, TResult>(Action<TViewModel>? configure = null)
        where TResult : class, IDialogResult
    {
        if (!_map.TryGetValue(typeof(TViewModel), out Type? dialogType))
            throw new InvalidOperationException(
                $"Aucun dialogue enregistré pour '{typeof(TViewModel).Name}'. " +
                $"Ajoutez dialogService.Register<{typeof(TViewModel).Name}, VotreDialog>() dans App.xaml.cs.");

        UserControl dialog = (UserControl)_serviceProvider.GetRequiredService(dialogType);

        // Le ViewModel du dialogue est son DataContext (injecté dans son constructeur).
        if (configure is not null && dialog.DataContext is TViewModel viewModel)
            configure(viewModel);

        object? result = await DialogHost.Show(dialog, DialogHostIdentifier);

        return result as TResult;
    }
}
