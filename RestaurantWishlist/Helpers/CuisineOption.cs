using System.Text.RegularExpressions;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using RestaurantWishlist.Enums;

namespace RestaurantWishlist.Helpers;

public partial class CuisineOption : ObservableObject
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public Cuisine Cuisine { get; }
    public string DisplayName { get; }
    public ICommand ToggleCommand { get; }

    public event Action<CuisineOption>? Toggled;

    public CuisineOption(Cuisine cuisine)
    {
        Cuisine = cuisine;
        DisplayName = Regex.Replace(cuisine.ToString(), "(?<=[a-z])([A-Z])", " $1");
        ToggleCommand = new Command(() =>
        {
            IsSelected = !IsSelected;
            Toggled?.Invoke(this);
        });
    }
}
