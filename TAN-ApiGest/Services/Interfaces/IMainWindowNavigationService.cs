using System.Windows.Controls;

namespace TanApiGest.Services.Interfaces;

/// <summary>Navigation entre les « pages » (UserControl) affichées dans la fenêtre principale.</summary>
public interface IMainWindowNavigationService
{
    UserControl CurrentContent { get; }
    event Action? CurrentContentChanged;

    void Register<TViewModel, TView>() where TViewModel : class where TView : UserControl;
    void NavigateTo<TViewModel>() where TViewModel : class;
}
