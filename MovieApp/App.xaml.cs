using System.Diagnostics;

namespace MovieApp
{
    public partial class App : Application
    {
        static MovieDatabase database;
        public static int CurrentUserId = 2; 



        public static MovieDatabase Database
        {
            get
            {
                if (database == null)
                {
                    string dbPath = Path.Combine(FileSystem.AppDataDirectory, "Movies.db3");
                    database = new MovieDatabase(dbPath);
                }
                return database;
            }
        }
        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            return new Window(new NavigationPage(new WelcomePage()));
        }



        public static async Task<string> CopyModelToAppDirectoryAsync()
        {
            var fileName = "emotion_model.zip";
            var destPath = Path.Combine(FileSystem.AppDataDirectory, fileName);

            if (!File.Exists(destPath))
            {
                using var sourceStream = await FileSystem.OpenAppPackageFileAsync($"MLModels/{fileName}");
                using var destStream = File.Create(destPath);
                await sourceStream.CopyToAsync(destStream);
            }

            return destPath;
        }

    }
}