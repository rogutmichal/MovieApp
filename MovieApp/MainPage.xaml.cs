using MovieApp.Models;
using MovieApp.Services;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MovieApp;

public partial class MainPage : ContentPage
{
   

    public ObservableCollection<RatedMovie> FilteredMovies { get; } = new();
    public ObservableCollection<RatedMovie> RecommendedRatedMovies { get; set; } = new();

    private Dictionary<int, EmotionProfile> _movieEmotions;
    private float[] _userProfile;






    private readonly MovieService _movieService;
    private User CurrentUser;

    private int currentPage = 1;
    private string lastSearchQuery = null;
    private bool hasInitialized = false;
    private bool showingRecommendations = false;
    private string selectedGenre = null;





    public MainPage()
    {
        InitializeComponent();
        BindingContext = this;
        _movieService = new MovieService(App.Database);

       

    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!hasInitialized)
        {
            hasInitialized = true;
            await SearchMoviesAsync();
        }

        CurrentUser = await App.Database.GetUserByIdAsync(App.CurrentUserId);

        LoadReviewsButton.IsVisible = CurrentUser?.Role == "admin";
        AnalyzeReviewsButton.IsVisible = CurrentUser?.Role == "admin";

        var allMovies = await App.Database.GetMoviesAsync();
        SetupGenreFilters(allMovies.SelectMany(m => m.Genre?.Split(", ") ?? Array.Empty<string>()));

    }

    private async void OnSearchClicked(object sender, EventArgs e)
    {
        currentPage = 1;
        lastSearchQuery = SearchBar.Text?.Trim();
        await SearchMoviesAsync();
    }

    private async void OnPreviousPageClicked(object sender, EventArgs e)
    {
        if (currentPage > 1)
        {
            currentPage--;
            await SearchMoviesAsync();
        }
    }

    private async void OnNextPageClicked(object sender, EventArgs e)
    {
        currentPage++;
        await SearchMoviesAsync();
    }
    private async Task SearchMoviesAsync()
    {
        var allMovies = await App.Database.GetMoviesAsync();
        var ratings = await App.Database.GetRatingsForUserAsync(App.CurrentUserId);
        var ratingsDict = ratings.ToDictionary(r => r.MovieId, r => r.Score);
        var allReviews = await App.Database.GetReviewsAsync();

        // Profil emocjonalny filmów
        _movieEmotions = RecommendationService.ComputeMovieEmotionProfiles(allReviews);

        // Profil emocjonalny użytkownika
        _userProfile = RecommendationService.ComputeUserEmotionProfile(ratings, _movieEmotions);

        // Wagi emocji dla użytkownika
        float[] emotionWeights = RecommendationService.ComputeEmotionWeights(ratings, _movieEmotions);

        // Filtr po tytule
        IEnumerable<Movie> filtered = string.IsNullOrWhiteSpace(lastSearchQuery)
            ? allMovies
            : allMovies.Where(m => m.Title != null && m.Title.Contains(lastSearchQuery, StringComparison.OrdinalIgnoreCase));

        // Filtr po gatunku
        if (!string.IsNullOrWhiteSpace(selectedGenre))
        {
            filtered = filtered.Where(m => m.Genre != null && m.Genre.Split(", ").Contains(selectedGenre));
        }

        // Sortowanie po popularności
        filtered = filtered.OrderByDescending(m => m.Popularity);

        int pageSize = 10;
        var pageResults = filtered
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        FilteredMovies.Clear();

        foreach (var movie in pageResults)
        {
            int userRating = ratingsDict.TryGetValue(movie.Id, out var score) ? score : -1;

            float[] movieProfile = _movieEmotions.TryGetValue(movie.Id, out var profile)
                ? profile.Emotions.Select((e, i) => e * emotionWeights[i]).ToArray()
                : new float[RecommendationService.EmotionOrder.Length];

            RecommendationService.NormalizeVectorL2(movieProfile);

            float[] userProfileCopy = _userProfile ?? new float[RecommendationService.EmotionOrder.Length];

            var ratedMovie = new RatedMovie
            {
                Movie = movie,
                UserRating = userRating,
                MovieEmotionValues = movieProfile,
                UserEmotionValues = userProfileCopy,
                RecommendationScore = movie.RecommendationScore
            };

            FilteredMovies.Add(ratedMovie);
        }

        if (FilteredMovies.Count == 0 && currentPage > 1)
        {
            currentPage--;
            await SearchMoviesAsync();
            return;
        }

        if (!showingRecommendations)
            PageInfoLabel.Text = $"Page {currentPage}";
    }




    private async Task<string> CopyModelToAppDirectoryAsync()
    {
#if ANDROID
        string fileName = "emotion_model.zip";
        string destPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

        if (!File.Exists(destPath))
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            using var destStream = File.Create(destPath);
            await stream.CopyToAsync(destStream);
        }

        return destPath;
#else
        return Path.Combine(AppContext.BaseDirectory, "emotion_model.zip");
#endif
    }

    //wczytywanie recenzji
    private async Task LoadAllReviewsAsync()
    {
        await _movieService.LoadReviewsAsync();
        await DisplayAlert("Gotowe", "Wczytano recenzje filmów.", "OK");
    }

    private async void OnLoadReviewsClicked(object sender, EventArgs e)
    {
        await LoadAllReviewsAsync();

        var allReviews = await App.Database.GetReviewsAsync();
       
    }

    //Rekomendacja
    private async void OnRecommendClicked(object sender, EventArgs e)

    {
      
        var recommended = await RecommendationService.GetRecommendedMoviesAsync(10);

        
        if (recommended.Any())            

        {

            ButtonRec.IsVisible = false;
            ButtonRight.IsVisible = false;
            ButtonLeft.IsVisible = false;
            FilteredMovies.Clear();

            foreach (var item in recommended)
            {

                float[] movieProfile = item.EmotionProfile ?? new float[RecommendationService.EmotionOrder.Length];

                float[] userProfileCopy = _userProfile ?? new float[RecommendationService.EmotionOrder.Length];              

                var ratedMovie = new RatedMovie
                {
                    Movie = item.Movie,
                    MovieEmotionValues = movieProfile,
                    UserEmotionValues = userProfileCopy,
                    RecommendationScore = Math.Round(item.SimilarityScore * 100)
                };

                FilteredMovies.Add(ratedMovie);
            }


            showingRecommendations = true;
            MoviesCollectionView.ItemsSource = FilteredMovies;
            PageInfoLabel.Text = "Recommendations";
            BackToMoviesButton.IsVisible = true;
        }
        else
        {
            await DisplayAlert("Za mało danych", "Oceń najpierw co najmniej 10 filmów!", "OK");
        }
    }


    private async Task AnalyzeAllReviewsAsync()
    {
        var modelPath = await CopyModelToAppDirectoryAsync();
        await _movieService.AnalyzeReviewsAsync(modelPath);

        await DisplayAlert("Gotowe", "Zanalizowano emocje w recenzjach.", "OK");
    }

    private async void OnAnalyzeReviewsClicked(object sender, EventArgs e)
    {
        await AnalyzeAllReviewsAsync();

        var allReviews = await App.Database.GetReviewsAsync();
        
    }

    private async void OnBackToMoviesClicked(object sender, EventArgs e)
    {
        showingRecommendations = false;
        MoviesCollectionView.ItemsSource = FilteredMovies;
        BackToMoviesButton.IsVisible = false;
        await SearchMoviesAsync();
        ButtonRec.IsVisible = true;
        ButtonRight.IsVisible = true;
        ButtonLeft.IsVisible = true;

    }

    private async void OnStarTapped(object sender, EventArgs e)
    {
        if (sender is Image image && image.GestureRecognizers.FirstOrDefault() is TapGestureRecognizer tap)
        {
            if (tap.CommandParameter is string param && int.TryParse(param, out int selectedRating))
            {
                if (image.BindingContext is RatedMovie ratedMovie)
                {
                    var existingRating = await App.Database.GetRatingForUserAndMovieAsync(App.CurrentUserId, ratedMovie.MovieId);

                    if (ratedMovie.UserRating == selectedRating)
                    {
                        if (existingRating != null)
                        {
                            await App.Database.DeleteRatingAsync(existingRating);
                        }

                        ratedMovie.UserRating = -1; 
                        UpdateStars(image.Parent as HorizontalStackLayout, 0);
                        return;
                    }

                    if (selectedRating > 0)
                    {
                        if (existingRating != null)
                        {
                            existingRating.Score = selectedRating;
                            existingRating.RatingDate = DateTime.Now;
                            await App.Database.SaveRatingAsync(existingRating);
                        }
                        else
                        {
                            var newRating = new Rating
                            {
                                UserId = App.CurrentUserId,
                                MovieId = ratedMovie.MovieId,
                                Score = selectedRating,
                                RatingDate = DateTime.Now
                            };
                            await App.Database.SaveRatingAsync(newRating);
                        }

                        ratedMovie.UserRating = selectedRating;
                        UpdateStars(image.Parent as HorizontalStackLayout, selectedRating);
                    }
                }
            }
        }
    }


    private async void UpdateStars(HorizontalStackLayout starsLayout, int selectedRating)
    {
        if (starsLayout == null) return;

        int index = 1;
        foreach (var child in starsLayout.Children.OfType<Image>())
        {
            child.Source = index <= selectedRating ? "star_filled.png" : "star_outline.png";

         

            index++;
        }
    }


    private void OnFrameBindingContextChanged(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is RatedMovie ratedMovie)
        {
            var starsLayout = frame.Content.FindByName<HorizontalStackLayout>("StarsLayout");

            if (starsLayout != null)
            {
                UpdateStars(starsLayout, ratedMovie.UserRating);
            }
        }
    }

    private void SetupGenreFilters(IEnumerable<string> genres)
    {
        GenreFilterLayout.Children.Clear();

        foreach (var genre in genres.Distinct().OrderBy(g => g))
        {
            var button = new Button
            {
                Text = genre,
                BackgroundColor = Color.FromArgb("#374151"),
                TextColor = Colors.White,
                CornerRadius = 12,
                Padding = new Thickness(12, 6),
                FontSize = 14
            };

            button.Clicked += (s, e) =>
            {
                selectedGenre = selectedGenre == genre ? null : genre;

                foreach (var child in GenreFilterLayout.Children.OfType<Button>())
                {
                    child.BackgroundColor = child.Text == selectedGenre ? Color.FromArgb("#3B82F6") : Color.FromArgb("#374151");
                }

                currentPage = 1;
                _ = SearchMoviesAsync();
            };

            GenreFilterLayout.Children.Add(button);
        }
    }

}
