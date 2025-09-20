namespace CryptoForestApp.Services.HistoryService;

/// <summary>
/// The IHistoryService is used to handle the history of CryptoForests that were opened and in which order they were last opened
/// </summary>
internal interface IHistoryService
{
    public IImmutableList<string> Get();

    public void Add(string path);
}
