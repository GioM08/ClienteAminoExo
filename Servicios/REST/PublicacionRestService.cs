using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace ClienteAminoExo.Servicios.REST
{
    public class PublicacionDTO
    {
        public string titulo { get; set; }
        public string contenido { get; set; }
        public int usuarioId { get; set; }
        public string estado { get; set; } = "Publicado";
        public int recursoId { get; set; }
    }

    public class PublicacionRestService
    {
        private readonly HttpClient _httpClient = new();

        public async Task<bool> CrearPublicacionAsync(PublicacionDTO publicacion)
        {
            var response = await _httpClient.PostAsJsonAsync("http://localhost:3000/api/publicaciones", publicacion);
            return response.IsSuccessStatusCode;
        }
    }
}
