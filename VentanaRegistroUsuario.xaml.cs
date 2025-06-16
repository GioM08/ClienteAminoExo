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
using System.Text.Json;

namespace ClienteAminoExo
{
    public partial class VentanaRegistroUsuario : Window
    {
        public VentanaRegistroUsuario()
        {
            InitializeComponent();
            PanelContrasenaRol.Visibility = Visibility.Collapsed;
        }

        private async void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            string nombreUsuario = TxtUsuario.Text;
            string nombre = TxtNombre.Text;
            string apellidos = TxtApellidos.Text;
            string correo = TxtCorreo.Text;
            string contrasena = TxtContrasena.Password;
            string confirmar = TxtConfirmar.Password;
            string rol = (ComboRol.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Fan";
            string claveRol = TxtContrasenaRol.Password;

            string error = Validaciones.ValidarNombre(nombre, "Nombre") ??
                           Validaciones.ValidarNombre(apellidos, "Apellidos") ??
                           Validaciones.ValidarNombreDeUsuario(nombreUsuario) ??
                           Validaciones.ValidarCorreo(correo) ??
                           Validaciones.ValidarPassword(contrasena);

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show(error, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (contrasena != confirmar)
            {
                MessageBox.Show("Las contraseñas no coinciden", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if ((rol == "Moderador") && claveRol != "conmoder")
            {
                MessageBox.Show("La contraseña de rol es incorrecta", "Acceso restringido", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if ((rol == "Administrador") && claveRol != "conadmin")
            {
                MessageBox.Show("La contraseña de rol es incorrecta", "Acceso restringido", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    string errorMensaje = await respuesta.Content.ReadAsStringAsync();
                    string mensaje = "Error desconocido";

                    try
                    {
                        using var doc = JsonDocument.Parse(errorMensaje);
                        mensaje = doc.RootElement.GetProperty("msg").GetString();
                    }
                    catch
                    {
                        mensaje = errorMensaje;
                    }

                    MessageBox.Show($"Error al crear cuenta: {mensaje}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void ComboRol_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PanelContrasenaRol == null)
                return;

            string rol = (ComboRol.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (rol == "Moderador" || rol == "Administrador")
            {
                PanelContrasenaRol.Visibility = Visibility.Visible;
            }
            else
            {
                PanelContrasenaRol.Visibility = Visibility.Collapsed;
            }
        }
    }
}
