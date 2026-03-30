using RestaurantWishlist.Database;
using RestaurantWishlist.Models;

namespace RestaurantWishlist.Repository;

public class SqliteRestaurantRepository(DatabaseService db) : IRestaurantRepository
{
    public Task<List<Restaurant>> GetAllAsync() => db.GetAllAsync();
    public Task SaveAsync(Restaurant restaurant) => db.SaveAsync(restaurant);
    public Task DeleteAsync(Guid id) => db.DeleteAsync(id);
}
