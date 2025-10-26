using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Services.HistoryService;
using CryptoForestLibrary;
using CryptoForestLibrary.Cryptograph.Storage;
using Microsoft.Extensions.Localization;
using Windows.Storage.Pickers;

namespace CryptoForestApp.Presentation.Pages;
internal partial record HistoryModel
{
    private readonly IHistoryService _historyService;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly INavigator _navigator;

    public IState<string> SelectedHistoryItem { get; set; }
    public IListFeed<string> History => ListFeed<string>.Async(async _ => _historyService.Get());

    public HistoryModel(IHistoryService historyService, IStringLocalizer stringLocalizer, INavigator navigator)
    {
        _historyService = historyService;
        _stringLocalizer = stringLocalizer;
        _navigator = navigator;

        SelectedHistoryItem = State.Value(this, () => string.Empty);
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        var selectedPath = (await SelectedHistoryItem.Value(cancellationToken))!;
        if (selectedPath != string.Empty)
        {
            await _navigator.NavigateViewModelAsync<OpenViewModel>(this, data: new OpenDto(selectedPath), cancellation: cancellationToken);
        }
    }

    public async Task AddAsync(CancellationToken cancellationToken)
    {
        var folderPicker = new FolderPicker();
        StorageFolder? folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            await _navigator.NavigateViewModelAsync<OpenViewModel>(this, data: new OpenDto(folder.Path), cancellation: cancellationToken);
        }
    }

    public async Task CreateAsync(CancellationToken cancellationToken)
    {
        var folderPicker = new FolderPicker();
        StorageFolder? folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            try
            {
                // Check if folder is empty and if not ask user for confirmation
                var continueOnNonEmptyFolder = Directory.GetFiles(folder.Path).Length == 0 && Directory.GetDirectories(folder.Path).Length == 0;
                if (!continueOnNonEmptyFolder)
                {
                    var createResult = await _navigator.ShowMessageDialogAsync<string>(
                        this,
                        title: _stringLocalizer["CreateDialog.Title"],
                        content: _stringLocalizer["CreateDialog.Content"],
                        buttons: [
                            new DialogAction(_stringLocalizer["CreateDialog.CreateButton"]),
                            new DialogAction(_stringLocalizer["CreateDialog.CancelButton"])
                        ],
                        cancellation: cancellationToken);
                    continueOnNonEmptyFolder = createResult == _stringLocalizer["CreateDialog.CreateButton"];
                }

                // Open CryptoForest view if folder was empty or user confirmed
                if (continueOnNonEmptyFolder)
                {
                
                    var storage = new CryptoForestFileStorage(folder.Path);
                    var cryptoForest = AesCryptoForest.CreateCryptoForest(storage);
                    _historyService.Add(folder.Path);
                    (Application.Current as App)!.UnexportedLevels.Add(cryptoForest.GetBaseLevel().EntryGuid);
                    await _navigator.NavigateViewModelAsync<CryptoForestViewModel>(this, data: new CryptoForestDto(cryptoForest), cancellation: cancellationToken);
                }
            }
            catch
            {
                await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["CreationFailureDialog.Title"],
                content: _stringLocalizer["CreationFailureDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
            }
        }
    }
}
