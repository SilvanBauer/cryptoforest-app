using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestLibrary;
using CryptoForestLibrary.Config;
using Microsoft.Extensions.Localization;
using Windows.Storage.Pickers;

namespace CryptoForestApp.Presentation.Pages;
internal partial record ExportConfigModel
{
    private readonly INavigator _navigator;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly AesCryptoForest _cryptoForest;

    private List<LevelGuids> _selectedLevels;
    public IListFeed<LevelGuids> Levels => ListFeed<LevelGuids>.Async(async _ =>
    {
        var baseLevel = _cryptoForest.GetBaseLevel();
        var baseLevelGuids = new LevelGuids(
            _stringLocalizer["BaseLevel"],
            baseLevel.EntryGuid,
            Parent: null,
            []
        );

        AddLevels(baseLevel, baseLevelGuids);

        return [baseLevelGuids];

        void AddLevels(LevelConfig levelConfig, LevelGuids parent)
        {
            levelConfig.GetLevels().ForEach(l => {
                var levelGuids = new LevelGuids(
                    l.Key,
                    l.Value.EntryGuid,
                    parent,
                    []
                );
                parent.Sublevels.Add(levelGuids);
                AddLevels(levelConfig.GetLevel(l.Value.EntryGuid), levelGuids);
            });
        }
    });
    public IState<string> Password { get; set; }

    public ExportConfigModel(INavigator navigator, IStringLocalizer stringLocalizer, ExportConfigDto exportConfigDto)
    {
        _navigator = navigator;
        _stringLocalizer = stringLocalizer;
        _cryptoForest = exportConfigDto.CryptoForest;
        _selectedLevels = [];

        Password = State.Value(this, () => string.Empty);
    }

    public void HandleLevelSelection(TreeViewSelectionChangedEventArgs args)
    {
        foreach (LevelGuids level in args.AddedItems)
        {
            _selectedLevels.Add(level);
        }

        foreach (LevelGuids level in args.RemovedItems)
        {
            _selectedLevels.Remove(level);
        }
    }

    public async Task ExportAsync(CancellationToken cancellationToken)
    {
        var levelSelection = GetFullLevelSelection();
        var password = (await Password.Value(cancellationToken))!;
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA256.HashData(passwordBytes);
        if (!levelSelection.Any())
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["NoLevelsSelectedDialog.Title"],
                content: _stringLocalizer["NoLevelsSelectedDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
        else
        {
            var fileSavePicker = new FileSavePicker();
            fileSavePicker.FileTypeChoices.Add("Cryptograph Forest Config", new List<string>() { ".cfc" });
            StorageFile? saveFile = await fileSavePicker.PickSaveFileAsync();
            if (saveFile != null)
            {
                try
                {
                    await _cryptoForest.ExportConfigAsync(levelSelection, hashBytes, saveFile.Path + (saveFile.Path.EndsWith(".cfc") ? string.Empty : ".cfc"), cancellationToken);
                    foreach (var level in levelSelection)
                    {
                        (Application.Current as App)!.UnexportedLevels.Remove(level);
                    }

                    await BackAsync(cancellationToken);
                }
                catch(Exception ex)
                {
                    await _navigator.ShowMessageDialogAsync<string>(
                        this,
                        title: _stringLocalizer["ExportFailureDialog.Title"],
                        content: _stringLocalizer["ExportFailureDialog.Content"],
                        buttons: [
                            new DialogAction(_stringLocalizer["Ok"])
                        ],
                        cancellation: cancellationToken);
                }
            }
        }
    }

    public Task BackAsync(CancellationToken cancellationToken)
        => _navigator.NavigateBackAsync(this, cancellation: cancellationToken);

    private IEnumerable<Guid> GetFullLevelSelection()
    {
        var levelGuids = new List<Guid>();
        foreach (var level in _selectedLevels)
        {
            AddLevels(level);
        }

        return levelGuids;

        void AddLevels(LevelGuids level)
        {
            if (!levelGuids.Contains(level.LevelGuid))
            {
                levelGuids.Add(level.LevelGuid);
            }

            if (level.Parent != null)
            {
                AddLevels(level.Parent);
            }
        }
    }
}
