using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using RestaurantWishlist.Enums;

namespace RestaurantWishlist.Helpers;

public partial class CostOption : ObservableObject
{
    [ObservableProperty]
    public partial bool IsSelected { get; set; }

    public Cost Cost { get; }
    public string DisplayName { get; }
    public string Range { get; }
    public ICommand ToggleCommand { get; }

    public event Action<CostOption>? Toggled;

    public CostOption(Cost cost)
    {
        Cost = cost;
        DisplayName = cost.ToString();
        Range = cost switch
        {
            Cost.Cheap      => "Under $10",
            Cost.Affordable => "$10–$25",
            Cost.Expensive  => "$25–$50",
            Cost.Luxury     => "$50+",
            _               => string.Empty
        };
        ToggleCommand = new Command(() =>
        {
            IsSelected = !IsSelected;
            Toggled?.Invoke(this);
        });
    }
}
