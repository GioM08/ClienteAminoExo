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

namespace ClienteAminoExo.Paginas
{
    /// <summary>
    /// Lógica de interacción para VentanaCaambiarContrasena.xaml
    /// </summary>
    public partial class VentanaCaambiarContrasena : Window
    {
        public VentanaCaambiarContrasena()
        {
            InitializeComponent();
        }

        private async void BtnActualizar_Click(object sender, RoutedEventArgs e)
        {
            var nueva = txtNuevaContrasena.Password;

            string errorValidacion = Validaciones.ValidarPassword(nueva);
            if (errorValidacion != null)
            {
                MessageBox.Show(errorValidacion);
                return;
            }

            try
            {
                var servicio = new UsuarioRestService(SesionActual.Token);
                var respuesta = await servicio.CambiarContrasenaAsync(SesionActual.UsuarioId, nueva);

                if (respuesta.IsSuccessStatusCode)
                {
                    MessageBox.Show("Contraseña actualizada correctamente.");
                    this.Close();
                }
                else
                {
                    string error = await respuesta.Content.ReadAsStringAsync();
                    MessageBox.Show("Error: " + error);
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
                    MessageBox.Show("Error al enviar comentario, contacte con el soporte o espere que se restablezca", "Error del servidor", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnCancelar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}

