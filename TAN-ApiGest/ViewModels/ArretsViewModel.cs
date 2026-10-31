using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Globalization;
using TanApiGest.DTOs.DialogResults;
using TanApiGest.DTOs.Responses;
using TanApiGest.Models;
using TanApiGest.Repositories.Interfaces;
using TanApiGest.Services.Interfaces;
using TanApiGest.ViewModels.Dialogs;

namespace TanApiGest.ViewModels;

/// <summary>
/// Écran « Arrêts ».
/// F1 : liste des arrêts gérés par la TAN, avec recherche.
/// F2 : codes des lignes passant par l'arrêt sélectionné.
/// F3 : ajout d'une ligne à un arrêt (fenêtre modale, POST vers l'API).
/// F4 : modification du nom d'un arrêt directement dans la grille (PUT vers l'API).
/// Réinitialisation des données de l'API dans leur état initial (GET /reset).
/// </summary>
public partial class ArretsViewModel : ObservableObject
{
    private readonly IArretRepository _arretRepository;
    private readonly ISnackbarService _snackbarService;
    private readonly IDialogService _dialogService;
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

    /// <summary>Vrai pendant un appel à l'API : affiche la barre de progression et désactive les boutons.</summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoadArretsCommand))]
    [NotifyCanExecuteChangedFor(nameof(AddLigneCommand))]
    [NotifyCanExecuteChangedFor(nameof(UpdateLibelleCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetCommand))]
    public partial bool IsBusy { get; set; } = false;

    /// <summary>Ex. : « 12 arrêt(s) affiché(s) sur 1 345 ».</summary>
    public string CountLabel => $"{Arrets.Count:N0} arrêt(s) affiché(s) sur {_allArrets.Count:N0}";

    public ArretsViewModel(IArretRepository arretRepository, ISnackbarService snackbarService, IDialogService dialogService, ILogService log)
    {
        _arretRepository = arretRepository;
        _snackbarService = snackbarService;
        _dialogService = dialogService;
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

            DisplayArrets(result.Data);
            _snackbarService.ShowSuccess($"{_allArrets.Count:N0} arrêts chargés");
            _log.Information($"{_allArrets.Count} arrêts chargés");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Remet les données de l'API dans leur état initial (toutes les modifications
    /// sont perdues). L'API V2 renvoie la liste réinitialisée : on l'affiche
    /// directement, sans second appel.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRunCommand))]
    private async Task ResetAsync()
    {
        _log.Information("ArretsViewModel - ResetAsync");
        IsBusy = true;
        CancellationToken token = RenewToken();
        try
        {
            ApiResponse<Collection<Arret>> result = await _arretRepository.ResetAsync(token);

            if (token.IsCancellationRequested) return;

            if (!result.IsSuccess)
            {
                _snackbarService.ShowError(result.ErrorMessage);
                return;
            }

            DisplayArrets(result.Data);
            _snackbarService.ShowSuccess($"Données réinitialisées : {_allArrets.Count:N0} arrêts");
            _log.Information("Données de l'API réinitialisées");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Remplace la liste complète par les arrêts reçus de l'API, triés par nom
    /// (l'API ne garantit aucun ordre), puis réapplique la recherche en cours.
    /// </summary>
    private void DisplayArrets(Collection<Arret>? arrets)
    {
        _allArrets = (arrets ?? new Collection<Arret>())
            .OrderBy(arret => arret.Libelle, StringComparer.CurrentCulture)
            .ToList();

        ApplyFilter();
    }

    /// <summary>
    /// F3 : ajoute une ligne à l'arrêt passé en paramètre (bouton « Ajouter ligne »
    /// de la grille). Même déroulement que l'ajout de catégorie d'AliPurchase :
    /// dialogue → appel API → mise à jour de l'affichage → message.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRunCommand))]
    private async Task AddLigneAsync(Arret? arret)
    {
        if (arret is null) return;

        // Cliquer sur un bouton dans une ligne ne sélectionne pas forcément cette
        // ligne : on la sélectionne pour que le panneau de droite montre l'arrêt modifié.
        SelectedArret = arret;

        AddLigneDialogResult? dialogResult =
            await _dialogService.ShowAsync<AddLigneDialogViewModel, AddLigneDialogResult>(dialog => dialog.Arret = arret);

        if (dialogResult is null) return;

        // Inutile d'appeler l'API si la ligne dessert déjà l'arrêt.
        if (arret.HasLigne(dialogResult.NumLigne))
        {
            _snackbarService.ShowInfo($"La ligne {dialogResult.NumLigne} dessert déjà l'arrêt « {arret.Libelle} »");
            return;
        }

        _log.Information($"ArretsViewModel - AddLigneAsync {arret.CodeLieu} + {dialogResult.NumLigne}");
        IsBusy = true;
        CancellationToken token = RenewToken();
        try
        {
            ApiResponse<Ligne> result = await _arretRepository.AddLigneAsync(arret.CodeLieu, dialogResult.NumLigne, token);

            if (token.IsCancellationRequested) return;

            if (!result.IsSuccess)
            {
                _snackbarService.ShowError(result.ErrorMessage);
                return;
            }

            // L'API renvoie la ligne créée (201) ; par sécurité, on se rabat sur la saisie.
            Ligne ligne = result.Data ?? new Ligne { NumLigne = dialogResult.NumLigne };

            // Lignes est une ObservableCollection : la grille de droite se met à jour seule.
            arret.Lignes.Add(ligne);
            _snackbarService.ShowSuccess($"Ligne {ligne.NumLigne} ajoutée à l'arrêt « {arret.Libelle} »");
            _log.Information($"Ligne ajoutée : {arret.CodeLieu}/{ligne.NumLigne}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// F4 : enregistre le nom modifié dans la grille. Appelée à la fin de
    /// l'édition d'une cellule (CellEditEnding), avec la ligne sélectionnée —
    /// même principe que UpdateCategoryNameCommand dans AliPurchase.
    /// Grâce à UpdateSourceTrigger=PropertyChanged, arret.Libelle contient
    /// déjà le nouveau nom ; arret.OriginalLibelle contient l'ancien.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRunCommand))]
    private async Task UpdateLibelleAsync(object? selectedItem)
    {
        if (selectedItem is not Arret arret) return;

        string libelle = arret.Libelle.Trim();

        // Nom inchangé (simple clic dans la cellule) : pas d'appel à l'API.
        if (libelle == arret.OriginalLibelle)
        {
            arret.RevertLibelle();
            return;
        }

        // Nom vide : refusé avant même d'appeler l'API.
        if (libelle.Length == 0)
        {
            arret.RevertLibelle();
            _snackbarService.ShowError("Le nom de l'arrêt ne peut pas être vide.");
            return;
        }

        _log.Information($"ArretsViewModel - UpdateLibelleAsync {arret.CodeLieu} : « {arret.OriginalLibelle} » → « {libelle} »");
        IsBusy = true;
        CancellationToken token = RenewToken();
        try
        {
            ApiResponse<Arret> result = await _arretRepository.UpdateLibelleAsync(arret.CodeLieu, libelle, token);

            if (token.IsCancellationRequested)
            {
                arret.RevertLibelle();
                return;
            }

            if (!result.IsSuccess)
            {
                // L'affichage doit refléter ce qui est réellement enregistré côté API.
                // ErrorMessage est celui de l'API (ex. 404 « Arrêt ZZZZ introuvable. »).
                arret.RevertLibelle();
                _snackbarService.ShowError(result.ErrorMessage);
                return;
            }

            // L'API renvoie l'arrêt tel qu'elle l'a enregistré : c'est ce nom qui fait foi.
            string savedLibelle = result.Data?.Libelle ?? libelle;
            arret.Libelle = savedLibelle;
            arret.AcceptLibelle();
            _snackbarService.ShowSuccess($"Arrêt {arret.CodeLieu} renommé « {savedLibelle} »");
            _log.Information($"Arrêt renommé : {arret.CodeLieu}/{savedLibelle}");
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
            || arret.HasLigne(search);
    }
}
