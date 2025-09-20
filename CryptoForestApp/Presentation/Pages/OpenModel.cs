using System.Security.Cryptography;
using System.Text;
using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Services.HistoryService;
using CryptoForestLibrary;
using CryptoForestLibrary.Cryptograph.Storage;
using Microsoft.Extensions.Localization;
using Windows.Storage.Pickers;

namespace CryptoForestApp.Presentation.Pages;
internal partial record OpenModel
{
    private readonly INavigator _navigator;
    private readonly IStringLocalizer _stringLocalizer;
    private readonly IHistoryService _historyService;
    private readonly OpenDto _openDto;

    public IState<string> SelectedFile { get; set; }
    public IState<string> Password { get; set; }

    public OpenModel(INavigator navigator, IStringLocalizer stringLocalizer, IHistoryService historyService, OpenDto openDto)
    {
        _navigator = navigator;
        _stringLocalizer = stringLocalizer;
        _historyService = historyService;
        _openDto = openDto;

        SelectedFile = State.Value(this, () => string.Empty);
        Password = State.Value(this, () => string.Empty);
    }

    public async Task SelectConfigurationFileAsync(CancellationToken cancellationToken)
    {
        var picker = new FileOpenPicker();
        picker.FileTypeFilter.Add(".cfc");
        StorageFile? file = await picker.PickSingleFileAsync();
        if (file != null)
        {
            await SelectedFile.UpdateAsync(_ => file.Path, cancellationToken);
        }
    }

    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        // Create 32 byte SHA256 hash key from password
        var password = (await Password.Value(cancellationToken))!;
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hashBytes = SHA256.HashData(passwordBytes);

        // Create crypto forest and change view
        try
        {
            var storage = new CryptoForestFileStorage(_openDto.Path);
            var cryptoForest = new AesCryptoForest(storage, hashBytes, (await SelectedFile.Value(cancellationToken))!);
            _historyService.Add(_openDto.Path);
            await _navigator.NavigateViewModelAsync<CryptoForestViewModel>(this, data: new CryptoForestDto(cryptoForest), cancellation: cancellationToken);
        }
        catch
        {
            // Handle exception on decrypting config
            await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["OpenFailureDialog.Title"],
                    content: _stringLocalizer["OpenFailureDialog.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["Ok"])
                    ],
                    cancellation: cancellationToken);
        }
    }

    public Task BackAsync(CancellationToken cancellationToken)
        => _navigator.NavigateBackAsync(this, cancellation: cancellationToken);
}
