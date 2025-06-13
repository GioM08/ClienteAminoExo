using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteAminoExo.Utils
{
    public static class SesionActual
    {
        public static string Token { get; set; }
        public static int UsuarioId { get; set; }
        
        public static void CerrarSesion()
        {
            Token = null;
            UsuarioId = 0;
        }


    }



}
