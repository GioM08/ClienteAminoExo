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

        public VentanaDetallePublicacion(PublicacionDTO publicacion)
        {
            InitializeComponent();
            _publicacion = publicacion;
            CargarPublicacion();
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
                    MessageBox.Show($"Error al cargar recurso: {ex.Message}");
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
            var comentarios = await _comentarioService.ObtenerComentariosPorPublicacionAsync(_publicacion.identificador
            );
            ListaComentarios.ItemsSource = comentarios;
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
                MessageBox.Show("Comentario enviado exitosamente.");
                await CargarComentarios();
            }
            else
            {
                MessageBox.Show("Error al enviar comentario.");
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
                MessageBox.Show(ok ? "¡Reacción creada exitosamente!" : "❌ Error al crear reacción.");
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
                    MessageBox.Show($"Error al eliminar la publicación: {ex.Message}");
                }
            }
        }


        private void BtnLike_Click(object sender, RoutedEventArgs e) => Reaccionar("like");
        private void BtnDislike_Click(object sender, RoutedEventArgs e) => Reaccionar("dislike");
        private void BtnEmoji_Click(object sender, RoutedEventArgs e) => Reaccionar("emoji");
    }
}
