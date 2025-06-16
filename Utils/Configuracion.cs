using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteAminoExo.Utils
{
    public static class Configuracion
    {
        static Configuracion()
        {
            Env.Load();
        }

        public static string IP => Environment.GetEnvironmentVariable("IP_CONEXION") ?? "localhost";

    }
}
