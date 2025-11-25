using System.Net.Http;
using Newtonsoft.Json.Linq;
using MovieApp.Models;
using Microsoft.Extensions.Configuration;

namespace MovieApp.Services
{
    public class MovieService
    {
        private readonly MovieDatabase _db;
        private readonly IConfiguration _configuration;
        private const string BaseUrl = "https://api.themoviedb.org/3/";

        public MovieService(MovieDatabase database)
        {
            _db = database;

            _configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();
        }


        private HttpClient CreateMovieDbClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(BaseUrl) };
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_configuration["ApiKeys:MovieDb"]}");
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            return client;
        }


        public async Task InitializeMoviesAsync()
        {
            var moviesInDb = await _db.GetMoviesAsync();
            if (moviesInDb == null || moviesInDb.Count == 0)
            {
                await LoadInitialMoviesAsync();
            }
        }

        private async Task LoadInitialMoviesAsync()
        {
            using HttpClient client = CreateMovieDbClient();


            const int totalPages = 500;
            for (int page = 1; page <= totalPages; page++)
            {
                try
                {
                    string url = $"discover/movie?sort_by=vote_count.desc&page={page}";
                    HttpResponseMessage response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode) continue;

                    string json = await response.Content.ReadAsStringAsync();
                    JObject result = JObject.Parse(json);
                    var movies = result["results"];
                    if (movies == null) continue;

                    foreach (var movieJson in movies)
                    {
                        
                            int id = (int)movieJson["id"];
                            string title = movieJson["title"]?.ToString();
                            string overview = movieJson["overview"]?.ToString();
                            string releaseDate = movieJson["release_date"]?.ToString();

                            var genreIds = movieJson["genre_ids"]?.Select(g => g.ToObject<int>()).ToList();
                            string genre = null;

                            if (genreIds != null && genreIds.Any())
                            {
                                genre = string.Join(", ", genreIds.Select(id => GenreMapping.ContainsKey(id) ? GenreMapping[id] : "Other"));
                            }

                         
                            var movie = await _db.GetMovieAsync(id);
                            if (movie == null)
                            {
                                movie = new Movie
                                {
                                    Id = id,
                                    Title = title,
                                    Overview = overview,
                                    Genre = genre,
                                    ReleaseDate = releaseDate,
                                    PosterPath = movieJson["poster_path"]?.ToString(),
                                    Popularity = movieJson["popularity"]?.ToObject<double>() ?? 0,
                                    VoteCount = movieJson["vote_count"]?.ToObject<int>() ?? 0
                                };

                               
                                    await _db.SaveMovieAsync(movie);
                                
                                
                            }
                       
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd podczas pobierania filmów: {ex.Message}");
                    continue;
                }

                await Task.Delay(300);
            }
        }

        public async Task LoadReviewsAsync()
        {
            var allMovies = await _db.GetMoviesAsync();
            using HttpClient client = CreateMovieDbClient();


            foreach (var movie in allMovies)
            {
                try
                {
                    HttpResponseMessage response = null;
                    response = await client.GetAsync($"movie/{movie.Id}/reviews");
                    
                   if (!response.IsSuccessStatusCode) continue;

                    string json = await response.Content.ReadAsStringAsync();
                    JObject result = JObject.Parse(json);
                    var reviews = result["results"];
                    if (reviews == null || !reviews.Any()) continue;

                    foreach (var reviewJson in reviews.Take(2))
                    {
                        
                            string author = reviewJson["author"]?.ToString();
                            string content = reviewJson["content"]?.ToString();
                            if (string.IsNullOrWhiteSpace(content)) continue;

                            var existingReview = await _db.GetReviewAsync(movie.Id, author, content);
                            if (existingReview != null) continue;

                            var review = new Review
                            {
                                MovieId = movie.Id,
                                Author = author,
                                Content = content
                            };
                           
                                await _db.SaveReviewAsync(review);                       
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd pobierania recenzji dla filmu {movie.Id}: {ex.Message}");
                }

                await Task.Delay(200);
            }
        }


        public async Task AnalyzeReviewsAsync(string modelPath)
        {
            var emotionService = new EmotionService(modelPath);
            var allReviews = await _db.GetReviewsAsync();

            foreach (var review in allReviews)
            {

                try
                {

                    var emotions = emotionService.PredictEmotions(review.Content);

                review.Emotion1 = emotions[0].Emotion;
                review.Score1 = emotions[0].Score;
                review.Emotion2 = emotions[1].Emotion;
                review.Score2 = emotions[1].Score;
                review.Emotion3 = emotions[2].Emotion;
                review.Score3 = emotions[2].Score;
                review.Emotion4 = emotions[3].Emotion;
                review.Score4 = emotions[3].Score;
                review.Emotion5 = emotions[4].Emotion;
                review.Score5 = emotions[4].Score;
                review.Emotion6 = emotions[5].Emotion;
                review.Score6 = emotions[5].Score;

                   
                        await _db.SaveReviewAsync(review);
                    
                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd analizy emocji recenzji ID {review.Id}: {ex.Message}");
                }
            }
        }

        private static readonly Dictionary<int, string> GenreMapping = new()
        {
            { 28, "Action" },
            { 12, "Adventure" },
            { 16, "Animation" },
            { 35, "Comedy" },
            { 80, "Crime" },
            { 99, "Documentary" },
            { 18, "Drama" },
            { 10751, "Family" },
            { 14, "Fantasy" },
            { 36, "History" },
            { 27, "Horror" },
            { 10402, "Music" },
            { 9648, "Mystery" },
            { 10749, "Romance" },
            { 878, "Science Fiction" },
            { 10770, "TV Movie" },
            { 53, "Thriller" },
            { 10752, "War" },
            { 37, "Western" }
        };
    }
}
