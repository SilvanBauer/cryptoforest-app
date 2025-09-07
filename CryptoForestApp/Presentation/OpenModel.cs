using System.Text;
using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Services.HistoryService;
using CryptoForestLibrary;
using CryptoForestLibrary.Cryptograph.Storage;
using Microsoft.Extensions.Localization;
using Windows.Storage.Pickers;

namespace CryptoForestApp.Presentation;
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
        // Create 32 byte key from password
        var password = (await Password.Value(cancellationToken))!;
        while (password.Length < 16)
        {
            password = $"#{password}";
        }

        var key = Encoding.UTF8.GetBytes(password);

        // Create crypto forest and change view
        try
        {
            var storage = new CryptoForestFileStorage(_openDto.Path);
            var cryptoForest = new AesCryptoForest(storage, key, (await SelectedFile.Value(cancellationToken))!);
            _historyService.Add(_openDto.Path);
            await _navigator.NavigateViewAsync<CryptoForestViewModel>(this, data: new CryptoForestDto(cryptoForest), cancellation: cancellationToken);
        }
        catch
        {
            // Handle exception on decrypting config
            await _navigator.ShowMessageDialogAsync<string>(
                    this,
                    title: _stringLocalizer["OpenFailureDialog.Title"],
                    content: _stringLocalizer["OpenFailureDialog.Content"],
                    buttons: [
                        new DialogAction(_stringLocalizer["OpenFailureDialog.OkButton"])
                    ],
                    cancellation: cancellationToken);
        }
    }

    public Task BackAsync(CancellationToken cancellationToken)
        => _navigator.NavigateBackAsync(cancellationToken);
}
