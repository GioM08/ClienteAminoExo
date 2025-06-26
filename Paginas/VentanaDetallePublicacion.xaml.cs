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
using ClienteAminoExo.Servicios.gRPC;
using Usuario;
using System.Collections.ObjectModel;


namespace ClienteAminoExo.Paginas
{
    /// <summary>
    /// Lógica de interacción para VentanaDetallePublicacion.xaml
    /// </summary>
    public partial class VentanaDetallePublicacion : Window
    {
        private readonly PublicacionDTO _publicacion;
        private readonly RecursoGrpcService _recursoGrpcService = new();



        private readonly PublicacionRestService _publicacionService = new(SesionActual.Token);
        private readonly ComentarioRestService _comentarioService = new(SesionActual.Token);
        private readonly ReaccionRestService _reaccionService = new(SesionActual.Token);
        private readonly RecursoRestService _recursoService = new(SesionActual.Token);
        private readonly UsuarioRestService _usuarioService = new(SesionActual.Token);
        private ObservableCollection<ComentarioDTO> _comentarios = new ObservableCollection<ComentarioDTO>();


        public VentanaDetallePublicacion(PublicacionDTO publicacion)
        {
            InitializeComponent();
            _publicacion = publicacion;
            CargarPublicacion();
            ListaComentarios.ItemsSource = _comentarios;

            this.Loaded += VentanaDetallePublicacion_Loaded;
        }

        private async void VentanaDetallePublicacion_Loaded(object sender, RoutedEventArgs e)
        {
            if (_publicacion.recurso == null || _publicacion.recurso.identificador == 0)
            {
                try
                {
                    _publicacion.recurso = await _recursoService.ObtenerRecursoPorIdAsync(_publicacion.recursoId);

                    if (_publicacion.recurso == null)
                    {
                        MessageBox.Show("No se pudo obtener información del recurso.");
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
                        MessageBox.Show($"Error al conectar con el servidor, contacte con el soporte o espere que se restablezca", "Error del servidor", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            if (SesionActual.Rol == "Moderador" || SesionActual.Rol == "Administrador")
            {
                BtnEliminarPublicacion.Visibility = Visibility.Visible;
            }
        }

        private async void CargarPublicacion()
        {
            TxtTitulo.Text = _publicacion.titulo;
            TxtContenido.Text = _publicacion.contenido;

            if (_publicacion.recurso != null)
            {
                CargarRecursoMultimedia(_publicacion.recurso);
            }
            else if (_publicacion.recursoId > 0)
            {
                var recurso = await _recursoService.ObtenerRecursoPorIdAsync(_publicacion.recursoId);
                CargarRecursoMultimedia(recurso);
            }

            await CargarComentarios();
        }


        private void CargarRecursoMultimedia(RecursoDTO recurso)
        {
            if (string.IsNullOrWhiteSpace(recurso?.url)) return;

            if (!Uri.IsWellFormedUriString(recurso.url, UriKind.Absolute))
                recurso.url = $"{BackendConfig.BackendBaseUrl}/{recurso.url.TrimStart('/')}";

            if (recurso.tipo == "Foto")
            {
                var imagen = new Image
                {
                    Height = 150,
                    Stretch = Stretch.Uniform,
                    Source = new BitmapImage(new Uri(recurso.url))
                };
                RecursoMultimedia.Content = imagen;
            }
            else if (recurso.tipo == "Video" || recurso.tipo == "Audio")
            {
                var media = new MediaElement
                {
                    Height = recurso.tipo == "Video" ? 150 : 30,
                    Source = new Uri(recurso.url),
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Stop
                };
                media.Play();
                RecursoMultimedia.Content = media;
            }
        }

        private async Task CargarComentarios()
        {
            var comentarios = await _comentarioService.ObtenerComentariosPorPublicacionAsync(_publicacion.identificador);

            _comentarios.Clear();

            foreach (var comentario in comentarios)
            {
                var usuario = await _usuarioService.ObtenerUsuarioPorIdAsync(comentario.usuarioId);
                comentario.nombreUsuario = usuario?.nombreUsuario ?? "Desconocido";
                _comentarios.Add(comentario); 
            }
        }



        private async void BtnEnviarComentario_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtComentario.Text)) return;

            var comentario = new ComentarioDTO
            {
                texto = TxtComentario.Text, 
                publicacionId = _publicacion.identificador,
                usuarioId = SesionActual.UsuarioId,
                
            };

            var exito = await _comentarioService.CrearComentarioAsync(comentario);
            if (exito)
            {
                TxtComentario.Clear();
                await CargarComentarios();

                MessageBox.Show("Comentario enviado exitosamente.");
                if (_comentarios.Count > 0)
                {
                    ListaComentarios.ScrollIntoView(_comentarios[_comentarios.Count - 1]);
                }
            }
            else
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

        private async void BtnActualizarComentario_Click(object sender, RoutedEventArgs e)
        {
            var boton = sender as Button;
            if (boton?.Tag is ComentarioDTO comentario)
            {
                var contenedor = VisualTreeHelper.GetParent(boton);
                TextBox? txt = null;

                while (contenedor != null && contenedor is not ContentPresenter)
                {
                    contenedor = VisualTreeHelper.GetParent(contenedor);
                }

                if (contenedor is ContentPresenter presenter)
                {
                    txt = FindVisualChild<TextBox>(presenter);
                }

                if (txt == null || string.IsNullOrWhiteSpace(txt.Text))
                {
                    MessageBox.Show("El texto del comentario no puede estar vacío.");
                    return;
                }

                if (comentario.usuarioId != SesionActual.UsuarioId)
                {
                    MessageBox.Show("Solo puedes actualizar tus propios comentarios.");
                    return;
                }

                var result = MessageBox.Show("¿Deseas actualizar este comentario?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes) return;

                var resultado = await _comentarioService.ActualizarComentarioAsync(comentario.comentarioId, txt.Text.Trim());

                if (resultado)
                {
                    MessageBox.Show("Comentario actualizado.");
                    await CargarComentarios();
                }
                else
                {
                    MessageBox.Show("No se pudo actualizar el comentario.");
                }
            }
        }


        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                    return typedChild;

                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }

        private async void BtnEliminarComentario_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button boton && boton.Tag is ComentarioDTO comentario)
            {
                if (comentario.usuarioId != SesionActual.UsuarioId)
                {
                    MessageBox.Show("Solo puedes eliminar tus propios comentarios.");
                    return;
                }

                var result = MessageBox.Show("¿Estás seguro de que deseas eliminar este comentario?", "Confirmar eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result != MessageBoxResult.Yes) return;

                var exito = await _comentarioService.EliminarComentarioAsync(comentario.comentarioId);

                if (exito)
                {
                    MessageBox.Show("Comentario eliminado correctamente.");
                    await CargarComentarios(); 
                }
                else
                {
                    MessageBox.Show("Error al eliminar el comentario.");
                }
            }
        }


        private async void Reaccionar(string tipo)
        {
            var tipoActual = await _reaccionService.ObtenerTipoReaccionAsync(SesionActual.UsuarioId, _publicacion.identificador);

            if (tipoActual == null)
            {
                var nueva = new ReaccionDTO
                {
                    tipo = tipo,
                    publicacionId = _publicacion.identificador,
                    usuarioId = SesionActual.UsuarioId,
                    nombreUsuario = SesionActual.nombreUsuario,
                };

                var ok = await _reaccionService.CrearReaccionAsync(nueva);
                MessageBox.Show(ok ? "¡Reacción creada exitosamente!" : "Error al crear reacción.");
            }
            else if (tipoActual == tipo)
            {
                MessageBox.Show("Ya has reaccionado con ese tipo.");
            }
            else
            {
                var reaccionId = await _reaccionService.ObtenerReaccionIdAsync(SesionActual.UsuarioId, _publicacion.identificador);

                if (reaccionId == null)
                {
                    MessageBox.Show("No se pudo obtener el ID de la reacción para actualizar.");
                    return;
                }

                var reaccionActualizada = new ReaccionDTO
                {
                    reaccionId = reaccionId.Value,
                    tipo = tipo
                };

                var ok = await _reaccionService.ActualizarReaccionAsync(reaccionActualizada);
                MessageBox.Show(ok ? "Reacción actualizada correctamente." : "❌ Error al actualizar la reacción.");
            }
        }



        private async void BtnDescargarRecurso_Click(object sender, RoutedEventArgs e)
        {
            var tipo = _publicacion.recurso?.tipo;
            var identificador = _publicacion.recurso?.identificador ?? _publicacion.recursoId;

            MessageBox.Show($"Intentando descargar:\nTipo: {tipo}\nIdentificador: {identificador}");

            if (string.IsNullOrWhiteSpace(tipo) || identificador <= 0)
            {
                MessageBox.Show("No hay recurso válido para descargar.");
                return;
            }

            (bool exito, byte[] archivo, string mensaje) = await _recursoGrpcService.DescargarRecursoAsync(tipo, identificador);


            if (!exito)
            {
                MessageBox.Show($"{mensaje}");
                return;
            }

            var extension = tipo == "Foto" ? "jpg" :
                tipo == "Audio" ? "mp3" :
                tipo == "Video" ? "mp4" : "bin";

            var dialogo = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"recurso_{identificador}.{extension}",
                Filter = $"{tipo} (*.{extension})|*.{extension}|Todos los archivos (*.*)|*.*"
            };

            if (dialogo.ShowDialog() == true)
            {
                try
                {
                    System.IO.File.WriteAllBytes(dialogo.FileName, archivo);
                    MessageBox.Show("Recurso descargado exitosamente.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al guardar el archivo: {ex.Message}");
                }
            }
        }

        private async void BtnEliminarPublicacion_Click(object sender, RoutedEventArgs e)
        {
            var confirmacion = MessageBox.Show("¿Estás seguro de que deseas eliminar esta publicación?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirmacion == MessageBoxResult.Yes)
            {
                try
                {
                    await _publicacionService.EliminarPublicacionAsync(_publicacion.identificador);

                    MessageBox.Show("Publicación eliminada exitosamente.");
                    this.Close();
                }
                catch (Exception ex)
                {
                    if (!System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                    {
                        MessageBox.Show("No se pudo establecer conexión. Verifica tu conexión a Internet.", "Sin conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"hubo un error al eliminar la publicación, contacte con el soporte o espere que se restablezca", "Error del servidor", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }


        private void BtnLike_Click(object sender, RoutedEventArgs e) => Reaccionar("like");
        private void BtnDislike_Click(object sender, RoutedEventArgs e) => Reaccionar("dislike");
        private void BtnEmoji_Click(object sender, RoutedEventArgs e) => Reaccionar("emoji");
    }
}
