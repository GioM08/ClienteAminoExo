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
using ClienteAminoExo;

namespace MusicClient
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

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            
            var main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void IrARegistro_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var registro = new VentanaRegistroUsuario();
            registro.Show();
            this.Close();
        }
    }
}
