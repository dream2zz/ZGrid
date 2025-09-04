using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace Z;

public sealed class EditorKindEqualsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is EditorKind actual)
        {
            if (parameter is EditorKind expected)
                return actual == expected;
            if (parameter is string s && Enum.TryParse<EditorKind>(s, out var parsed))
                return actual == parsed;
        }
        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
