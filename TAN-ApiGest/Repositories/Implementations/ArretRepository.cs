using System.Collections.ObjectModel;
using TanApiGest.DTOs.Responses;
using TanApiGest.Http.Implementations;
using TanApiGest.Http.Interfaces;
using TanApiGest.Models;
using TanApiGest.Repositories.Interfaces;

namespace TanApiGest.Repositories.Implementations;

/// <summary>
/// Implémentation de IArretRepository s'appuyant sur l'API REST V2.
/// Le succès se lit dans le code HTTP (traité par Api) : chaque méthode se
/// contente de construire la requête et de typer la réponse.
/// </summary>
public class ArretRepository : IArretRepository
{
    private readonly IApi _api;

    public ArretRepository(IApi api)
    {
        _api = api;
    }

    public Task<ApiResponse<Collection<Arret>>> GetAllAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<Collection<Arret>>(ApiEndpoints.Arrets.GetAll, null, cancellationToken);

    public Task<ApiResponse<Ligne>> AddLigneAsync(string codeLieu, string numLigne, CancellationToken cancellationToken = default)
    {
        var routeParams = new Dictionary<string, string> { { "codeLieu", codeLieu } };

        // Le nom de la propriété anonyme donne le nom du champ JSON : { "numLigne": "..." }.
        var body = new { numLigne };

        // 201 + { "numLigne": "..." } : désérialisé directement en Ligne.
        return _api.PostAsync<Ligne>(ApiEndpoints.Arrets.AddLigne, routeParams, body, cancellationToken);
    }

    public Task<ApiResponse<Arret>> UpdateLibelleAsync(string codeLieu, string libelle, CancellationToken cancellationToken = default)
    {
        var routeParams = new Dictionary<string, string> { { "codeLieu", codeLieu } };

        // Seul le nom est envoyé : l'API ne modifie que les champs présents.
        var body = new { libelle };

        // 200 + l'arrêt modifié.
        return _api.PutAsync<Arret>(ApiEndpoints.Arrets.Update, routeParams, body, cancellationToken);
    }

    public Task<ApiResponse<Collection<Arret>>> ResetAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<Collection<Arret>>(ApiEndpoints.Data.Reset, null, cancellationToken);
}
