namespace CryptoForestApp.Models;

/// <summary>
/// A small record to be used in ComboBoxes if text and value aren't the same.
/// </summary>
/// <typeparam name="T">The type of the data</typeparam>
/// <param name="Text">The text value of the data</param>
/// <param name="Value">The actual value of the data of type T</param>
internal record ValueText<T>(string Text, T Value);
