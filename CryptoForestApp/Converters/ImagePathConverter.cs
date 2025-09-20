using CryptoForestLibrary.Config;
using Microsoft.UI.Xaml.Data;

namespace CryptoForestApp.Converters;

/// <summary>
/// A XAML value binding convertor used to convert an ItemConfig to their appropriate icon iamge path
/// </summary>
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

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
