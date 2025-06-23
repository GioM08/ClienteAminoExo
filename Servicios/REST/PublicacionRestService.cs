using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;
using ClienteAminoExo.Utils;
using System.Windows;
using System.Text.Json;


namespace ClienteAminoExo.Servicios.REST
{
    public class PublicacionDTO
    {
        public int identificador { get; set; }
        public string titulo { get; set; }
        public string contenido { get; set; }
        public int usuarioId { get; set; }
        public string estado { get; set; } = "Publicado";
        public int recursoId { get; set; }
        public DateTime fechaCreacion { get; set; }

        public RecursoDTO recurso { get; set; }
    }
    public class RespuestaPublicaciones
    {
        public int total { get; set; }
        public int paginas { get; set; }
        public int paginaActual { get; set; }
        public List<PublicacionDTO> resultados { get; set; }
    }


    public class PublicacionRestService
    {
        private readonly HttpClient _httpClient = new();

        public PublicacionRestService(string token)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BackendConfig.BackendBaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            if (!string.IsNullOrEmpty(SesionActual.Rol))
            {
                _httpClient.DefaultRequestHeaders.Add("User-Role", SesionActual.Rol);
            }
        }

        public async Task<PublicacionDTO> CrearPublicacionConRespuestaAsync(PublicacionDTO publicacion)
        {
            var json = JsonConvert.SerializeObject(publicacion);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/publicaciones", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                MessageBox.Show("Error REST: " + response.StatusCode + "\nRespuesta: " + errorJson);
                return null;
            }

            var responseJson = await response.Content.ReadAsStringAsync();

            var respuesta = JsonConvert.DeserializeObject<RespuestaCrearPublicacion>(responseJson);
            return respuesta?.publicacion;
        }



        public async Task<bool> CrearPublicacionAsync(PublicacionDTO publicacion)
        {
            var response = await _httpClient.PostAsJsonAsync("api/publicaciones", publicacion);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<PublicacionDTO>> ObtenerPublicacionesAsync()
        {
            var response = await _httpClient.GetAsync("/api/publicaciones/con-recursos");
            if (!response.IsSuccessStatusCode)
                return new List<PublicacionDTO>();

            var json = await response.Content.ReadAsStringAsync();
            var publicaciones = JsonConvert.DeserializeObject<List<PublicacionDTO>>(json);
            return publicaciones ?? new List<PublicacionDTO>();
        }


        public async Task EliminarPublicacionAsync(int identificador)
        {
            var url = $"{BackendConfig.BackendBaseUrl}/api/publicaciones/{identificador}";

            var contenidoJson = new
            {
                rol = SesionActual.Rol,
                usuarioId = SesionActual.UsuarioId
            };

            var json = System.Text.Json.JsonSerializer.Serialize(contenidoJson);
            var request = new HttpRequestMessage(HttpMethod.Delete, url)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SesionActual.Token);

            var client = new HttpClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error al eliminar publicación: {response.StatusCode} - {error}");
            }
        }


        public void ActualizarHeaders()
        {
            if (!string.IsNullOrEmpty(SesionActual.Rol))
            {
                _httpClient.DefaultRequestHeaders.Remove("User-Role"); 
                _httpClient.DefaultRequestHeaders.Add("User-Role", SesionActual.Rol);
            }
        }

        public async Task<RespuestaBusquedaPublicaciones> BuscarPublicacionesAsync(
            string query = "",
            string tipoRecurso = null,
            string categorias = null,
            int pagina = 1,
            int limite = 10)
        {
            var parametros = new List<string>();

            if (!string.IsNullOrEmpty(query))
                parametros.Add($"query={Uri.EscapeDataString(query)}");

            if (!string.IsNullOrEmpty(tipoRecurso))
                parametros.Add($"tipoRecurso={Uri.EscapeDataString(tipoRecurso)}");

            if (!string.IsNullOrEmpty(categorias))
                parametros.Add($"categorias={Uri.EscapeDataString(categorias)}");

            parametros.Add($"page={pagina}");
            parametros.Add($"limit={limite}");

            string url = "/api/publicaciones/buscar";

            if (parametros.Count > 0)
                url += "?" + string.Join("&", parametros);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            var resultado = JsonConvert.DeserializeObject<RespuestaBusquedaPublicaciones>(json);

            return resultado;
        }



    }

    public class RespuestaCrearPublicacion
    {
        public string msg { get; set; }
        public PublicacionDTO publicacion { get; set; }
    }

    public class RespuestaBusquedaPublicaciones
    {
        public int total { get; set; }
        public int paginas { get; set; }
        public int paginaActual { get; set; }
        public List<PublicacionDTO> resultados { get; set; }
    }


}
