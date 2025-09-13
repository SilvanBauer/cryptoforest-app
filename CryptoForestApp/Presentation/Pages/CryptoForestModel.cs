using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Presentation.Dialogs;
using CryptoForestLibrary;
using CryptoForestLibrary.Config;

namespace CryptoForestApp.Presentation.Pages;
internal partial record CryptoForestModel
{
    private readonly INavigator _navigator;
    private readonly AesCryptoForest _cryptoForest;
    private readonly LevelConfig _currentLevel;

    public IState<KeyValuePair<string, ItemConfig>> SelectedEntry { get; set; }
    public IListState<KeyValuePair<string, ItemConfig>> Entries { get; set; }
    
    public CryptoForestModel(INavigator navigator, CryptoForestDto cryptoForestDto)
    {
        _navigator = navigator;
        _cryptoForest = cryptoForestDto.CryptoForest;
        _currentLevel = _cryptoForest.GetBaseLevel();

        SelectedEntry = State.Value(this, () =>
            new Dictionary<string, ItemConfig>()
            {
                { string.Empty, new ItemConfig(Guid.Empty, new KeyIV([], []), ItemType.Level) }
            }.FirstOrDefault());
        Entries = ListState.Value<CryptoForestModel, KeyValuePair<string, ItemConfig>>(this, () => [.. _currentLevel.GetLevels(), .. _currentLevel.GetItems()]);
    }

    public async Task MoveAsync(CancellationToken cancellationToken)
    {
        var entry = await SelectedEntry.Value(cancellationToken);
        if (entry.Key == string.Empty)
        {
            return;
        }

        await _navigator.NavigateViewModelAsync<MoveViewModel>(this, Qualifiers.Dialog, new MoveDto
            (
                ItemGuid: entry.Value.EntryGuid,
                CurrentLevelGuid: _currentLevel.EntryGuid,
                _cryptoForest,
                CallbackAsync: Refresh
            ), cancellationToken);
    }

    public async Task TestPrint(CancellationToken cancellationToken)
    {
        Console.WriteLine((await SelectedEntry.Value(cancellationToken)).Key);
    }

    private async Task Refresh(CancellationToken cancellationToken)
        => await Entries.UpdateAsync((_) => [.. _currentLevel.GetLevels(), .. _currentLevel.GetItems()], cancellationToken);
}
