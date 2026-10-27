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
}
