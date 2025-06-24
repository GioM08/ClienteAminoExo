using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ClienteAminoExo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rolUsuario;

        public MainWindow(string rol) // ← recibe el rol del usuario al abrir la ventana
        {
            InitializeComponent();
            rolUsuario = rol;

            if (rolUsuario == "Administrador")
            {
                BtnEstadisticas.Visibility = Visibility.Visible;
            }

            // Opcional: carga la página de inicio al abrir
            // MainFrame.Navigate(new Paginas.PaginaInicio());
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag)
            {
                switch (tag)
                {
                    case "Inicio":
                        MainFrame.Navigate(new Paginas.PaginaInicio());
                        break;
                    case "Perfil":
                        MainFrame.Navigate(new Paginas.PaginaPerfil());
                        break;
                    case "Publicar":
                        MainFrame.Navigate(new Paginas.PaginaPublicar());
                        break;
                    case "Notificaciones":
                        MainFrame.Navigate(new Paginas.PaginaNotificaciones());
                        break;
                    case "Estadisticas":
                        MainFrame.Navigate(new Paginas.PaginaEstadisticas());
                        break;
                }
            }
        }
    }

}