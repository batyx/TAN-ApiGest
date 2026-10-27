using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaterialDesignThemes.Wpf;
using TanApiGest.DTOs.DialogResults;
using TanApiGest.Models;

namespace TanApiGest.ViewModels.Dialogs;

/// <summary>
/// F3 : saisie du code de la ligne à ajouter à un arrêt.
/// Construit sur le modèle d'AddCategoryDialogViewModel (AliPurchase).
/// </summary>
public partial class AddLigneDialogViewModel : ObservableObject
{
    /// <summary>Arrêt concerné, renseigné par l'appelant via IDialogService.ShowAsync (affichage seul).</summary>
    [ObservableProperty]
    public partial Arret? Arret { get; set; }

    /// <summary>Code de la ligne saisi (ex. « C6 »).</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ConfirmCommand))]
    public partial string NumLigne { get; set; } = string.Empty;

    private bool CanConfirm() => !string.IsNullOrWhiteSpace(NumLigne);

    [RelayCommand(CanExecute = nameof(CanConfirm))]
    private void Confirm()
    {
        // Le ViewModel connaît les données saisies : c'est lui qui construit le
        // résultat typé et le transmet au DialogService en fermant le DialogHost.
        // Les codes de ligne TAN sont en majuscules ("C6") : on normalise la saisie.
        AddLigneDialogResult result = new()
        {
            Confirmed = true,
            NumLigne = NumLigne.Trim().ToUpperInvariant()
        };
        DialogHost.CloseDialogCommand.Execute(result, null);
    }

    [RelayCommand]
    private void Cancel() => DialogHost.CloseDialogCommand.Execute(null, null);
}
