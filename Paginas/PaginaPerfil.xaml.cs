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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClienteAminoExo.Paginas
{
    /// <summary>
    /// Lógica de interacción para PaginaPerfil.xaml
    /// </summary>
    public partial class PaginaPerfil : Page
    {

        private UsuarioRestService.Usuario _usuarioActual;
        public PaginaPerfil()
        {
            InitializeComponent();
            CargarPerfil();
        }


        private async void CargarPerfil()
        {
            try
            {
                var servicio = new UsuarioRestService(SesionActual.Token);
                var perfil = await servicio.ObtenerPerfilAsync();
                _usuarioActual = perfil.usuario;
                SesionActual.UsuarioId = _usuarioActual.usuarioId;

                txtNombre.Text = _usuarioActual.nombre;
                txtApellidos.Text = _usuarioActual.apellidos;
                txtCorreo.Text = _usuarioActual.correo;
                TbUsuario.Text = _usuarioActual.nombreUsuario;
                txtNombreUsuario.Text = _usuarioActual.nombreUsuario;
                TbRol.Text = "Rol: " + _usuarioActual.rol;
            }
            catch (System.Net.Http.HttpRequestException ex) when (ex.Message.Contains("401"))
            {
                MessageBox.Show("Tu sesión ha expirado. Por favor, vuelve a iniciar sesión.", "Sesión caducada", MessageBoxButton.OK, MessageBoxImage.Warning);

                SesionActual.CerrarSesion();

                var login = new VentanaLogin();
                login.Show();

                Window.GetWindow(this)?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el perfil: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text) ||
                string.IsNullOrWhiteSpace(txtApellidos.Text) ||
                string.IsNullOrWhiteSpace(txtCorreo.Text) ||
                string.IsNullOrWhiteSpace(txtNombreUsuario.Text))
            {
                MessageBox.Show("Por favor, completa todos los campos antes de guardar.", "Campos incompletos", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string errorNombre = Validaciones.ValidarNombre(txtNombre.Text, "Nombre");
            if (errorNombre != null)
            {
                MessageBox.Show(errorNombre, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string errorApellidos = Validaciones.ValidarNombre(txtApellidos.Text, "Apellidos");
            if (errorApellidos != null)
            {
                MessageBox.Show(errorApellidos, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string errorCorreo = Validaciones.ValidarCorreo(txtCorreo.Text);
            if (errorCorreo != null)
            {
                MessageBox.Show(errorCorreo, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string errorUsuario = Validaciones.ValidarNombreDeUsuario(txtNombreUsuario.Text);
            if (errorUsuario != null)
            {
                MessageBox.Show(errorUsuario, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var servicio = new UsuarioRestService(SesionActual.Token);

                var response = await servicio.ActualizarUsuarioAsync(
                    _usuarioActual.usuarioId,
                    txtNombre.Text,
                    txtApellidos.Text,
                    txtCorreo.Text,
                    txtNombreUsuario.Text
                );

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Perfil actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Puedes recargar el perfil si lo deseas
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("Error al actualizar perfil: " + error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message, "Error de red", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void BtnAbrirCambiarContrasena_Click(object sender, RoutedEventArgs e)
        {
            var ventana = new VentanaCaambiarContrasena(); // ya usará el id global
            ventana.ShowDialog();
        }

        private async void BtnEliminarCuenta_Click(object sender, RoutedEventArgs e)
        {
            var confirmacion = MessageBox.Show(
                "¿Estás seguro de que deseas eliminar tu cuenta? Esta acción eliminará todos tus datos y no se puede deshacer.",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion != MessageBoxResult.Yes)
                return;

            try
            {
                var servicio = new UsuarioRestService(SesionActual.Token);
                var response = await servicio.EliminarUsuarioAsync(_usuarioActual.usuarioId);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Tu cuenta ha sido eliminada correctamente.", "Cuenta eliminada", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Limpiar sesión
                    SesionActual.CerrarSesion();

                    // Redirigir al login
                    var ventanaLogin = new VentanaLogin();
                    ventanaLogin.Show();

                    // Cerrar ventana actual si estás dentro de un Frame
                    Window.GetWindow(this)?.Close();
                }
                else
                {
                    string error = await response.Content.ReadAsStringAsync();
                    MessageBox.Show("No se pudo eliminar la cuenta:\n" + error, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al intentar eliminar la cuenta:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var confirmacion = MessageBox.Show(
                "¿Deseas cerrar sesión?",
                "Cerrar sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (confirmacion == MessageBoxResult.Yes)
            {
                SesionActual.CerrarSesion();

                var ventanaLogin = new VentanaLogin();
                ventanaLogin.Show();

                Window.GetWindow(this)?.Close();
            }
        }

    }
}
