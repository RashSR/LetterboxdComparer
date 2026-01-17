using System;
using System.Globalization;
using System.Windows.Data;

namespace LetterboxdComparer
{
    public class CountToHeightConverter : IValueConverter
    {
        // Converts count to pixel height for the bar
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
                return count * 5;

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
