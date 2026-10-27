using TanApiGest.DTOs.Responses;

namespace TanApiGest.Http.Interfaces;

/// <summary>
/// Contrat du client HTTP. Les repositories dépendent de cette interface,
/// jamais de la classe concrète Api.
/// </summary>
public interface IApi
{
    /// <summary>
    /// Envoie une requête GET et désérialise la réponse JSON en T.
    /// Le cancellationToken permet d'annuler la requête (ex. : l'utilisateur quitte l'écran).
    /// </summary>
    Task<ApiResponse<T>> GetAsync<T>(string route, Dictionary<string, string>? routeParams = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Envoie une requête POST dont le corps est l'objet body sérialisé en JSON,
    /// et désérialise la réponse en T (F3 : ajout d'une ligne à un arrêt).
    /// </summary>
    Task<ApiResponse<T>> PostAsync<T>(string route, Dictionary<string, string>? routeParams = null, object? body = null, CancellationToken cancellationToken = default);
}
