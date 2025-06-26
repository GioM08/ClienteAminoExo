using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ClienteAminoExo.Utils;

namespace ClienteAminoExo.Servicios.REST
{
    public class UsuarioRestService
    {
        private readonly HttpClient _httpClient;

        public UsuarioRestService(string token)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BackendConfig.BackendBaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public UsuarioRestService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BackendConfig.BackendBaseUrl)
            };
        }


        public class UsuarioPerfilResponse
        {
            public string msg { get; set; }
            public Usuario usuario { get; set; }
        }

        public class Usuario
        {
            public int usuarioId { get; set; }
            public string nombreUsuario { get; set; }
            public string nombre { get; set; }
            public string apellidos { get; set; }
            public string correo { get; set; }
            public string rol { get; set; }
        }



        public async Task<HttpResponseMessage> CrearCuentaAsync(
            string nombreUsuario,
            string nombre,
            string apellidos,
            string correo,
            string contrasena,
            string rol)
        {
            var usuario = new
            {
                nombreUsuario = nombreUsuario,
                nombre = nombre,
                apellidos = apellidos,
                correo = correo,
                contrasena = contrasena,
                rol = rol
            };

            return await _httpClient.PostAsJsonAsync("api/usuarios", usuario);
        }

        public async Task<UsuarioPerfilResponse> ObtenerPerfilAsync()
        {
            var response = await _httpClient.GetAsync("api/usuarios/perfil");
            response.EnsureSuccessStatusCode();
            var contenido = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<UsuarioPerfilResponse>(contenido);
        }

        public async Task<HttpResponseMessage> ActualizarUsuarioAsync(int id, string nombre, string apellidos, string correo, string nombreUsuario)
        {
            var data = new { nombre, apellidos, correo, nombreUsuario };
            return await _httpClient.PutAsJsonAsync($"api/usuarios/{id}", data);
        }

        public async Task<HttpResponseMessage> CambiarContrasenaAsync(int id, string nuevaContrasena)
        {
            var data = new { nuevaContrasena };
            return await _httpClient.PutAsJsonAsync($"api/usuarios/{id}/contrasena", data);
        }

        public async Task<HttpResponseMessage> EliminarUsuarioAsync(int id)
        {
            return await _httpClient.DeleteAsync($"api/usuarios/{id}");
        }

        public void ActualizarHeaders()
        {
            if (!string.IsNullOrEmpty(SesionActual.Rol))
            {
                _httpClient.DefaultRequestHeaders.Remove("User-Role"); 
                _httpClient.DefaultRequestHeaders.Add("User-Role", SesionActual.Rol);
            }
        }

        public async Task<UsuarioDTO?> ObtenerUsuarioPorIdAsync(int usuarioId)
        {
            var response = await _httpClient.GetAsync($"api/usuarios/{usuarioId}");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            // La respuesta es un objeto que contiene { ok: true, usuario: { ... } }
            var wrapper = JsonConvert.DeserializeObject<UsuarioRespuestaDTO>(json);
            return wrapper?.usuario;
        }

        public class UsuarioRespuestaDTO
        {
            public bool ok { get; set; }
            public UsuarioDTO? usuario { get; set; }
        }

        public class UsuarioDTO
        {
            public int usuarioId { get; set; }
            public string nombreUsuario { get; set; }
        }


    }

}
