using ClienteAminoExo.Servicios.REST;
using ClienteAminoExo.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Lógica de interacción para PaginaInicio.xaml
    /// </summary>
    public partial class PaginaInicio : Page
    {

        private readonly RecursoRestService _recursoService = new(SesionActual.Token);
        private readonly PublicacionRestService _publicacionService = new(SesionActual.Token);

        public PaginaInicio()
        {
            


            InitializeComponent();
            CargarPublicaciones();
        }

        private async void CargarPublicaciones()
        {
            var publicaciones = await _publicacionService.ObtenerPublicacionesAsync();

            foreach (var pub in publicaciones)
            {
                RecursoDTO recurso = null;

                if (pub.recursoId > 0)
                {
                    recurso = await _recursoService.ObtenerRecursoPorIdAsync(pub.recursoId);
                }

                var tarjeta = CrearTarjetaPublicacion(pub, recurso);
                WrapPanelPublicaciones.Children.Add(tarjeta);
            }
        }

        private UIElement CrearTarjetaPublicacion(PublicacionDTO pub, RecursoDTO recurso)
        {
            var border = new Border
            {
                Background = (Brush)FindResource("ColorBarraLateral"),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(10),
                Width = 250
            };

            var stack = new StackPanel();

            // Mostrar recurso multimedia si existe
            if (recurso != null)
            {
                // Verificar si la URL es válida
                if (!string.IsNullOrEmpty(recurso.url) && !Uri.IsWellFormedUriString(recurso.url, UriKind.Absolute))
                {
                    recurso.url = $"http://localhost:3000/{recurso.url.TrimStart('/')}";
                }

                if (!string.IsNullOrEmpty(recurso.url))
                {
                    try
                    {
                        MessageBox.Show($"Tipo: {recurso?.tipo}, URL: {recurso?.url ?? "nulo"}");

                        if (recurso.tipo == "Foto")
                        {
                            var imagen = new Image
                            {
                                Height = 150,
                                Stretch = Stretch.UniformToFill,
                                Margin = new Thickness(0, 0, 0, 10),
                                Source = new BitmapImage(new Uri(recurso.url, UriKind.Absolute))
                            };
                            stack.Children.Add(imagen);
                        }
                        else if (recurso.tipo == "Video")
                        {
                            var video = new MediaElement
                            {
                                Height = 150,
                                LoadedBehavior = MediaState.Manual,
                                UnloadedBehavior = MediaState.Stop,
                                Margin = new Thickness(0, 0, 0, 10),
                                Source = new Uri(recurso.url, UriKind.Absolute)
                            };
                            video.Play(); // opcional
                            stack.Children.Add(video);
                        }
                        else if (recurso.tipo == "Audio")
                        {
                            var audio = new MediaElement
                            {
                                Height = 30,
                                LoadedBehavior = MediaState.Manual,
                                UnloadedBehavior = MediaState.Stop,
                                Margin = new Thickness(0, 0, 0, 10),
                                Source = new Uri(recurso.url, UriKind.Absolute)
                            };
                            audio.Play(); // opcional
                            stack.Children.Add(audio);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"❌ Error al cargar recurso multimedia: {ex.Message}");
                    }
                }
            }

            // Título
            stack.Children.Add(new TextBlock
            {
                Text = pub.titulo,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            });

            // Contenido
            stack.Children.Add(new TextBlock
            {
                Text = pub.contenido,
                FontSize = 12,
                Foreground = Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5)
            });

            // Reacciones simuladas
            var reacciones = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 5, 0, 0)
            };

            reacciones.Children.Add(new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/Recursos/like.png")),
                Width = 16,
                Margin = new Thickness(0, 0, 5, 0)
            });

            reacciones.Children.Add(new TextBlock
            {
                Text = "0", // simulado
                Foreground = Brushes.White
            });

            stack.Children.Add(reacciones);
            border.Child = stack;
            return border;
        }
    }
}