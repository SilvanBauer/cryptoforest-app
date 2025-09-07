namespace CryptoForestApp.Presentation.Pages;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
    }

    // Workaround: MainWindow cannot be closed inside view model so method needs to be here instead of a command in the view model
    private void Cancel(object sender, RoutedEventArgs e)
        => (Application.Current as App)!.MainWindow!.Close();
}
