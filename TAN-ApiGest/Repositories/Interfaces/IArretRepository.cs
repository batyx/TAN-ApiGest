using System.Collections.ObjectModel;
using TanApiGest.DTOs.Responses;
using TanApiGest.Models;

namespace TanApiGest.Repositories.Interfaces;

/// <summary>Accès aux arrêts du réseau TAN.</summary>
public interface IArretRepository
{
    /// <summary>Récupère tous les arrêts, avec pour chacun les lignes qui le desservent.</summary>
    Task<ApiResponse<Collection<Arret>>> GetAllAsync(CancellationToken cancellationToken = default);
}
