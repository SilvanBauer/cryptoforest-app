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
        // Load the history from the application local settings
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
        // Save the history from the application local settings
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
        // Removes the existing path if exists and insterts it again at the top of the history to ensure a correct order of the history
        _history.Remove(path);
        _history.Insert(0, path);
        SaveHistory();
    }
}
