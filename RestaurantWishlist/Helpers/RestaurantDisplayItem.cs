using CommunityToolkit.Mvvm.ComponentModel;
using RestaurantWishlist.Models;

namespace RestaurantWishlist.Helpers;

/// <summary>
/// Wraps a Restaurant for display in the spin wheel CollectionView.
/// Each instance is unique so repeated items in the list are individually selectable.
/// </summary>
public partial class RestaurantDisplayItem : ObservableObject
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public Restaurant Restaurant { get; }

    public RestaurantDisplayItem(Restaurant restaurant)
    {
        Restaurant = restaurant;
    }
}
