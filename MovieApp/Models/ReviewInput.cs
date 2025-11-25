using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Models
{
    public class ReviewInput
    {
        [ColumnName("Emotion")]
        public string Emotion { get; set; }

        [ColumnName("ReviewText")]
        public string ReviewText { get; set; }
    }
}
