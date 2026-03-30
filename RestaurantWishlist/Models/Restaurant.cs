using RestaurantWishlist.Enums;
using SQLite;

namespace RestaurantWishlist.Models;

[Table("Restaurants")]
public class Restaurant
{
    [PrimaryKey]
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // Stored as comma-separated enum names; Cuisines property is the public API
    public string CuisinesData { get; set; } = string.Empty;

    public Cost Cost { get; set; }

    public DateTime LastModified { get; set; }

    [Ignore]
    public List<Cuisine> Cuisines
    {
        get => string.IsNullOrEmpty(CuisinesData)
            ? []
            : CuisinesData.Split(',').Select(Enum.Parse<Cuisine>).ToList();
        set => CuisinesData = string.Join(",", value);
    }

    public Restaurant(string name, List<Cuisine> cuisines, Cost cost)
    {
        Id = Guid.NewGuid();
        Name = name;
        Cuisines = cuisines;
        Cost = cost;
        LastModified = DateTime.UtcNow;
    }

    public Restaurant() { }
}
