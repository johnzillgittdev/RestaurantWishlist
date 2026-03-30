using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using RestaurantWishlist.Database;
using RestaurantWishlist.Repository;
using RestaurantWishlist.ViewModels;
using RestaurantWishlist.Views;

namespace RestaurantWishlist;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf",   "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf",  "OpenSansSemibold");
            });

        // Data layer
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<IRestaurantRepository, SqliteRestaurantRepository>();

        // ViewModels
        builder.Services.AddSingleton<MainViewModel>();

        // Views
        builder.Services.AddSingleton<MainPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
