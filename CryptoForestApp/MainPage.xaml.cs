namespace CryptoForestApp;

public sealed partial class MainPage : Page
{
    public MainPage()
    {
        this.InitializeComponent();
        DataContext = (Application.Current as App)!.Host!.Services.GetRequiredService<MainViewModel>();
    }
}
