using CryptoForestApp.Models.Dtos;

namespace CryptoForestApp.Presentation.Dialogs;
internal partial record DecryptedModel
{
    public IFeed<bool> HasText { get; }

    public IFeed<string> Text { get; }

    public DecryptedModel(DecryptedDto decryptedDto)
    {
        HasText = Feed<bool>.Async(async _ => decryptedDto.Text != string.Empty);
        Text = Feed<string>.Async(async _ => decryptedDto.Text);
    }
}
