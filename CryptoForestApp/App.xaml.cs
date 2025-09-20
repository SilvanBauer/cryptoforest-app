using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Presentation;
using CryptoForestApp.Presentation.Dialogs;
using CryptoForestApp.Presentation.Pages;
using CryptoForestApp.Services.HistoryService;
using CryptoForestApp.Services.LevelsSourceProvider;

namespace CryptoForestApp;
public partial class App : Application
{
    /// <summary>
    /// Initializes the singleton application object. This is the first line of authored code
    /// executed, and as such is the logical equivalent of main() or WinMain().
    /// </summary>
    public App()
    {
        this.InitializeComponent();

        UnexportedLevels = [];
    }

    // Workaround: Changed to internal to be able to close main window from code
    internal Window? MainWindow { get; private set; }
    // Workaround: Changed to internal to be able to get services myself
    internal IHost? Host { get; private set; }

    internal List<Guid> UnexportedLevels { get; private set; }
    internal Action? DisplayExportDialog { get; set; }

    protected async override void OnLaunched(LaunchActivatedEventArgs args)
    {
        var builder = this.CreateBuilder(args)
            .Configure(host => host
#if DEBUG
                // Switch to Development environment when running in DEBUG
                .UseEnvironment(Environments.Development)
#endif
                .UseConfiguration(configure: configBuilder =>
                    configBuilder
                        .EmbeddedSource<App>()
                        .Section<AppConfig>()
                )
                .UseLocalization()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IHistoryService, HistoryService>();
                    services.AddSingleton<LevelsSourceProvider>();
                })
                .UseNavigation(ReactiveViewModelMappings.ViewModelMappings, RegisterRoutes)
            );
        MainWindow = builder.Window;

#if DEBUG
        MainWindow.UseStudio();
#endif
        MainWindow.SetWindowIcon();
        MainWindow.AppWindow.Closing += (_, e) =>
        {
            if (UnexportedLevels.Any())
            {
                DisplayExportDialog?.Invoke();
                e.Cancel = true;
            }
        };

        Host = await builder.NavigateAsync<Shell>();
    }

    private static void RegisterRoutes(IViewRegistry views, IRouteRegistry routes)
    {
        views.Register(
            new ViewMap(ViewModel: typeof(ShellModel)),

            // Pages
            new ViewMap<HistoryPage, HistoryViewModel>(),
            new DataViewMap<OpenPage, OpenViewModel, OpenDto>(),
            new DataViewMap<CryptoForestPage, CryptoForestViewModel, CryptoForestDto>(),
            new DataViewMap<AddItemPage, AddItemViewModel, AddItemDto>(),
            new DataViewMap<AddLevelPage, AddLevelViewModel, AddLevelDto>(),
            new DataViewMap<ExportConfigPage, ExportConfigViewModel, ExportConfigDto>(),

            // Dialogs
            new DataViewMap<MoveContentDialog, MoveViewModel, MoveDto>(),
            new DataViewMap<DecryptedContentDialog, DecryptedViewModel, DecryptedDto>()
        );

        routes.Register(
            new RouteMap("", View: views.FindByViewModel<ShellModel>(),
                Nested:
                [
                    // Pages
                    new ("Main", View: views.FindByViewModel<HistoryViewModel>(), IsDefault:true),
                    new ("Open", View: views.FindByViewModel<OpenViewModel>()),
                    new ("CryptoForest", View: views.FindByViewModel<CryptoForestViewModel>()),
                    new ("AddItem", View: views.FindByViewModel<AddItemViewModel>()),
                    new ("AddLevel", View: views.FindByViewModel<AddLevelViewModel>()),
                    new ("ExportConfig", View: views.FindByViewModel<ExportConfigViewModel>()),

                    // Dialogs
                    new ("Move", View: views.FindByViewModel<MoveViewModel>()),
                    new ("Decrypted", View: views.FindByViewModel<DecryptedViewModel>())
                ]
            )
        );
    }
}
