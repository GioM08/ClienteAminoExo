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

namespace MusicClient
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
        private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
        {
            // Más adelante: aquí irá la lógica para guardar al usuario en base de datos

            MessageBox.Show("Cuenta creada (simulado)", "Registro", MessageBoxButton.OK, MessageBoxImage.Information);

            // Redirigir al login o abrir MainWindow directamente
            var ventanaLogin = new VentanaLogin();
            ventanaLogin.Show();
            this.Close();
        }

        private void IrVentanaLogin_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var ventanaLogin = new VentanaLogin();
            ventanaLogin.Show();
            this.Close();
        }
    }
}
