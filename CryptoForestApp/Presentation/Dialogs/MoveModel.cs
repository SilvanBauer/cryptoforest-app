using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Services.LevelsSourceProvider;
using Microsoft.Extensions.Localization;

namespace CryptoForestApp.Presentation.Dialogs;
internal partial record MoveModel
{
    private readonly IStringLocalizer _stringLocalizer;
    private readonly MoveDto _moveDto;

    public IState<int> SelectedLevelIndex { get; set; }
    private readonly IImmutableList<ValueText<Guid>> _levels;
    public IListFeed<ValueText<Guid>> Levels => ListFeed<ValueText<Guid>>.Async(async _ => _levels);

    public MoveModel(IStringLocalizer stringLocalizer, LevelsSourceProvider levelsSourceProvider, MoveDto moveDto)
    {
        _stringLocalizer = stringLocalizer;
        _moveDto = moveDto;
        _levels = levelsSourceProvider.CreateLevelsSource(_moveDto.CryptoForest);

        SelectedLevelIndex = State.Value(this, () => _levels.IndexOf(_levels.Single(l => l.Value == moveDto.CurrentLevelGuid)));
    }

    public async Task MoveAsync(CancellationToken cancellationToken)
    {
        var newLevel = _levels.ElementAt(await SelectedLevelIndex.Value(cancellationToken));
        await _moveDto.CryptoForest.MoveItemAsync(_moveDto.ItemGuid, newLevel.Value, cancellationToken);
        await _moveDto.CallbackAsync(cancellationToken);
    }
}
