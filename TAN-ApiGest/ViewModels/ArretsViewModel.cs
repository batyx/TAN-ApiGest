using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using TanApiGest.DTOs.Responses;
using TanApiGest.Models;
using TanApiGest.Repositories.Interfaces;
using TanApiGest.Services.Interfaces;

namespace TanApiGest.ViewModels;

/// <summary>
/// Écran « Arrêts ».
/// F1 : liste des arrêts gérés par la TAN, avec recherche.
/// F2 : codes des lignes passant par l'arrêt sélectionné.
/// </summary>
public partial class ArretsViewModel : ObservableObject
{
    private readonly IArretRepository _arretRepository;
    private readonly ISnackbarService _snackbarService;
    private readonly ILogService _log;
    private CancellationTokenSource _cts = new();

    /// <summary>Liste complète reçue de l'API ; Arrets n'en est que la partie filtrée.</summary>
    private List<Arret> _allArrets = new();

    /// <summary>Arrêts affichés dans la grille (résultat du filtre de recherche).</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountLabel))]
    public partial ObservableCollection<Arret> Arrets { get; set; } = new();

    /// <summary>Texte saisi dans la zone de recherche (libellé, code d'arrêt ou code de ligne).</summary>
    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    /// <summary>
    /// F2 : arrêt sélectionné dans la grille. Ses lignes (SelectedArret.Lignes)
    /// sont déjà présentes dans la réponse de GET /arrets : aucun appel
    /// supplémentaire à l'API n'est nécessaire. Vaut null si aucune sélection.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedArret))]
    public partial Arret? SelectedArret { get; set; }

    public bool HasSelectedArret => SelectedArret is not null;

    /// <summary>Vrai pendant un appel à l'API : affiche la barre de progression et désactive « Actualiser ».</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadArretsCommand))]
    public partial bool IsBusy { get; set; } = false;

    /// <summary>Ex. : « 12 arrêt(s) affiché(s) sur 1 345 ».</summary>
    public string CountLabel => $"{Arrets.Count:N0} arrêt(s) affiché(s) sur {_allArrets.Count:N0}";

    public ArretsViewModel(IArretRepository arretRepository, ISnackbarService snackbarService, ILogService log)
    {
        _arretRepository = arretRepository;
        _snackbarService = snackbarService;
        _log = log;
    }

    private bool CanRunCommand() => !IsBusy;

    /// <summary>Annule la requête éventuellement en cours et fournit un nouveau jeton.</summary>
    private CancellationToken RenewToken()
    {
        _cts.Cancel();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }

    /// <summary>Appelée par la vue quand elle est déchargée (l'utilisateur change d'écran).</summary>
    public void CancelPendingOperations() => RenewToken();

    /// <summary>F1 : charge la liste des arrêts depuis l'API.</summary>
    [RelayCommand(CanExecute = nameof(CanRunCommand))]
    private async Task LoadArretsAsync()
    {
        _log.Information("ArretsViewModel - LoadArretsAsync");
        IsBusy = true;
        CancellationToken token = RenewToken();
        try
        {
            ApiResponse<Collection<Arret>> result = await _arretRepository.GetAllAsync(token);

            // L'utilisateur a quitté l'écran pendant le chargement : rien à afficher.
            if (token.IsCancellationRequested) return;

            if (!result.IsSuccess)
            {
                _snackbarService.ShowError(result.ErrorMessage);
                return;
            }

            // Tri alphabétique sur le libellé : l'API ne garantit aucun ordre.
            _allArrets = (result.Data ?? new Collection<Arret>())
                .OrderBy(arret => arret.Libelle, StringComparer.CurrentCulture)
                .ToList();

            ApplyFilter();
            _snackbarService.ShowSuccess($"{_allArrets.Count:N0} arrêts chargés");
            _log.Information($"{_allArrets.Count} arrêts chargés");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>Méthode générée par le Toolkit, appelée à chaque modification de SearchText.</summary>
    partial void OnSearchTextChanged(string value) => ApplyFilter();

    /// <summary>Recalcule la liste affichée à partir de la liste complète et du texte recherché.</summary>
    private void ApplyFilter()
    {
        string search = SearchText.Trim();

        IEnumerable<Arret> filtered = search.Length == 0
            ? _allArrets
            : _allArrets.Where(arret => Matches(arret, search));

        Arrets = new ObservableCollection<Arret>(filtered);
    }

    /// <summary>
    /// Un arrêt correspond à la recherche si son libellé ou son code contient
    /// le texte (sans tenir compte de la casse ni des accents : « precheurs »
    /// trouve « Prêcheurs »), ou si l'une de ses lignes a exactement ce code
    /// (« C6 » liste les arrêts de la ligne C6).
    /// </summary>
    private static bool Matches(Arret arret, string search)
    {
        CompareInfo compare = CultureInfo.CurrentCulture.CompareInfo;
        const CompareOptions options = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

        return compare.IndexOf(arret.Libelle, search, options) >= 0
            || compare.IndexOf(arret.CodeLieu, search, options) >= 0
            || arret.Lignes.Any(ligne => string.Equals(ligne.NumLigne, search, StringComparison.OrdinalIgnoreCase));
    }
}
