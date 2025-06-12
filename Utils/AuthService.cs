using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Usuario;

namespace ClienteAminoExo.Utils
{
    public class AuthService
    {
        private readonly UsuarioService.UsuarioServiceClient _client;
        private string _token;

        public string Token => _token;

        public AuthService()
        {
            var channel = GrpcChannel.ForAddress("http://localhost:50051"); // Cambia a https si aplica
            _client = new UsuarioService.UsuarioServiceClient(channel);
        }

        public async Task<bool> LoginAsync(string correo, string contrasena)
        {
            try
            {
                var response = await _client.LoginAsync(new LoginRequest
                {
                    Correo = correo,
                    Contrasena = contrasena
                });

                if (response.Exito)
                {
                    _token = response.Token;
                    return true;
                }
                else
                {
                    MessageBox.Show(response.Mensaje, "Login fallido", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return false;
                }
            }
            catch (RpcException ex)
            {
                MessageBox.Show("Error en el servidor: " + ex.Status.Detail, "gRPC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public async Task<UsuarioData?> ObtenerPerfilAsync(int usuarioId)
        {
            if (string.IsNullOrEmpty(_token))
            {
                MessageBox.Show("No hay token almacenado", "Autenticación requerida", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }

            var headers = new Metadata
            {
                { "authorization", $"Bearer {_token}" }
            };

            try
            {
                var perfilResponse = await _client.PerfilAsync(new PerfilRequest { UsuarioId = usuarioId }, headers);

                if (perfilResponse.Exito)
                {
                    return perfilResponse.Usuario;
                }

                MessageBox.Show(perfilResponse.Mensaje, "Error al obtener perfil", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
            catch (RpcException ex)
            {
                MessageBox.Show("Error al obtener perfil: " + ex.Status.Detail, "gRPC Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
