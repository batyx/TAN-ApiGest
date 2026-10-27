namespace TanApiGest.ViewModels.Components;

/// <summary>
/// Entrée du menu latéral (couche ViewModel) : données d'affichage et action
/// de navigation associée. Distincte du composant graphique IconAndTextMenuItem
/// (Views/Components), qui ne contient aucune logique.
/// </summary>
public class MainMenuItemNavigation
{
    public required string Label { get; set; }

    /// <summary>Nom d'une icône Material Design (énumération PackIconKind), ex. "Home".</summary>
    public required string IconType { get; set; }

    /// <summary>Exécutée à la sélection de l'entrée — définie dans MainWindowViewModel.</summary>
    public required Action NavigateTo { get; set; }
}
