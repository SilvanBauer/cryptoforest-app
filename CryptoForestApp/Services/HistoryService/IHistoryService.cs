namespace CryptoForestApp.Services.HistoryService;
internal interface IHistoryService
{
    public IImmutableList<string> Get();

    public void Add(string path);

    public void MoveUp(string path);
}
