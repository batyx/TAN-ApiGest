using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using TanApiGest.Properties;
using TanApiGest.Services.Implementations;
using TanApiGest.Services.Interfaces;
using TanApiGest.ViewModels;
using TanApiGest.Views;

namespace TanApiGest;

/// <summary>
/// Point d'entrée de l'application : configuration des journaux (Serilog),
/// des filets de sécurité globaux et du conteneur d'injection de dépendances.
/// </summary>
public partial class App : Application
{
    /// <summary>Conteneur DI, construit une seule fois au démarrage.</summary>
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        ConfigureLogging();
        RegisterGlobalExceptionHandlers();

        Services = ConfigureServices();
        RegisterNavigableViews(Services.GetRequiredService<IMainWindowNavigationService>());

        // Pas d'authentification sur cette API : on ouvre directement la fenêtre principale.
        Services.GetRequiredService<MainWindow>().Show();

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Log.Information($"=== {AppSettings.APP_NAME} arrêt ===");
        Log.CloseAndFlush();
        base.OnExit(e);
    }

    /// <summary>Journal quotidien dans le dossier logs/ de l'exécutable + fenêtre Sortie de Visual Studio.</summary>
    private static void ConfigureLogging()
    {
        string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "tan-apigest_.log");

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug(outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();

        Log.Information($"=== {AppSettings.APP_NAME} démarrage ===");
    }

    /// <summary>
    /// Sans ces abonnements, une exception non prévue ferait planter
    /// l'application sans trace dans les journaux ni message à l'utilisateur.
    /// </summary>
    private void RegisterGlobalExceptionHandlers()
    {
        // 1) Exceptions du thread UI (binding, commande, code-behind...).
        DispatcherUnhandledException += OnDispatcherUnhandledException;

        // 2) Exceptions hors thread UI : .NET termine toujours le processus, on ne peut que journaliser.
        AppDomain.CurrentDomain.UnhandledException += OnAppDomainUnhandledException;

        // 3) Exception d'une Task jamais attendue (await) ni observée.
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>Déclare toutes les dépendances de l'application.</summary>
    private static ServiceProvider ConfigureServices()
    {
        ServiceCollection services = new();

        // Services — une seule instance partagée dans toute l'application
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<ISnackbarService, SnackbarService>();
        services.AddSingleton<IMainWindowNavigationService, MainWindowNavigationService>();

        // ViewModels — une nouvelle instance à chaque résolution
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<HomeViewModel>();

        // Fenêtre et vues
        services.AddTransient<MainWindow>();
        services.AddTransient<HomeView>();

        return services.BuildServiceProvider();
    }

    /// <summary>Associe chaque ViewModel navigable à sa vue (menu latéral de MainWindow).</summary>
    private static void RegisterNavigableViews(IMainWindowNavigationService navigation)
    {
        navigation.Register<HomeViewModel, HomeView>();
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Exception non gérée sur le thread UI");

        MessageBox.Show(
            "Une erreur inattendue est survenue. L'application va tenter de continuer." +
            Environment.NewLine + Environment.NewLine + e.Exception.Message,
            "Erreur inattendue",
            MessageBoxButton.OK,
            MessageBoxImage.Error);

        // Empêche WPF de fermer l'application : l'erreur est journalisée et signalée.
        e.Handled = true;
    }

    private void OnAppDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        Exception? ex = e.ExceptionObject as Exception;
        Log.Fatal(ex, "Exception fatale non gérée hors du thread UI (IsTerminating={IsTerminating})", e.IsTerminating);
        Log.CloseAndFlush();

        if (e.IsTerminating)
        {
            MessageBox.Show(
                "Une erreur critique est survenue et l'application doit se fermer." +
                Environment.NewLine + Environment.NewLine + (ex?.Message ?? "Erreur inconnue."),
                "Erreur critique",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Error(e.Exception, "Exception non observée dans une Task");
        e.SetObserved();
    }
}
