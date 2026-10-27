using CommunityToolkit.Mvvm.ComponentModel;
using TanApiGest.Properties;

namespace TanApiGest.ViewModels;

/// <summary>Écran d'accueil : présentation de l'application et de l'API interrogée.</summary>
public partial class HomeViewModel : ObservableObject
{
    public string AppName => AppSettings.APP_NAME;

    /// <summary>Adresse de l'API, affichée pour savoir quelle copie est interrogée.</summary>
    public string ApiUrl => AppSettings.API_URL + AppSettings.API_SPACE;
}
