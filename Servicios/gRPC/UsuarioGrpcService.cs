using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Net.Client;
using Usuario;
using Grpc.Core;


namespace ClienteAminoExo.Servicios.gRPC
{

    public class UsuarioGrpcService
    {
        private readonly UsuarioService.UsuarioServiceClient _cliente;

        public UsuarioGrpcService()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var canal = GrpcChannel.ForAddress("http://localhost:50051");
            _cliente = new UsuarioService.UsuarioServiceClient(canal);
        }

        public async Task<LoginResponse> LoginAsync(string correo, string contrasena)
        {
            var request = new LoginRequest
            {
                Correo = correo,
                Contrasena = contrasena
            };

            return await _cliente.LoginAsync(request);
        }

        public async Task<PerfilResponse> ObtenerPerfilAsync(int usuarioId, string jwt)
        {
            var request = new PerfilRequest { UsuarioId = usuarioId };
            var headers = new Grpc.Core.Metadata
            {
                { "Authorization", $"Bearer {jwt}" }
            };

            return await _cliente.PerfilAsync(request, headers);
        }
    }

}
