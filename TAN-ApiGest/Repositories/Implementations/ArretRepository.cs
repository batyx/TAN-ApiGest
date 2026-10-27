using System.Collections.ObjectModel;
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

        // L'API répond toujours 200 avec un booléen (voir routes/api.php) :
        //   true  → ligne ajoutée ;
        //   false → arrêt introuvable, ou ligne déjà présente sur l'arrêt.
        // Le statut HTTP ne suffit donc pas : il faut lire le contenu.
        ApiResponse<bool> result =
            await _api.PostAsync<bool>(ApiEndpoints.Arrets.AddLigne, routeParams, body, cancellationToken);

        if (!result.IsSuccess)
            return ApiResponse<Ligne>.Failure(result.ErrorMessage);

        if (!result.Data)
            return ApiResponse<Ligne>.Failure($"Ajout refusé par l'API : arrêt {codeLieu} introuvable ou ligne {numLigne} déjà présente.");

        return ApiResponse<Ligne>.Success(new Ligne { NumLigne = numLigne });
    }

    public async Task<ApiResponse<bool>> UpdateLibelleAsync(string codeLieu, string libelle, CancellationToken cancellationToken = default)
    {
        var routeParams = new Dictionary<string, string> { { "codeLieu", codeLieu } };

        // Seul le nom est envoyé : l'API ne modifie que les champs présents.
        var body = new { libelle };

        // Comme pour l'ajout : statut 200 dans tous les cas, true / false dans le contenu.
        ApiResponse<bool> result =
            await _api.PutAsync<bool>(ApiEndpoints.Arrets.Update, routeParams, body, cancellationToken);

        if (!result.IsSuccess)
            return result;

        if (!result.Data)
            return ApiResponse<bool>.Failure($"Modification refusée par l'API : arrêt {codeLieu} introuvable ou nom vide.");

        return result;
    }

    public async Task<ApiResponse<bool>> ResetAsync(CancellationToken cancellationToken = default)
    {
        ApiResponse<bool> result = await _api.GetAsync<bool>(ApiEndpoints.Data.Reset, null, cancellationToken);

        if (result.IsSuccess && !result.Data)
            return ApiResponse<bool>.Failure("La réinitialisation des données a échoué.");

        return result;
    }
}
