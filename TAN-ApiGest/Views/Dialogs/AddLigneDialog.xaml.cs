using System.Windows.Controls;
using TanApiGest.ViewModels.Dialogs;

namespace TanApiGest.Views.Dialogs;

public partial class AddLigneDialog : UserControl
{
    public AddLigneDialog(AddLigneDialogViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
