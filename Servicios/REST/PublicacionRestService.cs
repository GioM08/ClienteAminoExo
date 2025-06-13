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
            _httpClient.BaseAddress = new Uri("http://localhost:3000"); // Ajusta si es necesario
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> CrearPublicacionAsync(PublicacionDTO publicacion)
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:3000/api/publicaciones", publicacion);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<PublicacionDTO>> ObtenerPublicacionesAsync()
        {
            var response = await _httpClient.GetAsync("/api/publicaciones");
            if (!response.IsSuccessStatusCode)
                return new List<PublicacionDTO>();

            var json = await response.Content.ReadAsStringAsync();
            var wrapper = JsonConvert.DeserializeObject<RespuestaPublicaciones>(json);
            return wrapper?.resultados ?? new List<PublicacionDTO>();
        }



    }
}
