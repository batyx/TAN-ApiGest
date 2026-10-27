# TAN-ApiGest

Maquette d'une application desktop (C# / WPF / MVVM) destinée au service informatique de
Nantes Métropole. À terme, elle permettra de saisir les changements liés aux travaux sur le
réseau TAN (arrêts, lignes, horaires…). Cette première version **récupère et affiche** les
données de la plateforme de données publiques.

| Fonctionnalité | Description | Où |
|---|---|---|
| **F1** | Liste des arrêts (bus, tram, etc.) gérés par la TAN, avec recherche par nom, code d'arrêt ou code de ligne | menu ☰ → **Arrêts** (grille) |
| **F2** | Pour chaque arrêt, codes des lignes qui le desservent | grille des lignes, dans le panneau de détail de l'arrêt sélectionné |
| **F3** | Ajout d'une ligne à un arrêt | bouton **Ajouter Ligne** de chaque arrêt → fenêtre modale → la grille des lignes se met à jour |

Interface réalisée avec **MaterialDesignInXAML**, sur le modèle du projet AliPurchase (même
architecture, mêmes paquets, mêmes versions).

## Prérequis

- Windows + Visual Studio 2022 (17.14 ou plus) ou Visual Studio 2026, charge de travail
  « Développement .NET Desktop », SDK **.NET 10** (comme AliPurchase).
- Accès au réseau local hébergeant l'API dupliquée `http://192.168.1.14/api/BATY/`.

## Installation

```bash
git clone https://github.com/batyx/TAN-ApiGest.git
```

Ouvrir `TAN-ApiGest.slnx`, puis **F5**. Les paquets NuGet ne sont pas versionnés
(voir `.gitignore`) : Visual Studio les restaure automatiquement à la première compilation
(ou `dotnet restore`).

## Configuration

Adresse de l'API : `TAN-ApiGest/Properties/AppSettings.cs`.

```csharp
public const string API_URL   = "http://192.168.1.14/api";
public const string API_SPACE = "/BATY";
```

Les routes sont centralisées dans `Http/Implementations/ApiEndpoints.cs`.

## API utilisée

| Route | Rôle | Utilisée par l'appli |
|---|---|---|
| `GET api/BATY/arrets` | liste des arrêts et de leurs lignes | oui (F1, F2) |
| `GET api/BATY/reset` | remise des données dans l'état initial | non (tests uniquement) |
| `POST api/BATY/{codeLieu}/arret` | ajoute `numLigne` à l'arrêt s'il n'y figure pas (corps JSON `{ "numLigne": "C6" }`) ; répond `true` ou `false`, toujours en 200 | oui (F3) |

Pas d'authentification. Format d'un arrêt (copie de l'API open data TAN `arrets.json`) :

```json
{ "codeLieu": "COMM", "libelle": "Commerce", "distance": null,
  "ligne": [ { "numLigne": "1" }, { "numLigne": "2" }, { "numLigne": "3" } ] }
```

### Adaptation au nommage de l'API

Les champs JSON ne suivent pas les conventions C# (camelCase, noms français, `ligne` au
singulier pour un tableau). Les modèles gardent des noms C# conventionnels et précisent le
nom JSON avec `[JsonPropertyName]` (System.Text.Json) :

| JSON | C# | Type |
|---|---|---|
| `codeLieu` | `Arret.CodeLieu` | `string` |
| `libelle` | `Arret.Libelle` | `string` |
| `ligne` | `Arret.Lignes` | `ObservableCollection<Ligne>` (la grille se met à jour après un ajout) |
| `numLigne` | `Ligne.NumLigne` | `string` (codes alphanumériques : `C6`…) |
| `distance` | *non repris* | toujours `null` pour la liste complète |

`Arret.HasLigne(numLigne)` indique si une ligne dessert déjà l'arrêt (recherche et contrôle avant ajout).

## Tests des routes avec Bruno

La collection se trouve dans `bruno/`.

1. Bruno → **Open Collection** → choisir le dossier `bruno/`.
2. Sélectionner l'environnement **BATY** (`baseUrl = http://192.168.1.14/api/BATY`).
3. Lancer **01 - Liste des arrêts** : statut 200 et trois tests sur la structure du JSON
   (tableau non vide, `codeLieu` / `libelle` / `ligne` présents, `numLigne` sur chaque ligne).
4. **02 - Reset des données** : statut 200.
5. **03 - Ajout d'une ligne à un arrêt** : ajoute la ligne `TEST` à l'arrêt `COMM` (statut 2xx).
   Relancer 01 pour vérifier, puis 02 pour revenir aux données initiales.

Si les tests passent, le modèle C# correspond à la réponse réelle. Sinon, adapter
`Models/Arret.cs` et `Models/Ligne.cs` avant tout le reste.

## Architecture

```
TAN-ApiGest/
├── App.xaml(.cs)                  Démarrage : Serilog, filets d'exceptions, conteneur DI
├── Properties/AppSettings.cs      URL de l'API
├── Http/                          Client HTTP (IApi / Api) + routes (ApiEndpoints)
├── DTOs/                          ApiResponse (succès ou erreur), résultats de dialogue
├── Models/                        Arret, Ligne (mapping JSON)
├── Repositories/                  IArretRepository / ArretRepository
├── Services/                      Journalisation, Snackbar, navigation interne, dialogues
├── ViewModels/                    MainWindow, Home, Arrets, Dialogs/AddLigne (CommunityToolkit.Mvvm)
└── Views/                         MainWindow, HomeView, ArretsView, Dialogs/AddLigneDialog (+ composant de menu)
```

Flux d'un chargement : `ArretsView` (Loaded) → `ArretsViewModel.LoadArretsCommand` →
`IArretRepository.GetAllAsync` → `IApi.GetAsync` → `ApiResponse<Collection<Arret>>`.
Les erreurs réseau, JSON ou de délai sont converties en message affiché dans le Snackbar,
et détaillées dans `logs/tan-apigest_AAAAMMJJ.log` à côté de l'exécutable.

Les lignes d'un arrêt (F2) sont déjà présentes dans la réponse de `/arrets` : afficher le
détail ne déclenche aucun appel supplémentaire. La recherche filtre la liste en mémoire.

## Versionnage

Dépôt : https://github.com/batyx/TAN-ApiGest

| Tag | Contenu |
|---|---|
| `v0.1` | Première maquette : F1 (liste des arrêts) et F2 (codes des lignes, en pastilles) |
| `v0.2` | Colonne « Lignes » retirée de la liste des arrêts ; lignes de l'arrêt présentées dans une grille (DataGrid) |

Branche en cours : `F3-ajout-ligne-arret` (F3 : ajout d'une ligne à un arrêt).

Les paquets NuGet, `bin/` et `obj/` ne sont pas versionnés (voir `.gitignore`).
