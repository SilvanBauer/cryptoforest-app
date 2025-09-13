using CryptoForestLibrary.Config;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;

namespace CryptoForestApp.Converters;
internal class ImagePathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is ItemConfig itemConfig)
        {
            switch (itemConfig.ItemType)
            {
                case ItemType.Level:
                    return "Assets/level_icon.png";
                case ItemType.Files:
                    return "Assets/data_icon.png";
                case ItemType.Text:
                    return "Assets/text_icon.png";
            }
        }

        return string.Empty;
    }

    // TODO check if RAM issues due to loading the same image multiple times occurs with default image implementation as WPF had this issue
    private BitmapImage GetImage(string imagePath)
    {
        using var stream = File.OpenRead($"./Assets/{imagePath}");
        var bmp = new BitmapImage();
        bmp.SetSource(stream);

        return bmp;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
