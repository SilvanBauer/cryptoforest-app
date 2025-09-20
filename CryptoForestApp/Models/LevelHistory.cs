using CryptoForestLibrary.Config;

namespace CryptoForestApp.Models;

/// <summary>
/// The LevelHistory is used to hold the data for a history entry in the CryptoForestPage.
/// </summary>
/// <param name="LevelConfig">The level config of the level that was opened</param>
/// <param name="SearchQuery">The search query if any that was entered</param>
internal record LevelHistory(LevelConfig LevelConfig, string SearchQuery);
