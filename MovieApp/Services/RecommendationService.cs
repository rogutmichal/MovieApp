using MovieApp.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MovieApp;

public static class RecommendationService
{
    public static readonly string[] EmotionOrder = { "anger", "fear", "joy", "sadness", "love", "surprise" };

 
    public static async Task<List<RecommendedMovie>> GetRecommendedMoviesAsync(int topN = 5)
    {
        var allMovies = await App.Database.GetMoviesAsync();
        var allReviews = await App.Database.GetReviewsAsync();
        var allRatings = await App.Database.GetRatingsForUserAsync(App.CurrentUserId);

        if (allRatings.Count < 10)
            return new List<RecommendedMovie>();

        // Profile emocjonalne filmów
        var movieEmotions = ComputeMovieEmotionProfiles(allReviews);

        // Wagi dla emocji
        float[] emotionWeights = ComputeEmotionWeights(allRatings, movieEmotions);

        // Profil emocjonalny użytkownika
        var userProfile = ComputeUserEmotionProfile(allRatings, movieEmotions);


        // Tworzenie rekomendacji
        var ratedMovieIds = allRatings.Select(r => r.MovieId).ToHashSet();

        var recommendedMovies = allMovies
            .Where(m => !ratedMovieIds.Contains(m.Id) && movieEmotions.ContainsKey(m.Id))
            .Select(m =>
            {
                var rawEmotions = movieEmotions[m.Id].Emotions;
                var weightedEmotions = new float[EmotionOrder.Length];

                for (int i = 0; i < weightedEmotions.Length; i++)
                    weightedEmotions[i] = rawEmotions[i] * emotionWeights[i];

                NormalizeVectorL2(weightedEmotions);

                return new RecommendedMovie
                {
                    Movie = m,
                    SimilarityScore = CosineSimilarity(userProfile, weightedEmotions),
                    EmotionProfile = weightedEmotions
                };
            })
            .OrderByDescending(x => x.SimilarityScore)
            .Take(topN)
            .ToList();

        return recommendedMovies;
    }

    // Obliczanie profilu emocjonalnego filmów
    public static Dictionary<int, EmotionProfile> ComputeMovieEmotionProfiles(List<Review> allReviews)
    {
        if (allReviews == null) return new Dictionary<int, EmotionProfile>();

        return allReviews
            .GroupBy(r => r.MovieId)
            .Select(group =>
            {
                var avgEmotions = new float[EmotionOrder.Length];
                foreach (var review in group)
                {
                    if (review == null) continue;

                    var emotionMap = GetEmotionDictionary(review);
                    for (int i = 0; i < EmotionOrder.Length; i++)
                        avgEmotions[i] += emotionMap.ContainsKey(EmotionOrder[i]) ? emotionMap[EmotionOrder[i]] : 0f;
                }
                for (int i = 0; i < avgEmotions.Length; i++)
                    avgEmotions[i] /= group.Count();

                NormalizeVectorL2(avgEmotions);

                return new EmotionProfile { MovieId = group.Key, Emotions = avgEmotions };
            })
            .ToDictionary(e => e.MovieId, e => e);
    }


    // Obliczanie profilu emocjonalnego użytkownika
    public static float[] ComputeUserEmotionProfile(List<Rating> allRatings, Dictionary<int, EmotionProfile> movieEmotions)
    {
        if (allRatings == null || movieEmotions == null)
            return new float[EmotionOrder.Length];

        float[] userProfile = new float[EmotionOrder.Length];
        float totalWeight = 0;

        foreach (var rating in allRatings)
        {
            if (rating == null) continue;
            if (!movieEmotions.ContainsKey(rating.MovieId)) continue;

            var emotion = movieEmotions[rating.MovieId].Emotions;
            float ageWeight = GetAgeWeight(rating.RatingDate);

            for (int i = 0; i < userProfile.Length; i++)
                userProfile[i] += emotion[i] * rating.Score * ageWeight;

            totalWeight += rating.Score * ageWeight;
        }

        for (int i = 0; i < userProfile.Length; i++)
            userProfile[i] = totalWeight > 0 ? userProfile[i] / totalWeight : 0f;

        NormalizeVectorL2(userProfile);

        return userProfile;
    }

    private static Dictionary<string, float> GetEmotionDictionary(Review review)
    {
        var rawPairs = new (string Emotion, float Score)?[]
        {
            string.IsNullOrWhiteSpace(review.Emotion1) ? null : (review.Emotion1.ToLower(), review.Score1),
            string.IsNullOrWhiteSpace(review.Emotion2) ? null : (review.Emotion2.ToLower(), review.Score2),
            string.IsNullOrWhiteSpace(review.Emotion3) ? null : (review.Emotion3.ToLower(), review.Score3),
            string.IsNullOrWhiteSpace(review.Emotion4) ? null : (review.Emotion4.ToLower(), review.Score4),
            string.IsNullOrWhiteSpace(review.Emotion5) ? null : (review.Emotion5.ToLower(), review.Score5),
            string.IsNullOrWhiteSpace(review.Emotion6) ? null : (review.Emotion6.ToLower(), review.Score6),
        };

        return rawPairs
            .Where(p => p.HasValue && EmotionOrder.Contains(p.Value.Emotion))
            .GroupBy(p => p.Value.Emotion)
            .ToDictionary(g => g.Key, g => g.Sum(p => p.Value.Score));
    }

    public static void NormalizeVectorL2(float[] vec)
    {
        float sumSquares = 0f;
        for (int i = 0; i < vec.Length; i++)
            sumSquares += vec[i] * vec[i];

        float norm = (float)Math.Sqrt(sumSquares);
        if (norm > 0f)
        {
            for (int i = 0; i < vec.Length; i++)
                vec[i] /= norm;
        }
    }

    private static float CosineSimilarity(float[] vec1, float[] vec2)
    {
        float dot = 0f, normA = 0f, normB = 0f;
        for (int i = 0; i < vec1.Length; i++)
        {
            dot += vec1[i] * vec2[i];
            normA += vec1[i] * vec1[i];
            normB += vec2[i] * vec2[i];
        }

        return (float)(dot / (Math.Sqrt(normA) * Math.Sqrt(normB) + 1e-8));
    }
         
    //Obliczanie wag dla emocji
    public static float[] ComputeEmotionWeights(List<Rating> ratings, Dictionary<int, EmotionProfile> movieEmotions)
    {
        int dim = EmotionOrder.Length;
        var emotionScores = new List<float>[dim];
        var userScores = new List<float>();

        for (int i = 0; i < dim; i++)
            emotionScores[i] = new List<float>();

        foreach (var rating in ratings)
        {
            if (!movieEmotions.ContainsKey(rating.MovieId)) continue;
            var emotions = movieEmotions[rating.MovieId].Emotions;

            for (int i = 0; i < dim; i++)
                emotionScores[i].Add(emotions[i]);

            userScores.Add(rating.Score);
        }

        float[] weights = new float[dim];

        for (int i = 0; i < dim; i++)
        {
            float correlation = ComputePearson(emotionScores[i], userScores);

            int n = emotionScores[i].Count;
            const float shrinkageK = 10f;
            float shrinkFactor = (float)n / (n + shrinkageK);

            float shrunkCorrelation = correlation * shrinkFactor;
            weights[i] = NormalizeCorrelationToWeight(shrunkCorrelation);
        }

        return weights;
    }

    private static float ComputePearson(List<float> x, List<float> y)
    {
        if (x.Count != y.Count || x.Count == 0) return 0;

        float avgX = x.Average();
        float avgY = y.Average();

        float sumXY = 0, sumX2 = 0, sumY2 = 0;
        for (int i = 0; i < x.Count; i++)
        {
            float dx = x[i] - avgX;
            float dy = y[i] - avgY;
            sumXY += dx * dy;
            sumX2 += dx * dx;
            sumY2 += dy * dy;
        }

        float denom = (float)(Math.Sqrt(sumX2) * Math.Sqrt(sumY2));
        return denom > 0 ? sumXY / denom : 0;
    }

    private static float NormalizeCorrelationToWeight(float correlation)
    {
        return Math.Clamp((correlation + 1f) / 2f, 0f, 1f);
    }


    //"Starzenie się ocen"
    private static float GetAgeWeight(DateTime ratingDate)
    {
        var now = DateTime.UtcNow;
        var ageInDays = (now - ratingDate.ToUniversalTime()).Days;

        const float lambda = 0.002f;
        const float minWeight = 0.05f; 
        float weight = (float)Math.Exp(-lambda * ageInDays);

        return Math.Clamp(weight, minWeight, 1f);
    }
}
