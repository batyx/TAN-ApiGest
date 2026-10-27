using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace TanApiGest.Models;

/// <summary>
/// Arrêt du réseau TAN tel que renvoyé par GET /arrets.
/// <para>
/// Les noms des champs JSON ne suivent pas les conventions C# (camelCase,
/// français, « ligne » au singulier pour un tableau). Plutôt que de reprendre
/// ces noms dans le code, on garde des propriétés C# bien nommées et on
/// indique le nom JSON correspondant avec [JsonPropertyName].
/// </para>
/// <para>
/// F4 : le nom est modifiable dans la grille. La classe dérive donc de
/// ObservableObject, pour que la grille se mette à jour quand le code remet
/// l'ancien nom après un échec, et mémorise le nom d'origine (OriginalLibelle).
/// </para>
/// Exemple : { "codeLieu": "COMM", "libelle": "Commerce", "distance": null, "ligne": [ { "numLigne": "1" } ] }
/// </summary>
public partial class Arret : ObservableObject, IJsonOnDeserialized
{
    private ObservableCollection<Ligne> _lignes = new();

    /// <summary>Identifiant de l'arrêt (lieu), ex. "COMM". Non modifiable dans l'application.</summary>
    [JsonPropertyName("codeLieu")]
    public string CodeLieu { get; set; } = string.Empty;

    /// <summary>Nom de l'arrêt affiché aux voyageurs, ex. "Commerce". Modifiable (F4).</summary>
    [ObservableProperty]
    [JsonPropertyName("libelle")]
    public partial string Libelle { get; set; } = string.Empty;

    /// <summary>
    /// Nom tel qu'il est enregistré côté API : renseigné au chargement, puis
    /// après chaque modification réussie. Permet de savoir si le nom a changé
    /// et de le rétablir si l'API refuse la modification.
    /// </summary>
    [JsonIgnore]
    public string OriginalLibelle { get; private set; } = string.Empty;

    /// <summary>
    /// Lignes desservant l'arrêt. Dans le JSON, le champ s'appelle « ligne »
    /// au singulier bien qu'il contienne un tableau.
    /// <para>
    /// ObservableCollection (F3) : quand une ligne est ajoutée, la grille des
    /// lignes et le compteur (Lignes.Count) se mettent à jour automatiquement.
    /// </para>
    /// Si l'API renvoie null, on garde une collection vide : le reste du code
    /// n'a jamais à tester la nullité.
    /// </summary>
    [JsonPropertyName("ligne")]
    public ObservableCollection<Ligne> Lignes
    {
        get => _lignes;
        set => _lignes = value ?? new ObservableCollection<Ligne>();
    }

    // Le champ JSON "distance" n'est pas repris : il vaut null pour la liste
    // complète et ne sert qu'aux recherches par position. Les champs JSON
    // sans propriété correspondante sont simplement ignorés à la lecture.

    /// <summary>Appelée par System.Text.Json une fois l'objet rempli : on mémorise le nom d'origine.</summary>
    void IJsonOnDeserialized.OnDeserialized() => OriginalLibelle = Libelle;

    /// <summary>Enregistre le nom actuel comme nom d'origine (après une modification réussie).</summary>
    public void AcceptLibelle() => OriginalLibelle = Libelle;

    /// <summary>Rétablit le nom d'origine (modification annulée ou refusée).</summary>
    public void RevertLibelle() => Libelle = OriginalLibelle;

    /// <summary>Indique si la ligne de code numLigne dessert déjà l'arrêt (comparaison sans casse).</summary>
    public bool HasLigne(string numLigne)
        => Lignes.Any(ligne => string.Equals(ligne.NumLigne, numLigne, StringComparison.OrdinalIgnoreCase));
}
