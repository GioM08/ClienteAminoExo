using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ClienteAminoExo.Servicios.REST
{
    public class UsuarioRestService
    {
        private readonly HttpClient _httpClient;

        public UsuarioRestService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:3000/")
            };
        }

        public async Task<HttpResponseMessage> CrearCuentaAsync(string nombre, string apellidos, string correo, string contrasena)
        {
            var usuario = new
            {
                nombre = nombre,
                apellidos = apellidos,
                correo = correo,
                contrasena = contrasena
            };

            return await _httpClient.PostAsJsonAsync("api/usuarios", usuario);
        }
    }

}
