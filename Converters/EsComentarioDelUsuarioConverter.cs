using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClienteAminoExo.Utils;
using System.Windows.Data;
using System.Windows;

namespace ClienteAminoExo.Converters
{
    public class EsComentarioDelUsuarioConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int usuarioComentario = (int)value;
            bool esDelUsuarioActual = usuarioComentario == SesionActual.UsuarioId;

            if (parameter?.ToString() == "invertido")
            {
                return !esDelUsuarioActual ? Visibility.Visible : Visibility.Collapsed;
            }

            return esDelUsuarioActual ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }


}
