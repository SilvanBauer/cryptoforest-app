using CryptoForestApp.Services.HistoryService;

namespace CryptoForestApp;
internal partial record MainModel(IHistoryService HistoryService)
{
    public IListFeed<string> History => ListFeed<string>.Async(async (_) => HistoryService.Get());

    public void Cancel()
    {
        Console.WriteLine("Cancelling TEST");
        App.Current.Exit();
    }
}
