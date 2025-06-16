using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ClienteAminoExo.Utils
{
    public class Validaciones
    {
        public static string ValidarNombre(string texto, string nombreCampo)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return $"{nombreCampo} no puede estar vacío.";

            var regex = new Regex(@"^[A-Za-zÁÉÍÓÚáéíóúÑñ]+(\s[A-Za-zÁÉÍÓÚáéíóúÑñ]+)*$");
            if (!regex.IsMatch(texto))
                return $"{nombreCampo} solo debe contener letras y un espacio entre palabras.";

            return null;
        }

        public static string ValidarNombreDeUsuario(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "El nombre de usuario no puede estar vacío.";

            var regex = new Regex(@"^[a-zA-Z0-9_]{4,20}$");
            if (!regex.IsMatch(texto))
                return "El nombre de usuario solo debe contener letras, números y guiones bajos (4-20 caracteres).";

            return null;
        }

        public static string ValidarCorreo(string correo)
        {
            if (string.IsNullOrWhiteSpace(correo))
                return "El correo no puede estar vacío.";

            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            if (!regex.IsMatch(correo))
                return "Correo electrónico inválido.";

            return null;
        }

        public static string ValidarPassword(string pass)
        {
            if (string.IsNullOrWhiteSpace(pass))
                return "La contraseña no puede estar vacía.";

            if (pass.Length < 8)
                return "La contraseña debe tener al menos 8 caracteres.";

            var regex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&._-])[A-Za-z\d@$!%*?&._-]{8,}$");
            if (!regex.IsMatch(pass))
                return "La contraseña debe tener al menos una mayúscula, una minúscula, un número y un símbolo.";

            return null;
        }

        public static string ValidarTitulo(string titulo)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                return "El título no puede estar vacío.";

            if (titulo.Length > 100)
                return "El título no puede tener más de 100 caracteres.";

            var regex = new Regex(@"[<>""'{}]");
            if (regex.IsMatch(titulo))
                return "El título contiene caracteres no permitidos.";

            return null;
        }

        public static string ValidarLongitud(string texto, string nombreCampo, int min, int max)
        {
            if (texto.Length < min)
                return $"{nombreCampo} debe tener al menos {min} caracteres.";

            if (texto.Length > max)
                return $"{nombreCampo} no debe tener más de {max} caracteres.";

            return null;
        }
    }
}
