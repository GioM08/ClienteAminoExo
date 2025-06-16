using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using ClienteAminoExo.Utils;

namespace ClienteAminoExo.Servicios.REST
{
    public class RecursoDTO
    {
        public int identificador { get; set; }
        public string tipo { get; set; }          
        public string formato { get; set; }       

        [JsonProperty("URL")]
        public string url { get; set; }

        public int usuarioId { get; set; }
        public string resolucion { get; set; }    
        public string duracion { get; set; }      
    }

    public class RecursoRestService
    {
        private readonly HttpClient _http;

        public RecursoRestService(string token)
        {
            _http = new HttpClient
            {
                BaseAddress = new Uri(BackendConfig.BackendBaseUrl)
            };
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<RecursoDTO> ObtenerRecursoPorIdAsync(int recursoId)
        {
            var response = await _http.GetAsync($"{BackendConfig.BackendBaseUrl}/api/recursos/{recursoId}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RecursoDTO>(json);
        }


        public void ActualizarHeaders()
        {
            if (!string.IsNullOrEmpty(SesionActual.Rol))
            {
                _http.DefaultRequestHeaders.Remove("User-Role"); 
                _http.DefaultRequestHeaders.Add("User-Role", SesionActual.Rol);
            }
        }
    }
}
