using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Models
{
    public class EmotionPrediction
    {
        [ColumnName("Score")]
        public float[] Score { get; set; }
    }

}
