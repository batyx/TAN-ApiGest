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

        /// <summary>
        /// F3 : ajoute une ligne à l'arrêt {codeLieu} si elle n'y figure pas déjà.
        /// Corps JSON : { "numLigne": "C6" }. Réponse : true si ajoutée, false sinon
        /// (arrêt introuvable ou ligne déjà présente) — toujours avec le statut 200.
        /// </summary>
        public const string AddLigne = "/{codeLieu}/arret";

        /// <summary>
        /// F4 : modifie l'arrêt {codeLieu}. Corps JSON : { "libelle": "..." }
        /// (l'API accepte aussi "codeLieu", non utilisé par l'application).
        /// Réponse : true si modifié, false si arrêt introuvable ou nom vide — toujours en 200.
        /// Même URL que AddLigne : c'est le verbe (POST / PUT) qui distingue les deux routes.
        /// </summary>
        public const string Update = "/{codeLieu}/arret";
    }

    public static class Data
    {
        /// <summary>
        /// Remet les données de cette copie de l'API dans leur état initial
        /// (bouton « Réinitialiser »). Réponse : true.
        /// </summary>
        public const string Reset = "/reset";
    }
}
