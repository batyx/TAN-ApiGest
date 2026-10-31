using System.Collections.ObjectModel;
using TanApiGest.DTOs.Responses;
using TanApiGest.Models;

namespace TanApiGest.Repositories.Interfaces;

/// <summary>
/// Accès aux arrêts du réseau TAN (API V2). En cas d'échec, ErrorMessage
/// contient le message renvoyé par l'API (ex. « Arrêt ZZZZ introuvable. »).
/// </summary>
public interface IArretRepository
{
    /// <summary>Récupère tous les arrêts, avec pour chacun les lignes qui le desservent.</summary>
    Task<ApiResponse<Collection<Arret>>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// F3 : ajoute la ligne numLigne à l'arrêt codeLieu. En cas de succès,
    /// Data contient la ligne ajoutée, prête à être insérée dans Arret.Lignes.
    /// </summary>
    Task<ApiResponse<Ligne>> AddLigneAsync(string codeLieu, string numLigne, CancellationToken cancellationToken = default);

    /// <summary>
    /// F4 : remplace le nom de l'arrêt codeLieu par libelle.
    /// En cas de succès, Data contient l'arrêt tel qu'enregistré par l'API.
    /// </summary>
    Task<ApiResponse<Arret>> UpdateLibelleAsync(string codeLieu, string libelle, CancellationToken cancellationToken = default);

    /// <summary>
    /// Remet tous les arrêts et leurs lignes dans l'état initial : toutes les
    /// modifications faites via l'application sont perdues. En cas de succès,
    /// Data contient la liste réinitialisée (inutile de la recharger).
    /// </summary>
    Task<ApiResponse<Collection<Arret>>> ResetAsync(CancellationToken cancellationToken = default);
}
