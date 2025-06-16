using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ClienteAminoExo.Utils;

namespace ClienteAminoExo.Servicios.REST
{
    public class NotificacionRestService
    {
        private readonly HttpClient _httpClient;

        public NotificacionRestService(string token)
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BackendConfig.BackendBaseUrl)
            };
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public NotificacionRestService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BackendConfig.BackendBaseUrl)
            };
        }

        public class Notificacion
        {
            public string _id { get; set; }
            public int notificacionId { get; set; }
            public int usuarioId { get; set; }
            public string mensaje { get; set; }
            public bool leida { get; set; }
            public DateTime fecha { get; set; }
        }

        public class RespuestaNotificaciones
        {
            public int total { get; set; }
            public int paginas { get; set; }
            public int paginaActual { get; set; }
            public List<Notificacion> resultados { get; set; }
        }

        // Obtener notificaciones de un usuario (con paginación y filtro de leídas)
        public async Task<RespuestaNotificaciones> ObtenerNotificacionesAsync(int usuarioId, bool? leida = null, int limit = 10, int page = 1)
        {
            var query = $"?limit={limit}&page={page}";
            if (leida.HasValue)
                query += $"&leida={leida.Value.ToString().ToLower()}";

            var response = await _httpClient.GetAsync($"/api/notificaciones/usuario/{usuarioId}");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<RespuestaNotificaciones>(json);
        }

        // Marcar una notificación como leída
        public async Task<HttpResponseMessage> MarcarComoLeidaAsync(int notificacionId)
        {
            return await _httpClient.PatchAsync($"/api/notificaciones/{notificacionId}/marcar-leida", null);
        }

        public async Task<HttpResponseMessage> MarcarMultiplesComoLeidasAsync(List<string> ids)
        {
            var data = new { ids };
            var json = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            return await _httpClient.PatchAsync("/api/notificaciones/marcar-leidas", json);
        }

        // Eliminar una notificación
        public async Task<HttpResponseMessage> EliminarNotificacionAsync(int notificacionId)
        {
            return await _httpClient.DeleteAsync($"api/notificaciones/{notificacionId}");
        }
    }
}
