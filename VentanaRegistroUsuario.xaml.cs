﻿using ClienteAminoExo.Servicios.REST;
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
    /// Lógica de interacción para VentanaRegistroUsuario.xaml
    /// </summary>
    public partial class VentanaRegistroUsuario : Window
    {
        public VentanaRegistroUsuario()
        {
            InitializeComponent();
        }
        private async void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string nombreUsuario = TxtUsuario.Text;
            string nombre = TxtNombre.Text;
            string apellidos = TxtApellidos.Text;
            string correo = TxtCorreo.Text;
            string contrasena = TxtContrasena.Password;
            string confirmar = TxtConfirmar.Password;
            string rol = "Fan";

            // Validación
            if (string.IsNullOrWhiteSpace(nombreUsuario) || string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(apellidos) || string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena) || string.IsNullOrWhiteSpace(confirmar))
            {
                MessageBox.Show("Completa todos los campos", "Campos vacíos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (contrasena != confirmar)
            {
                MessageBox.Show("Las contraseñas no coinciden", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var servicio = new UsuarioRestService();
                var respuesta = await servicio.CrearCuentaAsync(nombreUsuario, nombre, apellidos, correo, contrasena, rol);

                if (respuesta.IsSuccessStatusCode)
                {
                    MessageBox.Show("Cuenta creada exitosamente", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    var ventanaLogin = new VentanaLogin();
                    ventanaLogin.Show();
                    this.Close();
                }
                else
                {
                    string error = await respuesta.Content.ReadAsStringAsync();
                    MessageBox.Show($"Error al crear cuenta: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error de conexión: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void IrVentanaLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ventanaLogin = new VentanaLogin();
            ventanaLogin.Show();
            this.Close();
        }
    }
}
