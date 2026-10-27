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
/// Exemple : { "codeLieu": "COMM", "libelle": "Commerce", "distance": null, "ligne": [ { "numLigne": "1" } ] }
/// </summary>
public class Arret
{
    private ObservableCollection<Ligne> _lignes = new();

    /// <summary>Identifiant de l'arrêt (lieu), ex. "COMM".</summary>
    [JsonPropertyName("codeLieu")]
    public string CodeLieu { get; set; } = string.Empty;

    /// <summary>Nom de l'arrêt affiché aux voyageurs, ex. "Commerce".</summary>
    [JsonPropertyName("libelle")]
    public string Libelle { get; set; } = string.Empty;

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

    /// <summary>Indique si la ligne de code numLigne dessert déjà l'arrêt (comparaison sans casse).</summary>
    public bool HasLigne(string numLigne)
        => Lignes.Any(ligne => string.Equals(ligne.NumLigne, numLigne, StringComparison.OrdinalIgnoreCase));
}
