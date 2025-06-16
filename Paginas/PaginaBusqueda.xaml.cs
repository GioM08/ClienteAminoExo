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
using ClienteAminoExo.Servicios.REST;
using ClienteAminoExo.Utils;


namespace ClienteAminoExo.Paginas
{
    /// <summary>
    /// Lógica de interacción para PaginaBusqueda.xaml
    /// </summary>
    public partial class PaginaBusqueda : Page
    {
        private readonly PublicacionRestService _publicacionRestService;

        public PaginaBusqueda()
        {
            InitializeComponent();

            _publicacionRestService = new PublicacionRestService(SesionActual.Token);
        }

        private async void ButtonBuscar_Click(object sender, RoutedEventArgs e)
        {
            await BuscarYMostrarPublicacionesAsync();
        }

        private async Task BuscarYMostrarPublicacionesAsync()
        {
            string termino = TextBoxBusqueda.Text.Trim();

            var resultadoBusqueda = await _publicacionRestService.BuscarPublicacionesAsync(query: termino, pagina: 1, limite: 20);

            WrapPanelResultados.Children.Clear();

            if (resultadoBusqueda == null || resultadoBusqueda.resultados == null || resultadoBusqueda.resultados.Count == 0)
            {
                var textoSinResultados = new TextBlock
                {
                    Text = "No se encontraron publicaciones.",
                    Foreground = Brushes.White,
                    FontSize = 16,
                    Margin = new Thickness(10)
                };
                WrapPanelResultados.Children.Add(textoSinResultados);
                return;
            }

            foreach (var publicacion in resultadoBusqueda.resultados)
            {
                var tarjeta = CrearTarjetaPublicacion(publicacion);
                WrapPanelResultados.Children.Add(tarjeta);
            }
        }

        private UIElement CrearTarjetaPublicacion(PublicacionDTO publicacion)
        {
            var border = new Border
            {
                Background = (Brush)Application.Current.Resources["ColorBarraLateral"],
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(10),
                Width = 250,
                Cursor = Cursors.Hand 
            };

            var stack = new StackPanel();

            var imgSource = "/Recursos/user.png"; // default

            if (publicacion.recurso != null && !string.IsNullOrEmpty(publicacion.recurso.url))
            {
                imgSource = publicacion.recurso.url;

                if (!imgSource.StartsWith("http"))
                {
                    imgSource = $"{BackendConfig.BackendBaseUrl}/{imgSource.TrimStart('/')}";
                }
            }

            var imagen = new Image
            {
                Source = new BitmapImage(new Uri(imgSource, UriKind.RelativeOrAbsolute)),
                Height = 150,
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(0, 0, 0, 10)
            };

            stack.Children.Add(imagen);

            stack.Children.Add(new TextBlock
            {
                Text = publicacion.titulo,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"Usuario {publicacion.usuarioId}",
                FontSize = 14,
                Foreground = Brushes.LightGray
            });

            stack.Children.Add(new TextBlock
            {
                Text = publicacion.contenido,
                FontSize = 12,
                Foreground = Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5)
            });

            border.MouseLeftButtonUp += (s, e) =>
            {
                var ventana = new VentanaDetallePublicacion(publicacion);
                ventana.ShowDialog();
            };

            border.Child = stack;

            return border;
        }

    }
}
