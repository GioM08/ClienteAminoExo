﻿using ClienteAminoExo;
using ClienteAminoExo.Servicios.gRPC;
using ClienteAminoExo.Servicios.REST;
using ClienteAminoExo.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ClienteAminoExo
{
    /// <summary>
    /// Lógica de interacción para VentanaLogin.xaml
    /// </summary>
    public partial class VentanaLogin : Window
    {
        public VentanaLogin()
        {
            InitializeComponent();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string correo = TxtUsuario.Text;
            string contrasena = TxtPassword.Password;

            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                MessageBox.Show("Completa todos los campos", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var servicio = new UsuarioGrpcService();
                var resultado = await servicio.LoginAsync(correo, contrasena);

                if (resultado.Exito)
                {
                    MessageBox.Show($"Bienvenido, {resultado.NombreUsuario}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    string token = resultado.Token;
                    SesionActual.Token = resultado.Token;

                    var perfilService = new UsuarioRestService(resultado.Token);
                    var perfil = await perfilService.ObtenerPerfilAsync();



                    SesionActual.UsuarioId = perfil.usuario.usuarioId;
                    SesionActual.Rol = perfil.usuario.rol;
                    SesionActual.nombreUsuario = perfil.usuario.nombreUsuario;

                    perfilService.ActualizarHeaders();


                    var main = new MainWindow(perfil.usuario.rol);
                    main.Show();
                    this.Close();

                }
                else
                {
                    MessageBox.Show(resultado.Mensaje, "Inicio fallido", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    MessageBox.Show("No se pudo establecer conexión. Verifica tu conexión a Internet.", "Sin conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    MessageBox.Show($"Error al conectar con el servidor, contacte con el soporte o espere que se restablezca", "Error del servidor", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private void IrARegistro_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var registro = new VentanaRegistroUsuario();
            registro.Show();
            this.Close();
        }
    }
}
