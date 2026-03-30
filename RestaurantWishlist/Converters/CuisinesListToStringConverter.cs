using System.Globalization;
using System.Text.RegularExpressions;
using RestaurantWishlist.Enums;

namespace RestaurantWishlist.Converters;

public class CuisinesListToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not List<Cuisine> cuisines || cuisines.Count == 0)
            return string.Empty;

        return string.Join(", ", cuisines.Select(c =>
            Regex.Replace(c.ToString(), "(?<=[a-z])([A-Z])", " $1")));
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
