using CryptoForestApp.Models;
using CryptoForestApp.Services.HistoryService;

namespace CryptoForestApp.Presentation;
internal partial record MainModel
{
    private readonly IHistoryService _historyService;
    private readonly INavigator _navigator;

    public IState<string> SelectedHistoryItem { get; set; }
    public IListFeed<string> History => ListFeed<string>.Async(async (_) => _historyService.Get());

    public MainModel(IHistoryService historyService, INavigator navigator)
    {
        _historyService = historyService;
        _navigator = navigator;
        SelectedHistoryItem = State.Value(this, () => string.Empty);
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
        => await _navigator.NavigateViewModelAsync<OpenPageModel>(this, data: new OpenPageUrl(await SelectedHistoryItem.Value(cancellationToken)));
}
