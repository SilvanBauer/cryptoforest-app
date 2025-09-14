using Microsoft.UI.Xaml.Input;

namespace CryptoForestApp.Presentation.Pages;
public sealed partial class CryptoForestPage : Page
{
    public CryptoForestPage()
    {
        this.InitializeComponent();
    }

    // Workaround: Executing view model command on double click/tap
    private void OpenAsync(object sender, DoubleTappedRoutedEventArgs e)
    {
        var viewModel = DataContext as CryptoForestViewModel;
        viewModel!.OpenAsync.Execute(null);
    }
}
