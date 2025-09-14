using CryptoForestLibrary;

namespace CryptoForestApp.Models.Dtos;
internal record MoveDto(Guid ItemGuid, string ItemKey, Guid CurrentLevelGuid, AesCryptoForest CryptoForest, Func<CancellationToken, Task> CallbackAsync);
