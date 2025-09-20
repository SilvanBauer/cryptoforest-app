namespace CryptoForestApp.Models;

/// <summary>
/// Used in the ExportConfigPage to handle level selection in the TreeView.
/// This record holds the data of each selectable level and also contains navigation properties to their parent and their sublevels.
/// </summary>
/// <param name="LevelKey">The key/name of the current level</param>
/// <param name="LevelGuid">The GUID of the current level</param>
/// <param name="Parent">The parent LevelGuids entry used for navigation</param>
/// <param name="Sublevels">The sublevels LevelGuids entries used for navigation</param>
internal record LevelGuids(string LevelKey, Guid LevelGuid, LevelGuids? Parent, List<LevelGuids> Sublevels);
