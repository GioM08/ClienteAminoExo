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
        public MainWindow()
        {
            InitializeComponent();
            //MainFrame.Navigate(new Paginas.PaginaInicio()); Si se quiere mandar de una al inicio cuando se entra
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
                }
            }
        }
    }
}