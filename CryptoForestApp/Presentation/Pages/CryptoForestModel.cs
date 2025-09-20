using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Presentation.Dialogs;
using CryptoForestLibrary;
using CryptoForestLibrary.Config;
using Microsoft.Extensions.Localization;
using Windows.Storage.Pickers;

namespace CryptoForestApp.Presentation.Pages;
internal partial record CryptoForestModel
{
    private readonly INavigator _navigator;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly AesCryptoForest _cryptoForest;

    private static LevelConfig _currentLevel;
    private static List<LevelHistory> _levelHistory;
    private static int _index;

    public IState<bool> IsBackPossible { get; set; }
    public IState<bool> IsForwardPossible { get; set; }

    public IState<string> SearchQuery { get; set; }

    public IState<KeyValuePair<string, ItemConfig>> SelectedEntry { get; set; }
    public IListState<KeyValuePair<string, ItemConfig>> Entries { get; set; }
    
    public CryptoForestModel(INavigator navigator, IStringLocalizer stringLocalizer, CryptoForestDto cryptoForestDto)
    {
        _navigator = navigator;
        _stringLocalizer = stringLocalizer;
        _cryptoForest = cryptoForestDto.CryptoForest;
        if (_currentLevel == null)
        {
            _currentLevel = _cryptoForest.GetBaseLevel();
            _levelHistory = [new LevelHistory(_currentLevel, string.Empty)];
        }

        IsBackPossible = State.Value(this, () => _index != 0);
        IsForwardPossible = State.Value(this, () => _index + 1 != _levelHistory.Count);
        SearchQuery = State.Value(this, () => _levelHistory[_index].SearchQuery);
        SelectedEntry = State.Value(this, () =>
            new Dictionary<string, ItemConfig>()
            {
                { string.Empty, new ItemConfig(Guid.Empty, new KeyIV([], []), ItemType.Level) }
            }.FirstOrDefault());
        Entries = ListState.Value<CryptoForestModel, KeyValuePair<string, ItemConfig>>(this, () => [.. _currentLevel.GetLevels(), .. _currentLevel.GetItems()]);
    }

    public async Task GoBackAsync(CancellationToken cancellationToken)
    {
        _index--;
        _currentLevel = _levelHistory[_index].LevelConfig;
        await IsBackPossible.UpdateAsync(_ => _index != 0, cancellationToken);
        await IsForwardPossible.UpdateAsync(_ => true, cancellationToken);
        await SearchQuery.UpdateAsync(_ => _levelHistory[_index].SearchQuery, cancellationToken);
        await RefreshAsync(cancellationToken);
    }

    public async Task GoForwardAsync(CancellationToken cancellationToken)
    {
        _index++;
        _currentLevel = _levelHistory[_index].LevelConfig;
        await IsBackPossible.UpdateAsync(_ => true, cancellationToken);
        await IsForwardPossible.UpdateAsync(_ => _index + 1 != _levelHistory.Count, cancellationToken);
        await SearchQuery.UpdateAsync(_ => _levelHistory[_index].SearchQuery, cancellationToken);
        await RefreshAsync(cancellationToken);
    }

    public async Task SearchAsync(CancellationToken cancellationToken)
    {
        await AddToHistoryAsync(_cryptoForest.GetBaseLevel(), searchQuery: await SearchQuery.Value(cancellationToken), cancellationToken);
        await RefreshAsync(cancellationToken);
    }

    public async Task ResetAsync(CancellationToken cancellationToken)
    {
        await AddToHistoryAsync(_cryptoForest.GetBaseLevel(), searchQuery: string.Empty, cancellationToken);
        await SearchQuery.UpdateAsync(_ => _levelHistory[_index].SearchQuery, cancellationToken);
        await RefreshAsync(cancellationToken);
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        var selectedEntry = await SelectedEntry.Value(cancellationToken);
        if (selectedEntry.Value.ItemType == ItemType.Level)
        {
            await AddToHistoryAsync((LevelConfig)selectedEntry.Value, searchQuery: string.Empty, cancellationToken);
            await IsBackPossible.UpdateAsync(_ => true, cancellationToken);
            await IsForwardPossible.UpdateAsync(_ => false, cancellationToken);
            await SearchQuery.UpdateAsync(_ => string.Empty, cancellationToken);
            await RefreshAsync(cancellationToken);
        }
    }

    public async Task AddAsync(CancellationToken cancellationToken)
        => await _navigator.NavigateViewModelAsync<AddItemViewModel>(this, data: new AddItemDto(_currentLevel.EntryGuid, _cryptoForest), cancellation: cancellationToken);

    public async Task DecryptAsync(CancellationToken cancellationToken)
    {
        var entry = await SelectedEntry.Value(cancellationToken);
        if (string.IsNullOrEmpty(entry.Key) || entry.Value.ItemType == ItemType.Level)
        {
            return;
        }

        if (entry.Value.ItemType == ItemType.Files)
        {
            var folderPicker = new FolderPicker();
            StorageFolder? folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                try
                {
                    await _cryptoForest.GetDataItemAsync(entry.Value.EntryGuid, folder.Path, cancellationToken: cancellationToken);
                    await _navigator.NavigateViewModelAsync<DecryptedViewModel>(this, Qualifiers.Dialog, new DecryptedDto
                    (
                        Text: string.Empty
                    ), cancellationToken);
                }
                catch
                {
                    await _navigator.ShowMessageDialogAsync<string>(
                        this,
                        title: _stringLocalizer["DecryptionFailureDialog.Title"],
                        content: _stringLocalizer["DecryptionFailureDialog.Content"],
                        buttons: [
                            new DialogAction(_stringLocalizer["Ok"])
                        ],
                        cancellation: cancellationToken);
                }
            }
        }
        else
        {
            var text = await _cryptoForest.GetTextItemAsync(entry.Value.EntryGuid, cancellationToken);
            if (text != string.Empty)
            {
                await _navigator.NavigateViewModelAsync<DecryptedViewModel>(this, Qualifiers.Dialog, new DecryptedDto
                (
                    text
                ), cancellationToken);
            }
            else
            {
                await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["DecryptionFailureDialog.Title"],
                    content: _stringLocalizer["DecryptionFailureDialog.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["Ok"])
                    ],
                    cancellation: cancellationToken);
            }
        }
    }

    public async Task DeleteAsync(CancellationToken cancellationToken)
    {
        var entry = await SelectedEntry.Value(cancellationToken);
        if (string.IsNullOrEmpty(entry.Key))
        {
            return;
        }

        bool? deletionSuccessfull = null;
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
                deletionSuccessfull = await _cryptoForest.RemoveItemAsync(entry.Value.EntryGuid, cancellationToken);
                await RefreshAsync(cancellationToken);
            }
        }
        else
        {
            var shouldDelete = true;
            var levelConfig = (LevelConfig)entry.Value;
            if (levelConfig.GetItems().Count > 0 || levelConfig.GetLevels().Count > 0)
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
                shouldDelete = createResult == _stringLocalizer["Yes"];
            }

            if (shouldDelete)
            {
                deletionSuccessfull = await _cryptoForest.RemoveLevelAsync(entry.Value.EntryGuid, cancellationToken);
                await RefreshAsync(cancellationToken);

                if (deletionSuccessfull == true)
                {
                    (Application.Current as App)!.UnexportedLevels.Remove(entry.Value.EntryGuid);
                }
            }
        }

        if (deletionSuccessfull == false)
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
        if (string.IsNullOrEmpty(entry.Key) || entry.Value.ItemType == ItemType.Level)
        {
            return;
        }

        await _navigator.NavigateViewModelAsync<MoveViewModel>(this, Qualifiers.Dialog, new MoveDto
            (
                entry.Value.EntryGuid,
                entry.Key,
                _currentLevel.EntryGuid,
                _cryptoForest,
                CallbackAsync: RefreshAsync
            ), cancellationToken);
    }

    public async Task AddLevelAsync(CancellationToken cancellationToken)
        => await _navigator.NavigateViewModelAsync<AddLevelViewModel>(this, data: new AddLevelDto(_currentLevel.EntryGuid, _cryptoForest), cancellation: cancellationToken);

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var levelHistory = _levelHistory[_index];
        if (string.IsNullOrEmpty(levelHistory.SearchQuery))
        {
            await Entries.UpdateAsync(_ => [.. _currentLevel.GetLevels(), .. _currentLevel.GetItems()], cancellationToken);
        }
        else
        {
            try
            {
                var entries = levelHistory.LevelConfig.SearchItems(levelHistory.SearchQuery);
                await Entries.UpdateAsync(_ => [.. entries], cancellationToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private async Task AddToHistoryAsync(LevelConfig levelConfig, string searchQuery, CancellationToken cancellationToken)
    {
        _currentLevel = levelConfig;
        _index++;
        if (_levelHistory.Count > _index)
        {
            for (var i = _levelHistory.Count - 1; i >= _index; i--)
            {
                _levelHistory.RemoveAt(i);
            }
        }

        _levelHistory.Add(new LevelHistory(levelConfig, searchQuery));
        await IsBackPossible.UpdateAsync(_ => true, cancellationToken);
        await IsForwardPossible.UpdateAsync(_ => false, cancellationToken);
    }
}
