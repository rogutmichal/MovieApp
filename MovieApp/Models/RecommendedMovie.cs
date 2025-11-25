using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Models
{
    public class RecommendedMovie
    {
        public Movie Movie { get; set; }
        public float SimilarityScore { get; set; } 

        public float[] EmotionProfile { get; set; }
    }

}
