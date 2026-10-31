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
| **F4** | Modification du nom d'un arrêt | double-clic sur le nom dans la grille des arrêts, saisie, puis Entrée ou clic ailleurs |
| Reset | Remise des données de l'API dans leur état initial | bouton **Réinitialiser**, à côté d'**Actualiser** ; la liste est ensuite rechargée |

Interface réalisée avec **MaterialDesignInXAML**, sur le modèle du projet AliPurchase (même
architecture, mêmes paquets, mêmes versions).

## Prérequis

- Windows + Visual Studio 2022 (17.14 ou plus) ou Visual Studio 2026, charge de travail
  « Développement .NET Desktop », SDK **.NET 10** (comme AliPurchase).
- Accès au réseau local hébergeant l'API dupliquée, version 2 : `http://192.168.1.14/api/v2/BATY/`.

Le cahier des charges (contexte, user stories, spécifications, étapes) est dans
[`docs/cahier-des-charges.md`](docs/cahier-des-charges.md).

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
public const string API_URL   = "http://192.168.1.14/api/v2";
public const string API_SPACE = "/BATY";   // identifiant de l'élève : lettres et chiffres uniquement
```

Les routes sont centralisées dans `Http/Implementations/ApiEndpoints.cs`.

## API utilisée (version 2)

L'application utilise la **V2** de l'API (`api/v2/...`) : codes HTTP classiques et erreurs
au format JSON `{ "message": "..." }`, affichées telles quelles à l'utilisateur.
La V1 (`api/BATY/...`, réponses `true` / `false` toujours en 200) reste disponible côté
serveur mais n'est plus utilisée. Les deux versions partagent les mêmes données.

| Route | Rôle | Succès | Erreurs | Utilisée par l'appli |
|---|---|---|---|---|
| `GET api/v2/BATY/arrets` | liste des arrêts et de leurs lignes | 200 + liste | — | oui (F1, F2) |
| `GET api/v2/BATY/reset` | remise des données dans l'état initial | 200 + liste réinitialisée | — | oui (bouton Réinitialiser) |
| `POST api/v2/BATY/{codeLieu}/arret` | ajoute une ligne : corps `{ "numLigne": "C6" }` | 201 + la ligne | 404 arrêt introuvable · 409 ligne déjà présente · 422 `numLigne` vide | oui (F3) |
| `PUT api/v2/BATY/{codeLieu}/arret` | modifie l'arrêt : corps `{ "libelle": "..." }` (et/ou `codeLieu`) | 200 + l'arrêt | 404 · 409 code déjà pris · 422 valeur vide | oui (F4, nom seulement) |
| `PUT api/v2/BATY/{codeLieu}/arret/{numLigne}` | remplace un code de ligne : corps `{ "numLigne": "..." }` | 200 + l'arrêt | 404 · 409 · 422 | non (modification des codes écartée) |

Un identifiant d'élève (`BATY`) qui contient autre chose que des lettres et des chiffres
reçoit une 404.

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
`Arret.OriginalLibelle` (F4) mémorise le nom enregistré côté API : `ArretsViewModel.DisplayArrets`
l'initialise par un `foreach` après chaque chargement (`AcceptLibelle()`), puis il est mis à jour après
chaque modification réussie. Il permet de ne pas appeler l'API quand le nom n'a pas changé, et de
rétablir l'ancien nom (`RevertLibelle()`) si l'API refuse la modification.

## Tests des routes avec Bruno

La collection se trouve dans `bruno/`.

1. Bruno → **Open Collection** → choisir le dossier `bruno/`.
2. Sélectionner l'environnement **BATY** (`baseUrl = http://192.168.1.14/api/v2/BATY`).
3. Lancer **01 - Liste des arrêts** : statut 200 et trois tests sur la structure du JSON
   (tableau non vide, `codeLieu` / `libelle` / `ligne` présents, `numLigne` sur chaque ligne).
4. **02 - Reset des données** : statut 200 et liste réinitialisée.
5. **03 - Ajout d'une ligne à un arrêt** : ajoute la ligne `TEST` à l'arrêt `COMM` (statut 201,
   la ligne en réponse). Une seconde exécution donne 409 : c'est attendu.
   Relancer 01 pour vérifier, puis 02 pour revenir aux données initiales.
6. **04 - Modifier le nom d'un arrêt** : renomme `COMM` en « Commerce (test) » (statut 200,
   l'arrêt modifié en réponse).
7. **05 - Erreur : arrêt introuvable** : statut 404 et erreur JSON
   `{ "message": "Arrêt ZZZZ introuvable." }`, même sans en-tête `Accept`.

Si les tests passent, le modèle C# correspond à la réponse réelle. Sinon, adapter
`Models/Arret.cs` et `Models/Ligne.cs` avant tout le reste.

## Architecture

```
TAN-ApiGest/
├── App.xaml(.cs)                  Démarrage : Serilog, filets d'exceptions, conteneur DI
├── Properties/AppSettings.cs      URL de l'API
├── Http/                          Client HTTP (IApi / Api) + routes (ApiEndpoints)
├── DTOs/                          ApiResponse (succès ou erreur), ApiErrorResponse, résultats de dialogue
├── Models/                        Arret, Ligne (mapping JSON)
├── Repositories/                  IArretRepository / ArretRepository
├── Services/                      Journalisation, Snackbar, navigation interne, dialogues
├── ViewModels/                    MainWindow, Home, Arrets, Dialogs/AddLigne (CommunityToolkit.Mvvm)
└── Views/                         MainWindow, HomeView, ArretsView, Dialogs/AddLigneDialog (+ composant de menu)
```

Flux d'un chargement : `ArretsView` (Loaded) → `ArretsViewModel.LoadArretsCommand` →
`IArretRepository.GetAllAsync` → `IApi.GetAsync` → `ApiResponse<Collection<Arret>>`.
Les erreurs renvoyées par l'API (404, 409, 422 : son champ `message`), ainsi que les erreurs
réseau, JSON ou de délai, sont converties en message affiché dans le Snackbar,
et détaillées dans `logs/tan-apigest_AAAAMMJJ.log` à côté de l'exécutable.

Les lignes d'un arrêt (F2) sont déjà présentes dans la réponse de `/arrets` : afficher le
détail ne déclenche aucun appel supplémentaire. La recherche filtre la liste en mémoire.

## Versionnage

Dépôt : https://github.com/batyx/TAN-ApiGest

| Tag | Contenu |
|---|---|
| `v0.1` | Première maquette : F1 (liste des arrêts) et F2 (codes des lignes, en pastilles) |
| `v0.2` | Colonne « Lignes » retirée de la liste des arrêts ; lignes de l'arrêt présentées dans une grille (DataGrid) |
| `v0.3` | F3 (ajout d'une ligne à un arrêt) et F4 : modification du nom d'un arrêt dans la grille |

| `v0.4` | Bouton Réinitialiser (fusion de F4 dans `main`) |
| `v0.5` | Passage à l'API V2 : codes HTTP classiques, messages d'erreur de l'API affichés |

Branche en cours : `api-v2` (passage à l'API V2).
La modification des codes d'arrêt et de ligne a été écartée.

Les paquets NuGet, `bin/` et `obj/` ne sont pas versionnés (voir `.gitignore`).
