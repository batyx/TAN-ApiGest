using System.Windows.Controls;
using TanApiGest.ViewModels;

namespace TanApiGest.Views;

public partial class HomeView : UserControl
{
    public HomeView(HomeViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
