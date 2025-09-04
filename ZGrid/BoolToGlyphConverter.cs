using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Z;

public sealed class BoolToGlyphConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && b ? "-" : "+";

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is string s && s == "-";
}
