using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Services.LevelsSourceProvider;
using CryptoForestLibrary.DirectoryStructure;
using Microsoft.Extensions.Localization;
using Windows.Storage.Pickers;

namespace CryptoForestApp.Presentation.Pages;
internal partial record AddItemModel
{
    private readonly INavigator _navigator;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly AddItemDto _addItemDto;

    public IState<bool> IsFilesSelected { get; set; }

    public IState<bool> IsDirectorySelected { get; set; }

    public IState<bool> IsTextSelected { get; set; }

    // Workaround: As using an array in a state doesn't seem to properly work with the updates I extracted it
    private string[] _selectedFiles;
    public IState<string> SelectedFiles { get; set; }

    public IState<string> SelectedDirectory { get; set; }

    public IState<string> TextData { get; set; }

    public IState<int> SelectedLevelIndex { get; set; }
    private readonly IImmutableList<ValueText<Guid>> _levels;
    public IListFeed<ValueText<Guid>> Levels => ListFeed<ValueText<Guid>>.Async(async _ => _levels);

    public IState<string> ItemName { get; set; }

    public AddItemModel(INavigator navigator, IStringLocalizer stringLocalizer, LevelsSourceProvider levelsSourceProvider, AddItemDto addItemDto)
    {
        _navigator = navigator;
        _stringLocalizer = stringLocalizer;
        _addItemDto = addItemDto;
        _levels = levelsSourceProvider.CreateLevelsSource(_addItemDto.CryptoForest);

        IsFilesSelected = State.Value(this, () => true);
        IsDirectorySelected = State.Value(this, () => false);
        IsTextSelected = State.Value(this, () => false);
        _selectedFiles = []; 
        SelectedFiles = State.Value(this, () => string.Empty);
        SelectedDirectory = State.Value(this, () => string.Empty);
        TextData = State.Value(this, () => string.Empty);
        SelectedLevelIndex = State.Value(this, () => _levels.IndexOf(_levels.Single(l => l.Value == _addItemDto.CurrentLevelGuid)));
        ItemName = State.Value(this, () => string.Empty);
    }

    public async Task SelectFilesAsync(CancellationToken cancellationToken)
    {
        var filePicker = new FileOpenPicker();
        filePicker.FileTypeFilter.Add("*");
        IReadOnlyList<StorageFile>? files = await filePicker.PickMultipleFilesAsync();
        if (files != null)
        {
            await SelectedFiles.UpdateAsync((_) => string.Join(", ", files.Select(f => f.Path)), cancellationToken);
            _selectedFiles = [.. files.Select(f => f.Path)];
        }
    }

    public async Task SelectDirectoryAsync(CancellationToken cancellationToken)
    {
        var folderPicker = new FolderPicker();
        StorageFolder? folder = await folderPicker.PickSingleFolderAsync();
        if (folder != null)
        {
            await SelectedDirectory.UpdateAsync((_) => folder.Path, cancellationToken);
        }
    }

    public async Task AddAsync(CancellationToken cancellationToken)
    {
        var isFilesSelected = await IsFilesSelected.Value(cancellationToken);
        var isDirectorySelected = await IsDirectorySelected.Value(cancellationToken);
        var isTextSelected = await IsTextSelected.Value(cancellationToken);
        var selectedDirectory = await SelectedDirectory.Value(cancellationToken);
        var textData = await TextData.Value(cancellationToken);
        var level = _levels.ElementAt(await SelectedLevelIndex.Value(cancellationToken));
        var itemName = await ItemName.Value(cancellationToken);
        if (isFilesSelected && _selectedFiles.Length == 0)
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["NoFilesSelectedDialog.Title"],
                content: _stringLocalizer["NoFilesSelectedDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
        else if (isDirectorySelected && string.IsNullOrWhiteSpace(selectedDirectory))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["NoDirectorySelectedDialog.Title"],
                content: _stringLocalizer["NoDirectorySelectedDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
        else if (isTextSelected && string.IsNullOrWhiteSpace(textData))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["NoTextDataDialog.Title"],
                content: _stringLocalizer["NoTextDataDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
        else if (string.IsNullOrWhiteSpace(itemName))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["NoDataNameDialog.Title"],
                content: _stringLocalizer["NoDataNameDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
        else if (_addItemDto.CryptoForest.GetBaseLevel().GetItems().ContainsKey(itemName))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["ItemAlreadyExistsDialog.Title"],
                content: _stringLocalizer["ItemAlreadyExistsDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
        else
        {
            Guid itemGuid;
            if (isFilesSelected)
            {
                var fileSearch = new FileSearch(_selectedFiles);
                itemGuid = await _addItemDto.CryptoForest.AddItemAsync(fileSearch, itemName, level.Value, cancellationToken);
            }
            else if (isDirectorySelected)
            {
                var fileSearch = new FileSearch(selectedDirectory);
                itemGuid = await _addItemDto.CryptoForest.AddItemAsync(fileSearch, itemName, level.Value, cancellationToken);
            }
            else
            {
                itemGuid = await _addItemDto.CryptoForest.AddItemAsync(textData, itemName, level.Value, cancellationToken);
            }

            if (itemGuid != Guid.Empty)
            {
                await BackAsync(cancellationToken);
            }
            else
            {
                await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["EncryptFailureDialog.Title"],
                    content: _stringLocalizer["EncryptFailureDialog.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["Ok"])
                    ],
                    cancellation: cancellationToken);
            }
        }
    }

    public async Task BackAsync(CancellationToken cancellationToken)
        => await _navigator.NavigateBackAsync(this, cancellation: cancellationToken);
}
