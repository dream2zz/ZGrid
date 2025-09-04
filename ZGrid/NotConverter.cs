using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Z;

public sealed class NotConverter : IValueConverter
{
    public static readonly NotConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value is null;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b ? !b : value is null;
}
