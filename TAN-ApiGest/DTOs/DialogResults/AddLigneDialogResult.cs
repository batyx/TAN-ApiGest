using TanApiGest.Services.Interfaces;

namespace TanApiGest.DTOs.DialogResults;

/// <summary>Résultat du dialogue « Ajouter une ligne » : le code saisi, déjà nettoyé.</summary>
public class AddLigneDialogResult : IDialogResult
{
    public bool Confirmed { get; set; }
    public string NumLigne { get; set; } = string.Empty;
}
