namespace TanApiGest.Properties;

/// <summary>
/// Configuration de l'application, regroupée dans une classe statique
/// (même choix qu'AliPurchase : lisible et indépendant de Visual Studio).
/// </summary>
public static class AppSettings
{
    /// <summary>Nom affiché dans la barre de titre et l'en-tête.</summary>
    public const string APP_NAME = "TAN-ApiGest";

    /// <summary>
    /// Serveur hébergeant la copie de l'API open data TAN / Naolib, version 2.
    /// On n'interroge pas l'API officielle pour ne pas la surcharger.
    /// L'API est en accès libre : aucun jeton d'authentification à gérer.
    /// La V2 répond avec des codes HTTP classiques (200, 201, 404, 409, 422)
    /// et des erreurs JSON { "message": "..." } ; la V1 (sans /v2) répondait
    /// true / false avec un statut 200.
    /// </summary>
    public const string API_URL = "http://192.168.1.14/api/v2";

    /// <summary>
    /// Identifiant de l'élève (ici M. Bâty) : chaque identifiant travaille sur sa
    /// propre copie des données. Lettres et chiffres uniquement, sinon l'API répond 404.
    /// Les routes complètes sont donc de la forme http://192.168.1.14/api/v2/BATY/arrets.
    /// </summary>
    public const string API_SPACE = "/BATY";
}
