using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestLibrary.Config;
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

    private string[] _selectedFiles;
    public IState<string> SelectedFiles { get; set; }

    public IState<string> SelectedDirectory { get; set; }

    public IState<string> TextData { get; set; }

    public IState<int> SelectedLevelIndex { get; set; }
    private IImmutableList<ValueText<Guid>> _levels;
    public IListFeed<ValueText<Guid>> Levels => ListFeed<ValueText<Guid>>.Async(async _ => _levels);

    public IState<string> ItemName { get; set; }

    public AddItemModel(INavigator navigator, IStringLocalizer stringLocalizer, AddItemDto addItemDto)
    {
        _navigator = navigator;
        _stringLocalizer = stringLocalizer;
        _addItemDto = addItemDto;
        _levels = CreateLevelsSource();

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
                    title: _stringLocalizer["NoFilesSelected.Title"],
                    content: _stringLocalizer["NoFilesSelected.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["NoFilesSelected.OkButton"])
                    ],
                    cancellation: cancellationToken);
        }
        else if (isDirectorySelected && string.IsNullOrWhiteSpace(selectedDirectory))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["NoDirectorySelected.Title"],
                    content: _stringLocalizer["NoDirectorySelected.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["NoDirectorySelected.OkButton"])
                    ],
                    cancellation: cancellationToken);
        }
        else if (isTextSelected && string.IsNullOrWhiteSpace(textData))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["NoTextData.Title"],
                    content: _stringLocalizer["NoTextData.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["NoTextData.OkButton"])
                    ],
                    cancellation: cancellationToken);
        }
        else if (string.IsNullOrWhiteSpace(itemName))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["NoDataName.Title"],
                    content: _stringLocalizer["NoDataName.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["NoDataName.OkButton"])
                    ],
                    cancellation: cancellationToken);
        }
        else if (_addItemDto.CryptoForest.GetBaseLevel().GetItems().Keys.Contains(itemName))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["ItemAlreadyExists.Title"],
                    content: _stringLocalizer["ItemAlreadyExists.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["ItemAlreadyExists.OkButton"])
                    ],
                    cancellation: cancellationToken);
        }
        else
        {
            try
            {
                if (isFilesSelected)
                {
                    var fileSearch = new FileSearch(_selectedFiles);
                    await _addItemDto.CryptoForest.AddItemAsync(fileSearch, itemName, level.Value, cancellationToken);
                }
                else if (isDirectorySelected)
                {
                    var fileSearch = new FileSearch(selectedDirectory);
                    await _addItemDto.CryptoForest.AddItemAsync(fileSearch, itemName, level.Value, cancellationToken);
                }
                else
                {
                    await _addItemDto.CryptoForest.AddItemAsync(textData, itemName, level.Value, cancellationToken);
                }
            }
            catch
            {
                await _navigator.ShowMessageDialogAsync<string>(
                        this,
                        title: _stringLocalizer["EncryptFailureDialog.Title"],
                        content: _stringLocalizer["EncryptFailureDialog.Content"],
                        buttons: [
                            new DialogAction(_stringLocalizer["EncryptFailureDialog.OkButton"])
                        ],
                        cancellation: cancellationToken);
                return;
            }

            await _navigator.NavigateBackAsync(this, cancellation: cancellationToken);
        }
    }

    public async Task BackAsync(CancellationToken cancellationToken)
        => await _navigator.NavigateBackAsync(this, cancellation: cancellationToken);

    private IImmutableList<ValueText<Guid>> CreateLevelsSource()
    {
        var baseLevel = _addItemDto.CryptoForest.GetBaseLevel();
        var levels = new List<ValueText<Guid>>([new ValueText<Guid>(_stringLocalizer["BaseLevel"], baseLevel.EntryGuid)]);
        AddLevels(baseLevel);

        return [.. levels.OrderBy(l => l.Text)];

        void AddLevels(LevelConfig levelConfig)
        {
            levelConfig.GetLevels().ForEach(l => {
                levels.Add(new ValueText<Guid>(l.Key, l.Value.EntryGuid));
                AddLevels(levelConfig.GetLevel(l.Value.EntryGuid));
            });
        }
    }
}
