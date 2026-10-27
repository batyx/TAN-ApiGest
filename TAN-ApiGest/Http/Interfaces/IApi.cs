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

    // POST / PUT / DELETE seront ajoutés avec les fonctionnalités d'édition,
    // lorsque l'API exposera les routes correspondantes.
}
