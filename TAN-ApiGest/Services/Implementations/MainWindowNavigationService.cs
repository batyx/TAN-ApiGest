using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;
using TanApiGest.Services.Interfaces;

namespace TanApiGest.Services.Implementations;

/// <summary>
/// Singleton : CurrentContent est le même pour toute l'application.
/// Seule classe autorisée à résoudre les vues via le conteneur DI.
/// Les vues navigables sont déclarées par Register&lt;TViewModel, TView&gt;()
/// dans App.xaml.cs ; NavigateTo refuse toute vue non déclarée.
/// </summary>
public class MainWindowNavigationService : IMainWindowNavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<Type, Type> _registeredViews = new();

    public UserControl CurrentContent { get; private set; } = null!;

    /// <summary>Signale aux abonnés (MainWindowViewModel) que la vue courante a changé.</summary>
    public event Action? CurrentContentChanged;

    public MainWindowNavigationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void Register<TViewModel, TView>()
        where TViewModel : class
        where TView : UserControl
    {
        _registeredViews.Add(typeof(TViewModel), typeof(TView));
    }

    public void NavigateTo<TViewModel>()
        where TViewModel : class
    {
        if (!_registeredViews.TryGetValue(typeof(TViewModel), out Type? viewType))
        {
            throw new InvalidOperationException(
                $"La vue associée à '{typeof(TViewModel).Name}' n'est pas enregistrée. " +
                $"Ajoutez navigation.Register<{typeof(TViewModel).Name}, TView>() dans App.xaml.cs.");
        }

        CurrentContent = (UserControl)_serviceProvider.GetRequiredService(viewType);
        CurrentContentChanged?.Invoke();
    }
}
