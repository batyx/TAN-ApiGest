using System.Text.Json.Serialization;

namespace TanApiGest.DTOs.Responses;

/// <summary>
/// Corps d'une réponse en erreur de l'API V2 (404, 409, 422...) :
/// { "message": "Arrêt ZZZZ introuvable." }. Pour une erreur de validation (422),
/// l'API ajoute un champ "errors" détaillé, non utilisé ici : "message" en
/// reprend déjà la première erreur.
/// </summary>
public class ApiErrorResponse
{
    [JsonPropertyName("message")]
    public string? Message { get; set; }
}
