using System.Collections.ObjectModel;
using TanApiGest.DTOs.Responses;
using TanApiGest.Models;

namespace TanApiGest.Repositories.Interfaces;

/// <summary>Accès aux arrêts du réseau TAN.</summary>
public interface IArretRepository
{
    /// <summary>Récupère tous les arrêts, avec pour chacun les lignes qui le desservent.</summary>
    Task<ApiResponse<Collection<Arret>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// F3 : ajoute la ligne numLigne à l'arrêt codeLieu. En cas de succès,
    /// Data contient la ligne ajoutée, prête à être insérée dans Arret.Lignes.
    /// </summary>
    Task<ApiResponse<Ligne>> AddLigneAsync(string codeLieu, string numLigne, CancellationToken cancellationToken = default);
}
