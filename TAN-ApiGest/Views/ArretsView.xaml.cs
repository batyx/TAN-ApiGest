using System.Windows.Controls;
using TanApiGest.ViewModels;

namespace TanApiGest.Views;

public partial class ArretsView : UserControl
{
    public ArretsView(ArretsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        // Si l'utilisateur change d'écran pendant un chargement, la requête est annulée.
        Unloaded += (_, _) => viewModel.CancelPendingOperations();
    }
}
