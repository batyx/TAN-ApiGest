namespace TanApiGest.Http.Implementations;

/// <summary>
/// Source de vérité unique pour les routes de l'API V2 (relatives à
/// AppSettings.API_URL + AppSettings.API_SPACE). Routes testées avec Bruno :
/// voir le dossier bruno/ à la racine du dépôt.
/// <para>
/// En cas d'erreur, l'API répond 404 (arrêt ou ligne introuvable), 409 (valeur
/// déjà présente) ou 422 (corps invalide), avec un corps { "message": "..." }
/// que Api transmet tel quel à l'utilisateur.
/// </para>
/// </summary>
public static class ApiEndpoints
{
    public static class Arrets
    {
        /// <summary>GET — 200 : liste des arrêts et des lignes qui les desservent.</summary>
        public const string GetAll = "/arrets";

        /// <summary>
        /// POST — F3 : ajoute une ligne à l'arrêt {codeLieu}. Corps JSON : { "numLigne": "C6" }.
        /// 201 : la ligne ajoutée. 404 : arrêt introuvable. 409 : ligne déjà présente. 422 : numLigne vide.
        /// </summary>
        public const string AddLigne = "/{codeLieu}/arret";

        /// <summary>
        /// PUT — F4 : modifie l'arrêt {codeLieu}. Corps JSON : { "libelle": "..." }
        /// (l'API accepte aussi "codeLieu", non utilisé par l'application).
        /// 200 : l'arrêt modifié. 404 : arrêt introuvable. 422 : nom vide.
        /// Même URL que AddLigne : c'est le verbe (POST / PUT) qui distingue les deux routes.
        /// </summary>
        public const string Update = "/{codeLieu}/arret";
    }

    public static class Data
    {
        /// <summary>
        /// GET — remet les données de cette copie de l'API dans leur état initial
        /// (bouton « Réinitialiser »). 200 : la liste des arrêts réinitialisée.
        /// </summary>
        public const string Reset = "/reset";
    }
}
