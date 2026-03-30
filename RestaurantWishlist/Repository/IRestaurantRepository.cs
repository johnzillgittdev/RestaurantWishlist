using RestaurantWishlist.Models;

namespace RestaurantWishlist.Repository;

public interface IRestaurantRepository
{
    Task<List<Restaurant>> GetAllAsync();
    Task SaveAsync(Restaurant restaurant);
    Task DeleteAsync(Guid id);
}
