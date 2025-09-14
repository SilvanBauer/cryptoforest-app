using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Services.LevelsSourceProvider;
using Microsoft.Extensions.Localization;

namespace CryptoForestApp.Presentation.Pages;
internal partial record AddLevelModel
{
    private readonly INavigator _navigator;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly AddLevelDto _addLevelDto;

    public IState<int> SelectedLevelIndex { get; set; }
    private readonly IImmutableList<ValueText<Guid>> _levels;
    public IListFeed<ValueText<Guid>> Levels => ListFeed<ValueText<Guid>>.Async(async _ => _levels);

    public IState<string> LevelName { get; set; }

    public AddLevelModel(INavigator navigator, IStringLocalizer stringLocalizer, LevelsSourceProvider levelsSourceProvider, AddLevelDto addLevelDto)
    {
        _navigator = navigator;
        _stringLocalizer = stringLocalizer;
        _addLevelDto = addLevelDto;
        _levels = levelsSourceProvider.CreateLevelsSource(_addLevelDto.CryptoForest);

        SelectedLevelIndex = State.Value(this, () => _levels.IndexOf(_levels.Single(l => l.Value == _addLevelDto.CurrentLevelGuid)));
        LevelName = State.Value(this, () => string.Empty);
    }

    public async Task AddAsync(CancellationToken cancellationToken)
    {
        var levelName = await LevelName.Value(cancellationToken);
        var parentLevel = _levels.ElementAt(await SelectedLevelIndex.Value(cancellationToken));
        if (string.IsNullOrEmpty(levelName))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["NoLevelNameDialog.Title"],
                content: _stringLocalizer["NoLevelNameDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
        else if (_addLevelDto.CryptoForest.GetBaseLevel().GetLevel(parentLevel.Value).GetLevels().ContainsKey(levelName))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["LevelAlreadyExistsDialog.Title"],
                content: _stringLocalizer["LevelAlreadyExistsDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
        else
        {
            var levelGuid = await _addLevelDto.CryptoForest.AddLevelAsync(levelName, parentLevel.Value, cancellationToken);
            if (levelGuid != Guid.Empty)
            {
                await BackAsync(cancellationToken);
            }
            else
            {
                await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["AddLevelFalureDialog.Title"],
                    content: _stringLocalizer["AddLevelFalureDialog.Content"],
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
