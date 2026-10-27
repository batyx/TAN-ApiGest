namespace TanApiGest.Http.Implementations;

/// <summary>
/// Source de vérité unique pour les routes de l'API (relatives à
/// AppSettings.API_URL + AppSettings.API_SPACE). Routes testées avec Bruno :
/// voir le dossier bruno/ à la racine du dépôt.
/// </summary>
public static class ApiEndpoints
{
    public static class Arrets
    {
        /// <summary>Liste des arrêts et des lignes qui les desservent.</summary>
        public const string GetAll = "/arrets";
    }

    public static class Data
    {
        /// <summary>Remise des données dans leur état initial (non utilisée par l'application à ce stade).</summary>
        public const string Reset = "/reset";
    }
}
