using Microsoft.Extensions.Localization;

namespace CryptoForestApp.Presentation.Pages;
public sealed partial class AddItemPage : Page
{
    public AddItemPage()
    {
        this.InitializeComponent();

        // Workaround: As we need to display a UI Dialog we need to dispatch the request in the DispatcherQueue
        (Application.Current as App)!.DisplayExportDialog = () =>
        {
            var viewModel = DataContext as CryptoForestViewModel;
            DispatcherQueue.TryEnqueue(async () =>
            {
                var stringLocalizer = (Application.Current as App)!.Host!.Services.GetRequiredService<IStringLocalizer>();
                var navigator = this.Navigator();
                if (navigator != null)
                {
                    var result = await navigator.ShowMessageDialogAsync<string>(
                        this,
                        title: stringLocalizer["NotExportedDialog.Title"],
                        content: stringLocalizer["NotExportedDialog.Content"],
                        buttons: [
                            new DialogAction(stringLocalizer["Yes"]),
                            new DialogAction(stringLocalizer["No"])
                        ]);
                    if (result == stringLocalizer["Yes"])
                    {
                        (Application.Current as App)!.UnexportedLevels.Clear();
                        (Application.Current as App)!.MainWindow!.Close();
                    }
                }
            });
        };
    }
}
