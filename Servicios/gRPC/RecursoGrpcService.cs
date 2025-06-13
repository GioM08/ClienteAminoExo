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
        private readonly string grpcUrl = "http://localhost:50054"; // Ajusta si estás usando otro puerto

        public async Task<(bool exito, int identificador, string mensaje)> CrearRecursoAsync(
            string rutaArchivo,
            string tipo,
            string formato,
            long tamano,
            int usuarioId,
            string resolucion = null,
            string duracion = null)
        {
            using var channel = GrpcChannel.ForAddress(grpcUrl);
            var client = new RecursoService.RecursoServiceClient(channel);

            var archivoBytes = await File.ReadAllBytesAsync(rutaArchivo);

            var request = new CrearRecursoRequest
            {
                Tipo = tipo,
                Formato = formato,
                Tamano = tamano.ToString(),
                UsuarioId = usuarioId,
                Archivo = ByteString.CopyFrom(archivoBytes)
            };

            if (tipo == "Foto" || tipo == "Video")
                request.Resolucion = resolucion ?? "1920x1080";
            else if (tipo == "Audio")
                request.Duracion = duracion ?? "00:01:00";

            var respuesta = await client.CrearRecursoAsync(request);

            return (
                exito: respuesta.Exito,
                identificador: respuesta.Identificador,
                mensaje: respuesta.Mensaje
            );
        }

        public async Task<(bool exito, byte[] archivo, string mensaje)> DescargarRecursoAsync(string tipo, int identificador)
        {
            using var channel = Grpc.Net.Client.GrpcChannel.ForAddress(grpcUrl);
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
