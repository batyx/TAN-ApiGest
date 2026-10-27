using System.Text.Json.Serialization;

namespace TanApiGest.Models;

/// <summary>
/// Ligne du réseau TAN (tram, Busway, Chronobus, bus, Luciole, Navibus...)
/// telle qu'elle apparaît dans la réponse de GET /arrets.
/// </summary>
public class Ligne
{
    /// <summary>
    /// Code de la ligne : "1", "4", "C6", "23"... C'est une chaîne et
    /// non un nombre, car beaucoup de codes contiennent des lettres.
    /// </summary>
    [JsonPropertyName("numLigne")]
    public string NumLigne { get; set; } = string.Empty;
}
