using CryptoForestLibrary;
using CryptoForestLibrary.Cryptograph.Algorithm;

namespace CryptoForestApp.Models.Dtos;
internal record MoveDto(Guid ItemGuid, Guid CurrentLevelGuid, CryptoForest<CryptoForestAesAlgorithm> CryptoForest, Func<CancellationToken, Task> CallbackAsync)
{
}
