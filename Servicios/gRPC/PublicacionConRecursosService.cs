using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ClienteAminoExo.Utils;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Recurso;

namespace ClienteAminoExo.Servicios
{
    public class PublicacionConRecursosService
    {
        private readonly string _jwt;
        private readonly string _baseUrlBackend;

        public PublicacionConRecursosService(string jwt, string baseUrlBackend = "http://tubackend.com")
        {
            _jwt = jwt;
            _baseUrlBackend = baseUrlBackend;
        }

        public async Task<int> PublicarConRecursosAsync(
            string titulo,
            string contenido,
            List<string> rutasArchivos,
            int usuarioId)
        {
            try
            {
                // 1. Crear la publicación via REST
                var publicacionId = await CrearPublicacionRestAsync(titulo, contenido, usuarioId);

                // 2. Subir recursos via gRPC
                await SubirRecursosAsync(rutasArchivos, usuarioId, publicacionId);

                return publicacionId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al publicar: {ex.Message}");
            }
        }

        private async Task<int> CrearPublicacionRestAsync(string titulo, string contenido, int usuarioId)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _jwt);

            // Añadir header de rol si existe en SesionActual
            if (!string.IsNullOrEmpty(SesionActual.Rol))
            {
                httpClient.DefaultRequestHeaders.Add("User-Role", SesionActual.Rol);
            }

            var request = new
            {
                titulo,
                contenido,
                usuarioId,
                estado = "Publicado"
            };

            var response = await httpClient.PostAsync(
                $"{_baseUrlBackend}/api/publicaciones",
                new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json"));

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Error REST: {response.StatusCode}");

            var responseData = await JsonSerializer.DeserializeAsync<Dictionary<string, object>>(
                await response.Content.ReadAsStreamAsync());

            return Convert.ToInt32(responseData["identificador"]);
        }

        private async Task SubirRecursosAsync(List<string> rutasArchivos, int usuarioId, int publicacionId)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress("http://localhost:50054");
            var client = new RecursoService.RecursoServiceClient(channel);

            var headers = new Metadata { { "Authorization", $"Bearer {_jwt}" } };

            foreach (var ruta in rutasArchivos)
            {
                var tipo = Path.GetExtension(ruta).ToLower() switch
                {
                    ".jpg" or ".png" => "Foto",
                    ".mp4" or ".mov" => "Video",
                    ".mp3" or ".wav" => "Audio",
                    _ => throw new Exception($"Formato no soportado: {Path.GetExtension(ruta)}")
                };

                var fileInfo = new FileInfo(ruta);
                var archivoBytes = await File.ReadAllBytesAsync(ruta);

                var request = new CrearRecursoRequest
                {
                    Tipo = tipo,
                    Identificador = DateTime.Now.Ticks.GetHashCode() & 0xfffffff, // ID temporal
                    Formato = tipo switch { "Foto" => 1, "Video" => 3, "Audio" => 2, _ => 0 },
                    Tamano = (int)fileInfo.Length,
                    UsuarioId = usuarioId,
                    PublicacionId = publicacionId,
                    Archivo = ByteString.CopyFrom(archivoBytes)
                };

                if (tipo == "Foto" || tipo == "Video")
                    request.Resolucion = 1920; // Valor por defecto
                else if (tipo == "Audio")
                    request.Duracion = 60; // Valor por defecto

                var respuesta = await client.CrearRecursoAsync(request, headers);

                if (!respuesta.Exito)
                    throw new Exception($"Error gRPC: {respuesta.Mensaje}");
            }
        }
    }
}