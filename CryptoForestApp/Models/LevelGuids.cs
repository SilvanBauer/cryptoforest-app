namespace CryptoForestApp.Models;
internal record LevelGuids(string LevelKey, Guid LevelGuid, LevelGuids? Parent, List<LevelGuids> Sublevels);
