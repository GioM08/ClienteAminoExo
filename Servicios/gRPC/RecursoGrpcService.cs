using ClienteAminoExo.Utils;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Recurso;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClienteAminoExo.Servicios.gRPC
{

    public class RecursoGrpcService
    {
        private readonly RecursoService.RecursoServiceClient _cliente;

        public RecursoGrpcService()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var canal = GrpcChannel.ForAddress("http://192.168.200.88:50054");
            _cliente = new RecursoService.RecursoServiceClient(canal);
        }

        public async Task<CrearRecursoResponse> CrearRecursoAsync(
            string rutaArchivo,
            string tipo,
            int formato,
            int tamano,
            int usuarioId,
            int? publicacionId,
            string jwt,
            int? resolucion = null,
            int? duracion = null)
        {
            try
            {
                if (!File.Exists(rutaArchivo))
                    return new CrearRecursoResponse { Exito = false, Mensaje = "El archivo no existe" };

                var archivoBytes = await File.ReadAllBytesAsync(rutaArchivo);
                if (archivoBytes.Length == 0)
                    return new CrearRecursoResponse { Exito = false, Mensaje = "El archivo está vacío" };

                var identificadorTemporal = DateTime.Now.Ticks.GetHashCode() & 0xfffffff;

                var request = new CrearRecursoRequest
                {
                    Tipo = tipo,
                    Identificador = identificadorTemporal,
                    Formato = formato,
                    Tamano = tamano,
                    UsuarioId = usuarioId,
                    Archivo = ByteString.CopyFrom(archivoBytes)
                };

                if (publicacionId.HasValue)
                    request.PublicacionId = publicacionId.Value;

                if (tipo == "Foto" || tipo == "Video")
                    request.Resolucion = resolucion ?? 1920;
                else if (tipo == "Audio")
                    request.Duracion = duracion ?? 60;

                var headers = new Metadata
                {
                    { "Authorization", $"Bearer {jwt}" },
                    { "User-Role", SesionActual.Rol ?? "" }
                };

                return await _cliente.CrearRecursoAsync(request, headers);
            }
            catch (RpcException ex)
            {
                return new CrearRecursoResponse
                {
                    Exito = false,
                    Mensaje = $"Error del servidor: {ex.Status.Detail}"
                };
            }
        }

        public async Task<(bool exito, byte[] archivo, string mensaje)> DescargarRecursoAsync(string tipo, int identificador)
        {
            using var channel = Grpc.Net.Client.GrpcChannel.ForAddress("http://192.168.200.88:50054", new GrpcChannelOptions
            {
                MaxReceiveMessageSize = 50 * 1024 * 1024
            });

            var client = new RecursoService.RecursoServiceClient(channel);

            var request = new DescargarRecursoRequest
            {
                Tipo = tipo,
                Identificador = identificador
            };

            try
            {
                var respuesta = await client.DescargarRecursoAsync(request);

                if (!respuesta.Exito)
                    return (false, null, respuesta.Mensaje);

                return (true, respuesta.Archivo.ToByteArray(), "Recurso descargado exitosamente");
            }
            catch (RpcException ex)
            {
                return (false, null, $"Error al descargar recurso: {ex.Status.Detail}");
            }
        }

    }
}
