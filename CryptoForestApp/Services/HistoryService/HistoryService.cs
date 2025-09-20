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
        _history = [];
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        var historyLength = localSettings.Values["HistoryLength"] as int?;
        if (historyLength != null)
        {
            for (var i = 0; i < historyLength; i++)
            {
                _history.Add((localSettings.Values[$"History{i}"] as string)!);
            }
        }
    }

    private void SaveHistory()
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        localSettings.Values["HistoryLength"] = _history.Count;
        for (var i = 0; i < _history.Count; i++)
        {
            localSettings.Values[$"History{i}"] = _history[i];
        }
    }

    public IImmutableList<string> Get()
        => [.. _history];

    public void Add(string path)
    {
        _history.Remove(path);
        _history.Insert(0, path);
        SaveHistory();
    }
}
