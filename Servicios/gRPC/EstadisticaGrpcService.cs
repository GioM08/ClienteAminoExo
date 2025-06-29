﻿using Estadistica;
using Grpc.Net.Client;
using System.Threading.Tasks;

namespace ClienteAminoExo.Servicios.gRPC
{
    public class EstadisticaGrpcService
    {
        private readonly EstadisticasService.EstadisticasServiceClient _cliente;

        public EstadisticaGrpcService()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var canal = GrpcChannel.ForAddress("http://localhost:50055");
            _cliente = new EstadisticasService.EstadisticasServiceClient(canal);
        }

        public async Task<EstadisticasResponse> ObtenerEstadisticasAsync()
        {
            return await _cliente.ObtenerEstadisticasAsync(new EstadisticasRequest());
        }
    }
}
