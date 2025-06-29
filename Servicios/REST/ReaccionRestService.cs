﻿using ClienteAminoExo.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ClienteAminoExo.Servicios.REST
{

    public class ReaccionDTO
    {
        public int reaccionId { get; set; } 
        public string tipo { get; set; } 
        public int publicacionId { get; set; }
        public int usuarioId { get; set; }
        public string nombreUsuario { get; set; }
        public DateTime fecha { get; set; }
    }



    public class ReaccionRestService
    {
        private readonly HttpClient _httpClient;

        public ReaccionRestService(string token)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BackendConfig.BackendBaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> CrearReaccionAsync(ReaccionDTO reaccion)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/reacciones", reaccion);
                if (!response.IsSuccessStatusCode)
                {
                    var msg = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"❌ Error HTTP {response.StatusCode}\n{msg}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Excepción al enviar reacción:\n{ex.Message}");
                return false;
            }
        }


        public async Task<List<ReaccionDTO>> ObtenerReaccionesPorPublicacionAsync(int publicacionId)
        {
            var response = await _httpClient.GetAsync($"api/reacciones/publicacion/{publicacionId}");
            if (!response.IsSuccessStatusCode)
                return new List<ReaccionDTO>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ReaccionDTO>>(json);
        }

        public async Task<int> ObtenerConteoPorPublicacionAsync(int publicacionId)
        {
            var reacciones = await ObtenerReaccionesPorPublicacionAsync(publicacionId);
            return reacciones?.Count ?? 0;
        }

        public async Task<ReaccionDTO> ObtenerReaccionDelUsuarioAsync(int publicacionId, int usuarioId)
        {
            var todas = await ObtenerReaccionesPorPublicacionAsync(publicacionId);
            return todas.FirstOrDefault(r => r.usuarioId == usuarioId);
        }

        public async Task<bool> ActualizarReaccionAsync(ReaccionDTO reaccion)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/reacciones/{reaccion.reaccionId}", reaccion);
            return response.IsSuccessStatusCode;
        }

        public async Task<int?> ObtenerReaccionIdAsync(int usuarioId, int publicacionId)
        {
            var url = $"{BackendConfig.BackendBaseUrl}/reacciones/buscar?usuarioId={usuarioId}&publicacionId={publicacionId}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SesionActual.Token);

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(json);

            return (int?)data.reaccionId;
        }

        public async Task<string?> ObtenerTipoReaccionAsync(int usuarioId, int publicacionId)
        {
            var url = $"{BackendConfig.BackendBaseUrl}/reacciones/buscar?usuarioId={usuarioId}&publicacionId={publicacionId}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", SesionActual.Token);

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<dynamic>(json);

            return (string?)data.tipo;
        }





    }

}
