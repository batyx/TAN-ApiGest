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
    /// Serveur hébergeant la copie de l'API open data TAN / Naolib.
    /// On n'interroge pas l'API officielle pour ne pas la surcharger.
    /// L'API est en accès libre : aucun jeton d'authentification à gérer.
    /// </summary>
    public const string API_URL = "http://192.168.1.14/api";

    /// <summary>
    /// Segment propre à chaque copie de l'API (ici celle de M. Bâty).
    /// Les routes complètes sont donc de la forme http://192.168.1.14/api/BATY/arrets.
    /// </summary>
    public const string API_SPACE = "/BATY";
}
