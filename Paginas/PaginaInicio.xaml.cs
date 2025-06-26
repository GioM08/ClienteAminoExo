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

            await CargarPublicaciones();
        }


        private async Task CargarPublicaciones()
        {
            WrapPanelPublicaciones.Children.Clear();

            string terminoBusqueda = TextBoxBusqueda?.Text?.Trim();
            List<PublicacionDTO> publicaciones;

            if (!string.IsNullOrEmpty(terminoBusqueda))
            {
                var resultadoBusqueda = await _publicacionService.BuscarPublicacionesAsync(query: terminoBusqueda, pagina: 1, limite: 50);
                publicaciones = resultadoBusqueda?.resultados ?? new List<PublicacionDTO>();
            }
            else
            {
                publicaciones = await _publicacionService.ObtenerPublicacionesAsync() ?? new List<PublicacionDTO>();
            }

            string tipoFiltro = "Todos";
            if (ComboBoxFiltro.SelectedItem is ComboBoxItem filtroItem && filtroItem.Tag != null)
                tipoFiltro = filtroItem.Tag.ToString();

            foreach (var pub in publicaciones)
            {
                var recurso = pub.recurso;

                if (tipoFiltro != "Todos" && recurso?.tipo != tipoFiltro)
                    continue;

                var reacciones = await _reaccionService.ObtenerReaccionesPorPublicacionAsync(pub.identificador);
                var comentarios = await _comentarioService.ObtenerConteoPorPublicacionAsync(pub.identificador);

                var conteo = reacciones
                    .GroupBy(r => r.tipo)
                    .ToDictionary(g => g.Key, g => g.Count());

                int likes = conteo.ContainsKey("like") ? conteo["like"] : 0;
                int dislikes = conteo.ContainsKey("dislike") ? conteo["dislike"] : 0;
                int loves = conteo.ContainsKey("love") ? conteo["love"] : 0;

                var reaccionUsuario = await _reaccionService.ObtenerReaccionDelUsuarioAsync(pub.identificador, SesionActual.UsuarioId);

                var tarjeta = CrearTarjetaPublicacion(pub, recurso, likes, dislikes, loves, comentarios, reaccionUsuario?.tipo);
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

        private UIElement CrearTarjetaPublicacion(PublicacionDTO pub, RecursoDTO recurso, int likes, int dislikes, int loves, int comentarios, string reaccionUsuario)
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
                    var originalUri = new Uri(recurso.url);
                    var urlAjustada = new UriBuilder(originalUri) { Host = "localhost" }.Uri.ToString();
                    recurso.url = urlAjustada;
                }

                if (!string.IsNullOrEmpty(recurso.url))
                {
                    try
                    {
                        if (recurso.tipo == "Foto")
                        {
                            stack.Children.Add(new Image
                            {
                                Height = 150,
                                Stretch = Stretch.UniformToFill,
                                Margin = new Thickness(0, 0, 0, 10),
                                Source = new BitmapImage(new Uri(recurso.url, UriKind.Absolute))
                            });
                        }
                        else if (recurso.tipo == "Video")
                        {
                            var video = new MediaElement
                            {
                                Height = 150,
                                LoadedBehavior = MediaState.Manual,
                                UnloadedBehavior = MediaState.Stop,
                                Margin = new Thickness(0, 0, 0, 10),
                                Source = new Uri(recurso.url, UriKind.Absolute),
                                Volume = 0.0
                            };

                            video.Loaded += (s, e) =>
                            {
                                try
                                {
                                    video.Play();
                                    Task.Delay(500).ContinueWith(_ =>
                                    {
                                        video.Dispatcher.Invoke(() => video.Pause());
                                    });
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Error al reproducir vista previa: {ex.Message}");
                                }
                            };

                            stack.Children.Add(video);
                        }
                        else if (recurso.tipo == "Audio")
                        {
                            stack.Children.Add(new MediaElement
                            {
                                Height = 30,
                                LoadedBehavior = MediaState.Manual,
                                UnloadedBehavior = MediaState.Stop,
                                Margin = new Thickness(0, 0, 0, 10),
                                Source = new Uri(recurso.url, UriKind.Absolute)
                            });
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
            interacciones.Children.Add(new TextBlock { Text = $"👎 {dislikes}", Foreground = Brushes.White, Margin = new Thickness(0, 0, 10, 0) });
            interacciones.Children.Add(new TextBlock { Text = $"😍 {loves}", Foreground = Brushes.White, Margin = new Thickness(0, 0, 10, 0) });
            interacciones.Children.Add(new TextBlock { Text = $"💬 {comentarios}", Foreground = Brushes.White });
            stack.Children.Add(interacciones);

            var panelReaccion = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 5, 0, 0) };

            void AgregarBotonEmoji(string emoji, string tipo, string tooltip)
            {
                var boton = new Button
                {
                    Content = emoji,
                    ToolTip = tooltip,
                    Width = 40,
                    Height = 40,
                    Margin = new Thickness(5),
                    FontSize = 16,
                    Background = tipo == reaccionUsuario ? Brushes.DarkCyan : Brushes.Gray,
                    Foreground = Brushes.White,
                    BorderBrush = Brushes.Transparent,
                    Tag = tipo
                };

                boton.Click += async (s, e) =>
                {
                    if (tipo == reaccionUsuario)
                    {
                        MessageBox.Show("Ya tienes esta reacción.");
                        return;
                    }

                    bool exito;

                    var reaccionActual = await _reaccionService.ObtenerReaccionDelUsuarioAsync(pub.identificador, SesionActual.UsuarioId);

                    if (reaccionActual != null)
                    {
                        reaccionActual.tipo = tipo;
                        reaccionActual.fecha = DateTime.UtcNow;

                        exito = await _reaccionService.ActualizarReaccionAsync(reaccionActual);

                        if (!exito)
                            MessageBox.Show("No se pudo actualizar la reacción, espere a que el servidor responsa.");
                    }
                    else
                    {
                        var nuevaReaccion = new ReaccionDTO
                        {
                            tipo = tipo,
                            publicacionId = pub.identificador,
                            usuarioId = SesionActual.UsuarioId,
                            nombreUsuario = SesionActual.nombreUsuario,
                            fecha = DateTime.UtcNow
                        };

                        exito = await _reaccionService.CrearReaccionAsync(nuevaReaccion);

                        if (!exito)
                            MessageBox.Show("No se pudo crear la reacción, espere a que el servidor responda.");
                    }

                    if (exito)
                        await CargarPublicaciones();
                };

                panelReaccion.Children.Add(boton);
            }

            AgregarBotonEmoji("👍", "like", "Dar like");
            AgregarBotonEmoji("👎", "dislike", "Dar dislike");
            AgregarBotonEmoji("😍", "love", "Enviar amor");

            stack.Children.Add(panelReaccion);

            if (SesionActual.Rol == "Moderador" || pub.usuarioId == SesionActual.UsuarioId)
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
                    var resultado = MessageBox.Show(
                        "¿Estás seguro de que deseas eliminar esta publicación?",
                        "Confirmar eliminación",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning
                    );

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
                            if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                            {
                                MessageBox.Show("No se pudo establecer conexión. Verifica tu conexión a Internet.", "Sin conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                            else
                            {
                                MessageBox.Show("Error al intentar conectar con el servidor, contacte con el soporte o espere que se restablezca", "Error del servidor", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
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


        private async void ButtonBuscar_Click(object sender, RoutedEventArgs e)
        {
            await CargarPublicaciones();
        }


    }
}