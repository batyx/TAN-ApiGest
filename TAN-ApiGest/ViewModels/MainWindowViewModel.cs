using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using TanApiGest.Properties;
using TanApiGest.Services.Interfaces;
using TanApiGest.ViewModels.Components;

namespace TanApiGest.ViewModels;

/// <summary>
/// ViewModel de la fenêtre principale : menu latéral, vue courante et Snackbar global.
/// </summary>
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IMainWindowNavigationService _navigationService;
    private readonly ISnackbarService _snackbarService;

    public string Title => AppSettings.APP_NAME;

    public Collection<MainMenuItemNavigation> MainMenuItems { get; } = new();

    [ObservableProperty]
    public partial MainMenuItemNavigation SelectedMenuItem { get; set; } = null!;

    [ObservableProperty]
    public partial bool IsMenuOpen { get; set; } = false;

    public UserControl? CurrentView => _navigationService.CurrentContent;

    /// <summary>Bindée sur MessageQueue="{Binding MessageQueue}" dans MainWindow.xaml.</summary>
    public object MessageQueue => _snackbarService.MessageQueue;

    /// <summary>Couleur de bordure du Snackbar, mise à jour à chaque message.</summary>
    [ObservableProperty]
    public partial Brush SnackbarBorderBrush { get; set; } = Brushes.Transparent;

    public MainWindowViewModel(IMainWindowNavigationService navigationService, ISnackbarService snackbarService)
    {
        _navigationService = navigationService;
        _snackbarService = snackbarService;

        _navigationService.CurrentContentChanged += () => OnPropertyChanged(nameof(CurrentView));
        _snackbarService.MessageBrushChanged += brush => SnackbarBorderBrush = brush;

        MainMenuItems.Add(new MainMenuItemNavigation
        {
            Label = "Accueil",
            IconType = "Home",
            NavigateTo = () => _navigationService.NavigateTo<HomeViewModel>()
        });
    }

    /// <summary>Appelée une fois la fenêtre chargée : affiche la première entrée du menu.</summary>
    public void NavigateToDefault() => SelectedMenuItem = MainMenuItems[0];

    partial void OnSelectedMenuItemChanged(MainMenuItemNavigation value)
    {
        if (value is null) return;

        value.NavigateTo();
        IsMenuOpen = false;
    }
}
