using Estadistica;
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
            var canal = GrpcChannel.ForAddress("http://192.168.200.88:50055"); // Usa el puerto correspondiente
            _cliente = new EstadisticasService.EstadisticasServiceClient(canal);
        }

        public async Task<EstadisticasResponse> ObtenerEstadisticasAsync()
        {
            return await _cliente.ObtenerEstadisticasAsync(new EstadisticasRequest());
        }
    }
}
