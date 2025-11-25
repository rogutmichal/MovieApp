using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using MovieApp.Services;

namespace MovieApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            builder.Logging.AddDebug();
#endif

            // Rejestracja serwisów i stron
            builder.Services.AddSingleton<MovieDatabase>(sp =>
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Movies.db3");
                return new MovieDatabase(dbPath);
            });
            builder.Services.AddSingleton<MovieService>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddSingleton<WelcomePage>();
            builder.Services.AddSingleton<App>();

            return builder.Build(); 
        }
    }
}
