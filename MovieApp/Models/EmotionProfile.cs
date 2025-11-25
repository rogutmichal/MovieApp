using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Models
{
    public class EmotionProfile
    {
        public int MovieId { get; set; }
        public float[] Emotions { get; set; }
    }

}
