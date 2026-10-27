using System.Windows;
using TanApiGest.ViewModels;

namespace TanApiGest.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Navigation initiale une fois la fenêtre chargée, et non pendant
        // la construction du ViewModel.
        Loaded += (_, _) => viewModel.NavigateToDefault();
    }
}
