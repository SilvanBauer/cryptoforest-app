using CryptoForestApp.Models.Dtos;
using CryptoForestApp.Services.HistoryService;
using Windows.Storage.Pickers;

namespace CryptoForestApp.Presentation;
internal partial record OpenModel
{
    private readonly INavigator _navigator;
    private readonly IHistoryService _historyService;
    private readonly OpenDto _openDto;

    public IState<string> SelectedFile { get; set; }
    public IState<string> Password { get; set; }

    public OpenModel(INavigator navigator, IHistoryService historyService, OpenDto openDto)
    {
        _navigator = navigator;
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
        // TODO implement
        Console.WriteLine($"Url: {_openDto.Url}, Selected File: {await SelectedFile.Value(cancellationToken)}, Password: {await Password.Value(cancellationToken)}");
        _historyService.Add(_openDto.Url);
    }

    public Task BackAsync(CancellationToken cancellationToken)
        => _navigator.NavigateBackAsync(cancellationToken);
}
