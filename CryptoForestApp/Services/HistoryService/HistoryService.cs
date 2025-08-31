namespace CryptoForestApp.Services.HistoryService;
internal class HistoryService : IHistoryService
{
    private List<string> _history = [];

    public HistoryService()
    {
        LoadHistory();
    }

    private void LoadHistory()
    {
        _history = ["TestValue 1", "TestValue 2", "TestValue 3"];
        // TODO implement
    }

    private void SaveHistory()
    {
        // TODO implement
    }

    public IImmutableList<string> Get()
        => [.. _history];

    public void Add(string path)
    {
        _history.Insert(0, path);
        SaveHistory();
    }

    public void MoveUp(string path)
    {
        if (_history.Contains(path))
        {
            _history.Remove(path);
            Add(path);
        }
    }
}
