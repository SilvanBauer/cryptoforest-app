using CryptoForestApp.Models;
using CryptoForestLibrary;
using CryptoForestLibrary.Config;
using Microsoft.Extensions.Localization;

namespace CryptoForestApp.Services.LevelsSourceProvider;
internal class LevelsSourceProvider
{
    private readonly IStringLocalizer _stringLocalizer;

    public LevelsSourceProvider(IStringLocalizer stringLocalizer)
    {
        _stringLocalizer = stringLocalizer;
    }

    internal IImmutableList<ValueText<Guid>> CreateLevelsSource(AesCryptoForest cryptoForest)
    {
        var baseLevel = cryptoForest.GetBaseLevel();
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
