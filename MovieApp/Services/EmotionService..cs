using Microsoft.ML;
using Microsoft.ML.Data;
using MovieApp.Models;
using System.Text.RegularExpressions;

namespace MovieApp;


public class EmotionService
{

    //etykiety emocji
    private static readonly string[] EmotionLabels = new[]
    {
        "sadness", "anger", "love", "surprise", "fear", "joy"
    };

    private readonly PredictionEngine<ReviewInput, EmotionPrediction> _predictionEngine;

    public EmotionService(string modelPath)
    {
        var mlContext = new MLContext();

        if (!File.Exists(modelPath))
            throw new FileNotFoundException($"Nie znaleziono pliku modelu: {modelPath}");

        // Wczytanie modelu z pliku
        var loadedModel = mlContext.Model.Load(modelPath, out var _);
        // Utworzenie silnika predykcji
        _predictionEngine = mlContext.Model.CreatePredictionEngine<ReviewInput, EmotionPrediction>(loadedModel);
    }

    public List<(string Emotion, float Score)> PredictEmotions(string reviewText)
    {


        if (_predictionEngine == null)
        {
            Console.WriteLine("Model nie został poprawnie zainicjalizowany.");
        }

        var cleanedText = CleanText(reviewText);
        var input = new ReviewInput { ReviewText = cleanedText };
        // Uzyskanie predykcji z modelu
        var prediction = _predictionEngine.Predict(input);

        // Połączenie etykiet z wynikami
        return EmotionLabels
            .Select((label, index) => (label, prediction.Score[index]))
            .OrderByDescending(p => p.Item2)
            .ToList();
    }

    private static string CleanText(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return "";

        input = Regex.Replace(input, @"http[^\s]+", " ");
        input = Regex.Replace(input, @"www\.[^\s]+", " ");
        input = Regex.Replace(input, @"[^a-zA-Z0-9\s.,!?']", " ");
        input = Regex.Replace(input, @"\s+", " ").Trim();

        return input;
    }
}
