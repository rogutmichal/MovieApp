namespace MovieApp.Models;

public class RatedMovie
{
    public Movie Movie { get; set; }
    public int UserRating { get; set; }

   public double? RecommendationScore { get; set; }


    public string Title => Movie.Title;
    public string PosterFullPath => Movie.PosterFullPath;
    public double Popularity => Movie.Popularity;
    public int MovieId => Movie.Id;


    public string Overview => Movie.Overview;
    public string Genre => Movie.Genre;

    public string ReleaseDate => Movie.ReleaseDate;


    public float[] MovieEmotionValues { get; set; }
    public float[] UserEmotionValues { get; set; }
    public static readonly string[] EmotionOrder = { "anger", "fear", "joy", "sadness", "love", "surprise" };


}
