using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Presentation.Dialogs;

namespace CryptoForestApp.Presentation;
internal partial record CryptoForestModel
{
    private readonly INavigator _navigator;
    private readonly CryptoForestDto _cryptoForestDto;

    public CryptoForestModel(INavigator navigator, CryptoForestDto cryptoForestDto)
    {
        _navigator = navigator;
        _cryptoForestDto = cryptoForestDto;
    }

    // TODO use guids of selected item
    public async Task MoveAsync(CancellationToken cancellationToken)
        => await _navigator.NavigateViewModelAsync<MoveViewModel>(this, Qualifiers.Dialog, new MoveDto
            (
                ItemGuid: Guid.Empty,
                CurrentLevelGuid: _cryptoForestDto.CryptoForest.GetBaseLevel().EntryGuid,
                _cryptoForestDto.CryptoForest,
                CallbackAsync: Refresh
            ), cancellationToken);

    private async Task Refresh(CancellationToken cancellationToken)
    {
        Console.WriteLine("Refresh");
        // TODO implement overview refresh
    }
}
