using CryptoForestApp.Models;
using CryptoForestApp.Models.Dtos;
using CryptoForestLibrary.Config;
using Uno.Extensions.Specialized;

namespace CryptoForestApp.Presentation.Dialogs;
internal partial record MoveModel
{
    private readonly MoveDto _moveDto;

    public IState<int> SelectedLevelIndex { get; set; }
    private IImmutableList<ValueText<Guid>> _levels;
    public IListFeed<ValueText<Guid>> Levels => ListFeed<ValueText<Guid>>.Async(async _ => _levels);

    public MoveModel(MoveDto moveDto)
    {
        _moveDto = moveDto;
        _levels = CreateLevelsSource();

        SelectedLevelIndex = State.Value(this, () => _levels.IndexOf(_levels.Single(l => l.Value == moveDto.CurrentLevelGuid)));
    }

    public async Task MoveAsync(CancellationToken cancellationToken)
    {
        var newLevel = _levels.ElementAt(await SelectedLevelIndex.Value(cancellationToken));
        await _moveDto.CryptoForest.MoveItemAsync(_moveDto.ItemGuid, newLevel.Value, cancellationToken);
        await _moveDto.CallbackAsync(cancellationToken);
    }

    private IImmutableList<ValueText<Guid>> CreateLevelsSource()
    {
        var baseLevel = _moveDto.CryptoForest.GetBaseLevel();
        var levels = new List<ValueText<Guid>>([ new ValueText<Guid>("Basis Ebene", baseLevel.EntryGuid) ]);
        AddLevels(baseLevel);

        return [.. levels.OrderBy(l => l.Text)];

        void AddLevels(LevelConfig levelConfig)
        {
            levelConfig.GetLevels().ForEach(l => {
                levels.Add(new ValueText<Guid>(l.Value, l.Key));
                AddLevels(levelConfig.GetLevel(l.Key));
            });
        }
    }
}
