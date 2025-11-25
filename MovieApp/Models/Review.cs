using SQLite;


namespace MovieApp.Models;
public class Review
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int MovieId { get; set; }

    public string Author { get; set; }

    public string Content { get; set; }

    // Top1
    public string Emotion1 { get; set; }
    public float Score1 { get; set; }

    // Top2
    public string Emotion2 { get; set; }
    public float Score2 { get; set; }

    // Top3
    public string Emotion3 { get; set; }
    public float Score3 { get; set; }

    // Top4
    public string Emotion4 { get; set; }
    public float Score4 { get; set; }

    // Top5
    public string Emotion5 { get; set; }
    public float Score5 { get; set; }

    // Top6
    public string Emotion6 { get; set; }
    public float Score6 { get; set; }
}
