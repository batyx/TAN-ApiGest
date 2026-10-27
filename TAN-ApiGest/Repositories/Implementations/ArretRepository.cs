using System.Collections.ObjectModel;
using System.Text.Json;
using TanApiGest.DTOs.Responses;
using TanApiGest.Http.Implementations;
using TanApiGest.Http.Interfaces;
using TanApiGest.Models;
using TanApiGest.Repositories.Interfaces;

namespace TanApiGest.Repositories.Implementations;

/// <summary>Implémentation de IArretRepository s'appuyant sur l'API REST.</summary>
public class ArretRepository : IArretRepository
{
    private readonly IApi _api;

    public ArretRepository(IApi api)
    {
        _api = api;
    }

    public Task<ApiResponse<Collection<Arret>>> GetAllAsync(CancellationToken cancellationToken = default)
        => _api.GetAsync<Collection<Arret>>(ApiEndpoints.Arrets.GetAll, null, cancellationToken);

    public async Task<ApiResponse<Ligne>> AddLigneAsync(string codeLieu, string numLigne, CancellationToken cancellationToken = default)
    {
        var routeParams = new Dictionary<string, string> { { "codeLieu", codeLieu } };

        // Le nom de la propriété anonyme donne le nom du champ JSON : { "numLigne": "..." }.
        var body = new { numLigne };

        // Le contenu de la réponse n'est pas documenté : seul le statut HTTP
        // fait foi. JsonElement accepte n'importe quel JSON sans le typer.
        ApiResponse<JsonElement> result =
            await _api.PostAsync<JsonElement>(ApiEndpoints.Arrets.AddLigne, routeParams, body, cancellationToken);

        return result.IsSuccess
            ? ApiResponse<Ligne>.Success(new Ligne { NumLigne = numLigne })
            : ApiResponse<Ligne>.Failure(result.ErrorMessage);
    }
}
