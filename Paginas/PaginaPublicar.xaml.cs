using ClienteAminoExo.Servicios.gRPC;
using ClienteAminoExo.Servicios.REST;
using ClienteAminoExo.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using static ClienteAminoExo.Servicios.REST.UsuarioRestService;

namespace ClienteAminoExo.Paginas
{
    /// <summary>
    /// Lógica de interacción para PaginaPublicar.xaml
    /// </summary>
    public partial class PaginaPublicar : Page
    {

        private readonly RecursoGrpcService _recursoGrpc = new();
        private readonly PublicacionRestService _publicacionRest = new(SesionActual.Token);
        private string rutaArchivoSeleccionado;
        int usuarioId = SesionActual.UsuarioId;
        private int recursoIdSubido = 0; 
        private UsuarioRestService.Usuario _usuarioActual;
        private string _rutaArchivoSeleccionado;
        private string _tipoRecurso;
        private int _formatoRecurso;
        private int _tamanoRecurso;

        public PaginaPublicar()
        {
            InitializeComponent();
            BtnSeleccionarArchivo.IsEnabled = false;
            BtnPublicar.IsEnabled = false;
            CargarPerfil();
        }

        private async void CargarPerfil()
        {
            try
            {
                var servicio = new UsuarioRestService(SesionActual.Token);
                var perfil = await servicio.ObtenerPerfilAsync();
                _usuarioActual = perfil.usuario;
                SesionActual.UsuarioId = _usuarioActual.usuarioId;
                usuarioId = _usuarioActual.usuarioId;

                BtnSeleccionarArchivo.IsEnabled = true;
                BtnPublicar.IsEnabled = true;


            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al obtener el perfil: " + ex.Message);
            }
        }


        private async void BtnSeleccionarArchivo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != true) return;

            _rutaArchivoSeleccionado = dlg.FileName;
            _tipoRecurso = (CmbTipoRecurso.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrEmpty(_tipoRecurso) || !new[] { "Foto", "Video", "Audio" }.Contains(_tipoRecurso))
            {
                MessageBox.Show("Selecciona un tipo válido: Foto, Video o Audio.");
                return;
            }
            _formatoRecurso = _formatoRecurso = ObtenerCodigoFormato(System.IO.Path.GetExtension(_rutaArchivoSeleccionado).Trim('.'));
            _tamanoRecurso = (int)new FileInfo(_rutaArchivoSeleccionado).Length;

            TxtArchivoSeleccionado.Text = $"Archivo: {System.IO.Path.GetFileName(_rutaArchivoSeleccionado)}";
        }


        private async void BtnPublicar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_rutaArchivoSeleccionado))
            {
                MessageBox.Show("Debes seleccionar un archivo primero.");
                return;
            }

            var publicacionService = new PublicacionRestService(SesionActual.Token);
            publicacionService.ActualizarHeaders();

            var nuevaPublicacion = new PublicacionDTO
            {
                titulo = TxtTitulo.Text, 
                contenido = TxtContenido.Text,
                usuarioId = SesionActual.UsuarioId,
                fechaCreacion = DateTime.Now
            };

            var publicacionCreada = await publicacionService.CrearPublicacionConRespuestaAsync(nuevaPublicacion);

            if (publicacionCreada == null || publicacionCreada.identificador <= 0)
            {
                MessageBox.Show("No se pudo crear la publicación");
                return;
            }

            int publicacionId = publicacionCreada.identificador;

            var recursoService = new RecursoGrpcService();

            var respuestaRecurso = await recursoService.CrearRecursoAsync(
                rutaArchivo: _rutaArchivoSeleccionado,
                tipo: _tipoRecurso,
                formato: _formatoRecurso,
                tamano: _tamanoRecurso,
                usuarioId: SesionActual.UsuarioId,
                publicacionId: publicacionId,
                jwt: SesionActual.Token,
                resolucion: 1080
            );

            MessageBox.Show("Respuesta al subir recurso: " + respuestaRecurso.Mensaje);

            if (!respuestaRecurso.Exito)
            {
                MessageBox.Show("Error al subir recurso: " + respuestaRecurso.Mensaje);

                if (!respuestaRecurso.Exito)
                {
                    MessageBox.Show("Error al subir recurso: " + respuestaRecurso.Mensaje);

                    try
                    {
                        await publicacionService.EliminarPublicacionAsync(publicacionId);
                        MessageBox.Show("Publicación eliminada correctamente.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar publicación: {ex.Message}");
                    }
                }

                MessageBox.Show("Publicación y recurso creados correctamente.");
            }

            MessageBox.Show("Publicación y recurso creados correctamente.");
        }


        private int ObtenerCodigoFormato(string extension)
        {
            switch (extension.ToLower())
            {
                case "jpg": case "jpeg": return 1;
                case "png": return 2;
                case "mp3": return 3;
                case "mp4": return 4;
                default: return 0; 
            }
        }

    }
}
