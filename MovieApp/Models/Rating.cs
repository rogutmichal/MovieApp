using SQLite;

namespace MovieApp.Models;

public class Rating
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int MovieId { get; set; }
    public int UserId { get; set; }
    public int Score { get; set; } 
    public DateTime RatingDate { get; set; }

}
