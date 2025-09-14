using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Presentation.Dialogs;
using CryptoForestLibrary;
using CryptoForestLibrary.Config;
using Microsoft.Extensions.Localization;

namespace CryptoForestApp.Presentation.Pages;
internal partial record CryptoForestModel
{
    private readonly INavigator _navigator;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly AesCryptoForest _cryptoForest;

    private LevelConfig _currentLevel;

    public IState<KeyValuePair<string, ItemConfig>> SelectedEntry { get; set; }
    public IListState<KeyValuePair<string, ItemConfig>> Entries { get; set; }
    
    public CryptoForestModel(INavigator navigator, IStringLocalizer stringLocalizer, CryptoForestDto cryptoForestDto)
    {
        _navigator = navigator;
        _stringLocalizer = stringLocalizer;
        _cryptoForest = cryptoForestDto.CryptoForest;
        _currentLevel = _cryptoForest.GetBaseLevel();

        SelectedEntry = State.Value(this, () =>
            new Dictionary<string, ItemConfig>()
            {
                { string.Empty, new ItemConfig(Guid.Empty, new KeyIV([], []), ItemType.Level) }
            }.FirstOrDefault());
        Entries = ListState.Value<CryptoForestModel, KeyValuePair<string, ItemConfig>>(this, () => [.. _currentLevel.GetLevels(), .. _currentLevel.GetItems()]);
    }

    public async Task AddAsync(CancellationToken cancellationToken)
        => await _navigator.NavigateViewModelAsync<AddItemViewModel>(this, data: new AddItemDto(_currentLevel.EntryGuid, _cryptoForest), cancellation: cancellationToken);

    public async Task DeleteAsync(CancellationToken cancellationToken)
    {
        var entry = await SelectedEntry.Value(cancellationToken);
        if (entry.Key == string.Empty)
        {
            return;
        }

        try
        {
            if (entry.Value.ItemType != ItemType.Level)
            {
                var createResult = await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["DeleteConfirmationDialog.Title"],
                    content: _stringLocalizer["DeleteConfirmationDialog.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["Yes"]),
                        new DialogAction(_stringLocalizer["No"])
                    ],
                    cancellation: cancellationToken);
                if (createResult == _stringLocalizer["Yes"])
                {
                    await _cryptoForest.RemoveItemAsync(entry.Value.EntryGuid, cancellationToken);
                    await RefreshAsync(cancellationToken);
                }
            }
            else
            {
                var createResult = await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["DeleteLevelConfirmationDialog.Title"],
                    content: _stringLocalizer["DeleteLevelConfirmationDialog.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["Yes"]),
                        new DialogAction(_stringLocalizer["No"])
                    ],
                    cancellation: cancellationToken);
                if (createResult == _stringLocalizer["Yes"])
                {
                    await _cryptoForest.RemoveLevelAsync(entry.Value.EntryGuid, cancellationToken);
                    await RefreshAsync(cancellationToken);
                }
            }
        }
        catch
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["DeleteFailureDialog.Title"],
                content: _stringLocalizer["DeleteFailureDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
    }

    public async Task MoveAsync(CancellationToken cancellationToken)
    {
        var entry = await SelectedEntry.Value(cancellationToken);
        if (entry.Key == string.Empty || entry.Value.ItemType == ItemType.Level)
        {
            return;
        }

        await _navigator.NavigateViewModelAsync<MoveViewModel>(this, Qualifiers.Dialog, new MoveDto
            (
                entry.Value.EntryGuid,
                _currentLevel.EntryGuid,
                _cryptoForest,
                CallbackAsync: RefreshAsync
            ), cancellationToken);
    }

    public async Task AddLevelAsync(CancellationToken cancellationToken)
        => await _navigator.NavigateViewModelAsync<AddLevelViewModel>(this, data: new AddLevelDto(_currentLevel.EntryGuid, _cryptoForest), cancellation: cancellationToken);

    private async Task RefreshAsync(CancellationToken cancellationToken)
        => await Entries.UpdateAsync((_) => [.. _currentLevel.GetLevels(), .. _currentLevel.GetItems()], cancellationToken);
}
