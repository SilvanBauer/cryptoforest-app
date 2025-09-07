using CryptoForestLibrary;
using CryptoForestLibrary.Cryptograph.Algorithm;

namespace CryptoForestApp.Models.Dtos;
internal record CryptoForestDto(CryptoForest<CryptoForestAesAlgorithm> CryptoForest);
