using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using TanApiGest.DTOs.Responses;
using TanApiGest.Http.Interfaces;
using TanApiGest.Properties;
using TanApiGest.Services.Interfaces;

namespace TanApiGest.Http.Implementations;

/// <summary>
/// Client HTTP de l'application, enregistré en Singleton (HttpClient doit
/// être partagé). L'API TAN est en accès libre : aucun en-tête
/// d'authentification n'est ajouté. Toutes les erreurs techniques sont
/// converties en ApiResponse en échec, avec un message pour l'utilisateur.
/// </summary>
public class Api : IApi
{
    private readonly ILogService _log;

    private static readonly string _apiUrl = AppSettings.API_URL + AppSettings.API_SPACE;

    // Sans ce réglage, HttpClient attend ~100 s avant d'abandonner : l'écran
    // resterait en chargement plus d'une minute si le serveur ne répond pas.
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);
    private static readonly HttpClient _httpClient = new() { Timeout = _defaultTimeout };

    // Les noms de champs JSON sont précisés par [JsonPropertyName] sur les
    // modèles ; l'insensibilité à la casse n'est qu'une sécurité supplémentaire.
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public Api(ILogService log)
    {
        _log = log;
    }

    /// <summary>Remplace les paramètres {nom} de la route par leur valeur encodée.</summary>
    private static string ParseUrl(string url, Dictionary<string, string>? routeParams)
    {
        if (routeParams is null) return url;

        foreach (KeyValuePair<string, string> param in routeParams)
            url = url.Replace($"{{{param.Key}}}", Uri.EscapeDataString(param.Value));

        return url;
    }

    private async Task<ApiResponse<T>> SendAsync<T>(HttpMethod method, string route, Dictionary<string, string>? routeParams, object? body, CancellationToken cancellationToken)
    {
        string url = ParseUrl(_apiUrl + route, routeParams);
        _log.Debug($"→ {method} {url}");

        using HttpRequestMessage request = new(method, url);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.UserAgent.ParseAdd("TanApiGestClient/1.0");

        if (body is not null)
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        try
        {
            using HttpResponseMessage response = await _httpClient.SendAsync(request, cancellationToken);
            _log.Debug($"← {method} {route} : {(int)response.StatusCode}");

            if (!response.IsSuccessStatusCode)
                return ApiResponse<T>.Failure($"Erreur serveur ({(int)response.StatusCode}) : {response.ReasonPhrase}");

            string json = await response.Content.ReadAsStringAsync(cancellationToken);

            // Une écriture réussie peut renvoyer un corps vide : ce n'est pas une
            // erreur, il n'y a simplement rien à désérialiser.
            if (string.IsNullOrWhiteSpace(json))
                return ApiResponse<T>.Success(default);

            return ApiResponse<T>.Success(JsonSerializer.Deserialize<T>(json, _jsonOptions));
        }
        catch (HttpRequestException httpEx)
        {
            _log.Error($"HttpRequestException {method} {route}", httpEx);
            return ApiResponse<T>.Failure("Serveur inaccessible. Vérifiez votre connexion au réseau de Nantes Métropole.");
        }
        catch (JsonException jsonEx)
        {
            _log.Error($"JsonException {method} {route}", jsonEx);
            return ApiResponse<T>.Failure("Réponse du serveur invalide (format JSON inattendu).");
        }
        catch (TaskCanceledException)
        {
            // Deux cas distincts : annulation volontaire (l'utilisateur a quitté
            // l'écran) ou expiration du délai de HttpClient.
            if (cancellationToken.IsCancellationRequested)
            {
                _log.Debug($"Requête annulée par l'appelant {method} {route}");
                return ApiResponse<T>.Failure("Requête annulée.");
            }

            _log.Warning($"Timeout {method} {route}");
            return ApiResponse<T>.Failure("La requête a expiré (timeout). Réessayez dans un instant.");
        }
        catch (Exception ex)
        {
            // Filet de sécurité : toute exception non anticipée est convertie en
            // réponse exploitable plutôt que de remonter jusqu'à l'interface.
            _log.Fatal($"Exception non anticipée {method} {route}", ex);
            return ApiResponse<T>.Failure("Une erreur inattendue est survenue. Réessayez, et contactez le support si le problème persiste.");
        }
    }

    public Task<ApiResponse<T>> GetAsync<T>(string route, Dictionary<string, string>? routeParams = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(HttpMethod.Get, route, routeParams, null, cancellationToken);

    public Task<ApiResponse<T>> PostAsync<T>(string route, Dictionary<string, string>? routeParams = null, object? body = null, CancellationToken cancellationToken = default)
        => SendAsync<T>(HttpMethod.Post, route, routeParams, body, cancellationToken);
}
