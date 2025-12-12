using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace QualisulCameraApp.Converters
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSessionActive && isSessionActive)
            {
                return new SolidColorBrush(Color.FromRgb(220, 53, 69)); // Red (Encerrar)
            }
            return new SolidColorBrush(Color.FromRgb(0, 166, 81)); // Green (Iniciar)
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
