using System.Globalization;
using RestaurantWishlist.Enums;

namespace RestaurantWishlist.Converters;

public class CostToRangeStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is Cost cost ? cost switch
        {
            Cost.Cheap      => "Cheap · Under $10",
            Cost.Affordable => "Affordable · $10–$25",
            Cost.Expensive  => "Expensive · $25–$50",
            Cost.Luxury     => "Luxury · $50+",
            _               => cost.ToString()
        } : null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
