using SQLite;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MovieApp.Models;

public class Movie : INotifyPropertyChanged
{
    [PrimaryKey]
    public int Id { get; set; }

    public string Title { get; set; }
    public string PosterPath { get; set; }
    public double Popularity { get; set; }

    public int VoteCount { get; set; }

    public string Overview { get; set; }    
    public string Genre { get; set; }

    public string ReleaseDate { get; set; }


    [Ignore]
    public double? RecommendationScore { get; set; }




    [Ignore]
    public string PosterFullPath => string.IsNullOrEmpty(PosterPath)
        ? "https://via.placeholder.com/100x150.png?text=No+Image"
        : $"https://image.tmdb.org/t/p/w500{PosterPath}";

    public event PropertyChangedEventHandler PropertyChanged;

    void OnPropertyChanged([CallerMemberName] string name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


}
