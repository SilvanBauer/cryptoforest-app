using CryptoForestLibrary;

namespace CryptoForestApp.Models.Dtos;
internal record AddLevelDto(Guid CurrentLevelGuid, AesCryptoForest CryptoForest);
