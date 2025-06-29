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

namespace ClienteAminoExo.Servicios.REST
{

    public class ComentarioDTO
    {
        public int comentarioId { get; set; } 
        public string texto { get; set; }
        public int publicacionId { get; set; }
        public int usuarioId { get; set; }

        public string nombreUsuario { get; set; }

    }


    public class ComentarioRestService
    {
        private readonly HttpClient _httpClient;

        public ComentarioRestService(string token)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BackendConfig.BackendBaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<bool> CrearComentarioAsync(ComentarioDTO comentario)
        {
            var response = await _httpClient.PostAsJsonAsync("api/comentarios", comentario);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<ComentarioDTO>> ObtenerComentariosPorPublicacionAsync(int publicacionId)
        {
            var response = await _httpClient.GetAsync($"api/comentarios/publicacion/{publicacionId}");
            if (!response.IsSuccessStatusCode)
                return new List<ComentarioDTO>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ComentarioDTO>>(json);
        }

        public async Task<int> ObtenerConteoPorPublicacionAsync(int publicacionId)
        {
            var comentarios = await ObtenerComentariosPorPublicacionAsync(publicacionId);
            return comentarios?.Count ?? 0;
        }

        public async Task<bool> ActualizarComentarioAsync(int comentarioId, string nuevoTexto)
        {
            var contenido = new StringContent(
                JsonConvert.SerializeObject(new { texto = nuevoTexto }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PutAsync($"api/comentarios/{comentarioId}", contenido);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> EliminarComentarioAsync(int comentarioId)
        {
            var response = await _httpClient.DeleteAsync($"api/comentarios/{comentarioId}");
            return response.IsSuccessStatusCode;
        }


    }

}
