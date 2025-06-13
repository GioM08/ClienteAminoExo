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

namespace ClienteAminoExo.Paginas
{
    /// <summary>
    /// Lógica de interacción para VentanaDetallePublicacion.xaml
    /// </summary>
    public partial class VentanaDetallePublicacion : Window
    {
        private readonly int _publicacionId;

        private readonly PublicacionRestService _publicacionService = new(SesionActual.Token);
        private readonly ComentarioRestService _comentarioService = new(SesionActual.Token);
        private readonly ReaccionRestService _reaccionService = new(SesionActual.Token);
        private readonly RecursoRestService _recursoService = new(SesionActual.Token);

        public VentanaDetallePublicacion(int publicacionId)
        {
            InitializeComponent();
            _publicacionId = publicacionId;
            CargarPublicacion();
        }

        private async void CargarPublicacion()
        {
            var publicaciones = await _publicacionService.ObtenerPublicacionesAsync();
            var pub = publicaciones.FirstOrDefault(p => p.identificador == _publicacionId);
            if (pub == null)
            {
                MessageBox.Show("No se encontró la publicación.");
                Close();
                return;
            }

            TxtTitulo.Text = pub.titulo;
            TxtContenido.Text = pub.contenido;

            if (pub.recursoId > 0)
            {
                var recurso = await _recursoService.ObtenerRecursoPorIdAsync(pub.recursoId);
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
                    Stretch = System.Windows.Media.Stretch.UniformToFill,
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
            var comentarios = await _comentarioService.ObtenerComentariosPorPublicacionAsync(_publicacionId);
            ListaComentarios.ItemsSource = comentarios;
        }

        private async void BtnEnviarComentario_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtComentario.Text)) return;

            var comentario = new ComentarioDTO
            {
                contenido = TxtComentario.Text,
                publicacionId = _publicacionId,
                usuarioId = SesionActual.UsuarioId,
                fecha = DateTime.Now
            };

            var exito = await _comentarioService.CrearComentarioAsync(comentario);
            if (exito)
            {
                TxtComentario.Clear();
                await CargarComentarios();
            }
            else
            {
                MessageBox.Show("Error al enviar comentario.");
            }
        }

        private async void Reaccionar(string tipo)
        {
            MessageBox.Show($"Intentando reaccionar con: {tipo}");

            var existente = await _reaccionService.ObtenerReaccionDelUsuarioAsync(_publicacionId, SesionActual.UsuarioId);
            MessageBox.Show($"Reacción existente: {(existente != null ? existente.tipo : "ninguna")}");

            if (existente == null)
            {
                MessageBox.Show("Creando nueva reacción");

                var nueva = new ReaccionDTO
                {
                    tipo = tipo,
                    publicacionId = _publicacionId,
                    usuarioId = SesionActual.UsuarioId,
                    fecha = DateTime.Now
                };

                var ok = await _reaccionService.CrearReaccionAsync(nueva);
                if (!ok) MessageBox.Show("❌ Error al crear reacción.");
            }
            else if (existente.tipo != tipo)
            {
                MessageBox.Show($"Actualizando reacción de {existente.tipo} a {tipo}");

                existente.tipo = tipo;
                var ok = await _reaccionService.ActualizarReaccionAsync(existente);
                if (!ok) MessageBox.Show("❌ Error al actualizar reacción.");
            }
            else
            {
                MessageBox.Show("Ya has reaccionado con ese tipo.");
            }
        }



        private void BtnLike_Click(object sender, RoutedEventArgs e) => Reaccionar("like");
        private void BtnDislike_Click(object sender, RoutedEventArgs e) => Reaccionar("dislike");
        private void BtnEmoji_Click(object sender, RoutedEventArgs e) => Reaccionar("emoji");
    }
}
