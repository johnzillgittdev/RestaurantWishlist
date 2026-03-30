using RestaurantWishlist.Models;
using SQLite;

namespace RestaurantWishlist.Database;

public class DatabaseService
{
    private SQLiteAsyncConnection? _connection;

    private async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "wishlist.db3");
        _connection = new SQLiteAsyncConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _connection.CreateTableAsync<Restaurant>();
        return _connection;
    }

    public async Task<List<Restaurant>> GetAllAsync()
    {
        var db = await GetConnectionAsync();
        return await db.Table<Restaurant>().ToListAsync();
    }

    public async Task SaveAsync(Restaurant restaurant)
    {
        var db = await GetConnectionAsync();
        await db.InsertOrReplaceAsync(restaurant);
    }

    public async Task DeleteAsync(Guid id)
    {
        var db = await GetConnectionAsync();
        await db.DeleteAsync<Restaurant>(id);
    }
}
