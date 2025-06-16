using ClienteAminoExo.Servicios.REST;
using ClienteAminoExo.Utils;
using Reaccion;
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
        private readonly ReaccionRestService _reaccionService = new(SesionActual.Token);
        private readonly ComentarioRestService _comentarioService = new(SesionActual.Token);



        public PaginaInicio()
        {
            InitializeComponent();

            this.Loaded += PaginaInicio_Loaded;

        }

        private async void PaginaInicio_Loaded(object sender, RoutedEventArgs e)
        {
            if (SesionActual.Rol.Equals("Moderador"))
            {
                ComboBoxUsuarios.Visibility = Visibility.Visible;
                CargarUsuarios();
            }

            await CargarPublicaciones();
        }


        private async Task CargarPublicaciones()
        {
            WrapPanelPublicaciones.Children.Clear();

            var publicaciones = await _publicacionService.ObtenerPublicacionesAsync();

            if (publicaciones == null)
            {
                MessageBox.Show("⚠️ No se pudieron obtener publicaciones (es null)");
                return;
            }

            string tipoFiltro = "Todos";
            if (ComboBoxFiltro.SelectedItem is ComboBoxItem filtroItem && filtroItem.Tag != null)
                tipoFiltro = filtroItem.Tag.ToString();

            string usuarioFiltro = "Todos";
            if (ComboBoxUsuarios.SelectedItem is ComboBoxItem usuarioItem && usuarioItem.Tag != null)
                usuarioFiltro = usuarioItem.Tag.ToString();


            foreach (var pub in publicaciones)
            {
                var recurso = pub.recurso;

                
                if (tipoFiltro != "Todos" && recurso?.tipo != tipoFiltro)
                    continue;

                if (SesionActual.Rol.Equals($"Moderador") && usuarioFiltro != "Todos" && pub.usuarioId.ToString() != usuarioFiltro)
                    continue;

                var likes = await _reaccionService.ObtenerConteoPorPublicacionAsync(pub.identificador);
                var comentarios = await _comentarioService.ObtenerConteoPorPublicacionAsync(pub.identificador);

                var tarjeta = CrearTarjetaPublicacion(pub, recurso, likes, comentarios);
                WrapPanelPublicaciones.Children.Add(tarjeta);
            }

            if (WrapPanelPublicaciones.Children.Count == 0)
            {
                WrapPanelPublicaciones.Children.Add(new TextBlock
                {
                    Text = "No hay publicaciones que coincidan con el filtro.",
                    Foreground = Brushes.White,
                    FontSize = 16,
                    Margin = new Thickness(10)
                });
            }
        }


        private UIElement CrearTarjetaPublicacion(PublicacionDTO pub, RecursoDTO recurso, int likes, int comentarios)
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

            if (recurso != null)
            {
                if (!string.IsNullOrEmpty(recurso.url) && !recurso.url.StartsWith("http"))
                {
                    recurso.url = $"{BackendConfig.BackendBaseUrl}/{recurso.url.TrimStart('/')}";
                }

                if (!string.IsNullOrEmpty(recurso.url))
                {
                    Uri originalUri = new Uri(recurso.url);

                    var urlAjustada = new UriBuilder(originalUri)
                    {
                        Host = "localhost"
                    }.Uri.ToString();

                    recurso.url = urlAjustada;
                }



                if (!string.IsNullOrEmpty(recurso.url))
                {
                    try
                    {
                        

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
                            ; 
                            stack.Children.Add(audio);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al cargar recurso multimedia: {ex.Message}");
                    }
                }
            }

            stack.Children.Add(new TextBlock
            {
                Text = pub.titulo,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White
            });

            stack.Children.Add(new TextBlock
            {
                Text = pub.contenido,
                FontSize = 12,
                Foreground = Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 5)
            });

            var interacciones = new StackPanel { Orientation = Orientation.Horizontal };
            interacciones.Children.Add(new TextBlock { Text = $"👍 {likes}", Foreground = Brushes.White, Margin = new Thickness(5, 0, 10, 0) });
            interacciones.Children.Add(new TextBlock { Text = $"💬 {comentarios}", Foreground = Brushes.White });
            stack.Children.Add(interacciones);


            if (SesionActual.Rol.Equals("Moderador"))
            {
                var btnEliminar = new Button
                {
                    Content = "Eliminar",
                    Background = Brushes.Red,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 10, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Right
                };

                btnEliminar.Click += async (s, e) =>
                {
                    var resultado = MessageBox.Show("¿Estás seguro de que deseas eliminar esta publicación?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (resultado == MessageBoxResult.Yes)
                    {
                        try
                        {
                            await _publicacionService.EliminarPublicacionAsync(pub.identificador);
                            MessageBox.Show("Publicación eliminada correctamente.");
                            await CargarPublicaciones(); 
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error al eliminar publicación: {ex.Message}");
                        }
                    }
                };

                stack.Children.Add(btnEliminar);
            }

            border.Child = stack;

            border.MouseLeftButtonUp += (s, e) =>
            {
                var ventana = new VentanaDetallePublicacion(pub);
                ventana.ShowDialog();
            };

            return border;


        }

        private async void ComboBoxFiltro_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (WrapPanelPublicaciones != null)
                await CargarPublicaciones(); 
        }


        private async void CargarUsuarios()
        {
            var usuarios = await new UsuarioRestService(SesionActual.Token).ObtenerUsuariosAsync();
            ComboBoxUsuarios.Items.Clear();
            ComboBoxUsuarios.Items.Add(new ComboBoxItem { Content = "Todos", Tag = "Todos", IsSelected = true });

            foreach (var user in usuarios)
            {
                ComboBoxUsuarios.Items.Add(new ComboBoxItem { Content = user.nombreUsuario, Tag = user.usuarioId });
            }
        }


    }
}