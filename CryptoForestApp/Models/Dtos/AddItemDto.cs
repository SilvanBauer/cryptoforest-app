using CryptoForestLibrary;

namespace CryptoForestApp.Models.Dtos;
internal record AddItemDto(Guid CurrentLevelGuid, AesCryptoForest CryptoForest);
