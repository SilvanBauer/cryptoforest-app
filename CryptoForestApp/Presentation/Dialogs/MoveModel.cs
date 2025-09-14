using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Services.LevelsSourceProvider;
using Microsoft.Extensions.Localization;

namespace CryptoForestApp.Presentation.Dialogs;
internal partial record MoveModel
{
    private readonly INavigator _navigator;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly MoveDto _moveDto;

    public IState<int> SelectedLevelIndex { get; set; }
    private readonly IImmutableList<ValueText<Guid>> _levels;
    public IListFeed<ValueText<Guid>> Levels => ListFeed<ValueText<Guid>>.Async(async _ => _levels);

    public MoveModel(INavigator navigator, IStringLocalizer stringLocalizer, LevelsSourceProvider levelsSourceProvider, MoveDto moveDto)
    {
        _navigator = navigator;
        _stringLocalizer = stringLocalizer;
        _moveDto = moveDto;
        _levels = levelsSourceProvider.CreateLevelsSource(_moveDto.CryptoForest);

        SelectedLevelIndex = State.Value(this, () => _levels.IndexOf(_levels.Single(l => l.Value == moveDto.CurrentLevelGuid)));
    }

    public async Task MoveAsync(CancellationToken cancellationToken)
    {
        var newLevel = _levels.ElementAt(await SelectedLevelIndex.Value(cancellationToken));
        if (_moveDto.CryptoForest.GetBaseLevel().GetLevel(newLevel.Value).GetItems().Any(l => l.Key == _moveDto.ItemKey))
        {
            await _navigator.ShowMessageDialogAsync<string>(
                this,
                title: _stringLocalizer["NewLevelAlreadyHasItemDialog.Title"],
                content: _stringLocalizer["NewLevelAlreadyHasItemDialog.Content"],
                buttons: [
                    new DialogAction(_stringLocalizer["Ok"])
                ],
                cancellation: cancellationToken);
        }
        else
        {
            try
            {
                await _moveDto.CryptoForest.MoveItemAsync(_moveDto.ItemGuid, newLevel.Value, cancellationToken);
                await _moveDto.CallbackAsync(cancellationToken);
            }
            catch
            {
                await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["MoveFailureDialog.Title"],
                    content: _stringLocalizer["MoveFailureDialog.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["Ok"])
                    ],
                    cancellation: cancellationToken);
            }
        }
    }
}
