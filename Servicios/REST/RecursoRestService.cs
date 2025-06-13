using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClienteAminoExo.Servicios.REST
{
    public class RecursoDTO
    {
        public int identificador { get; set; }
        public string tipo { get; set; }          // "Foto", "Audio", "Video"
        public string formato { get; set; }       // "jpg", "mp3", "mp4"
        [JsonProperty("URL")]
        public string url { get; set; }
    }


    public class RecursoRestService
    {
        private readonly HttpClient _http;

        public RecursoRestService(string token)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri("http://localhost:3000") // Ajusta si tu backend tiene otro puerto
            };
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<RecursoDTO> ObtenerRecursoPorIdAsync(int recursoId)
        {
            var response = await _http.GetAsync($"/api/recursos/{recursoId}");
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RecursoDTO>(json);
        }
    }
}
