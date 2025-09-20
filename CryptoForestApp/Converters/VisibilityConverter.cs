using Microsoft.UI.Xaml.Data;

namespace CryptoForestApp.Converters;

/// <summary>
/// A XAML value binding convertor used to convert a boolean to a Visibility enum value.
/// Used to display and hide certain UI elements based on entered data.
/// </summary>
internal class VisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var isVisible = (bool)value;
        return isVisible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
