using SQLite;

namespace MovieApp.Models;

public class User
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string Name { get; set; }

    public string Role { get; set; }  
}
