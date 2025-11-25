using MovieApp.Models;
using SQLite;


namespace MovieApp;

public class MovieDatabase
{
    private readonly SQLiteAsyncConnection _database;

    public MovieDatabase(string dbPath)
    {
        _database = new SQLiteAsyncConnection(dbPath);

        _database.CreateTableAsync<Movie>().Wait();
        _database.CreateTableAsync<Review>().Wait();
        _database.CreateTableAsync<User>().Wait();
        _database.CreateTableAsync<Rating>().Wait();

    }

    // FILMY

    public Task<List<Movie>> GetMoviesAsync() =>
        _database.Table<Movie>().ToListAsync();

    public Task<Movie> GetMovieAsync(int id) =>
        _database.Table<Movie>().Where(m => m.Id == id).FirstOrDefaultAsync();

    public Task<int> SaveMovieAsync(Movie movie) =>
        _database.InsertOrReplaceAsync(movie);

    // RECENZJE

    public Task<List<Review>> GetReviewsForMovieAsync(int movieId) =>
        _database.Table<Review>().Where(r => r.MovieId == movieId).ToListAsync();

    public Task<int> SaveReviewAsync(Review review)
    {
        if (review.Id == 0)
            return _database.InsertAsync(review);
        else
            return _database.UpdateAsync(review);
    }


    public Task<List<Review>> GetReviewsAsync() =>
    _database.Table<Review>().ToListAsync();

    public Task<Review?> GetReviewAsync(int movieId, string author, string content)
    {
        return _database.Table<Review>()
            .Where(r => r.MovieId == movieId
                        && r.Author == author
                        && r.Content == content)
            .FirstOrDefaultAsync();
    }



    // UŻYTKOWNICY
    public Task<int> SaveUserAsync(User user) =>
        user.Id == 0 ? _database.InsertAsync(user) : _database.UpdateAsync(user);

    public Task<User> GetUserAsync(int userId) =>
        _database.Table<User>().Where(u => u.Id == userId).FirstOrDefaultAsync();

    // OCENY
    public Task<List<Rating>> GetRatingsForMovieAsync(int movieId) =>
        _database.Table<Rating>().Where(r => r.MovieId == movieId).ToListAsync();

    public Task<Rating> GetRatingForUserAndMovieAsync(int userId, int movieId) =>
        _database.Table<Rating>().Where(r => r.UserId == userId && r.MovieId == movieId).FirstOrDefaultAsync();

    public Task<int> SaveRatingAsync(Rating rating) =>
        rating.Id == 0 ? _database.InsertAsync(rating) : _database.UpdateAsync(rating);

    public Task<List<Rating>> GetRatingsForUserAsync(int userId) =>
        _database.Table<Rating>().Where(r => r.UserId == userId).ToListAsync();

    public Task<int> DeleteRatingAsync(Rating rating) =>
    _database.DeleteAsync(rating);

    public Task<User> GetUserByNameAsync(string name) =>
        _database.Table<User>().FirstOrDefaultAsync(u => u.Name == name);

    //użytkownicy
    
    public Task<List<User>> GetAllUsersAsync() =>
        _database.Table<User>().ToListAsync();

    public Task<User> GetUserByIdAsync(int id) =>
    _database.Table<User>().FirstOrDefaultAsync(u => u.Id == id);





}
