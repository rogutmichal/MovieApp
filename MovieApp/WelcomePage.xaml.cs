using MovieApp.Models;
using MovieApp.Services;

namespace MovieApp;

public partial class WelcomePage : ContentPage
{
    private bool isDataLoaded = false;
    private MainPage preloadedMainPage;
    private readonly MovieService _movieService;

    public WelcomePage()
    {
        InitializeComponent();
        _movieService = new MovieService(App.Database);

       
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _ = CheckDataAndProceedAsync();
    }

    private async Task CheckDataAndProceedAsync()
    {
        try
        {
            var moviesInDb = await App.Database.GetMoviesAsync();
            if (moviesInDb != null && moviesInDb.Count > 0)
            {
                await ProceedToMainPage();
            }
            else
            {
                LoadingIndicator.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                LoadingLabel.IsVisible = true;

                await _movieService.InitializeMoviesAsync();
                isDataLoaded = true;

                await ProceedToMainPage();
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Nie udało się załadować danych: {ex.Message}", "OK");
        }
    }

    private async Task ProceedToMainPage()
    {
        preloadedMainPage ??= new MainPage();
        await Navigation.PushAsync(preloadedMainPage);
    }
}
