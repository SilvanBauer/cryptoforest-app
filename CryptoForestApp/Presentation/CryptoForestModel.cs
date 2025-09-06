using CryptoForestLibrary;

namespace CryptoForestApp.Presentation;
internal partial record CryptoForestModel
{
    private readonly INavigator _navigator;
    private readonly AesCryptoForest _cryptoForest;

    public CryptoForestModel(INavigator navigator, AesCryptoForest cryptoForest)
    {
        _navigator = navigator;
        _cryptoForest = cryptoForest;
    }
}
