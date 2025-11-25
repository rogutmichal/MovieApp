using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp
{
    public class PercentToWidthConverter : IValueConverter
    {
        public double MaxWidth { get; set; } = 120;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is float f)
            {
                double percent = Math.Clamp(f, 0, 1);
                return MaxWidth * percent;
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}