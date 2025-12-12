using System;
using System.Globalization;
using System.Windows.Data;

namespace QualisulCameraApp.Converters
{
    public class BooleanToSessionTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSessionActive && isSessionActive)
            {
                return "ENCERRAR SESSÃO";
            }
            return "INICIAR SESSÃO";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
