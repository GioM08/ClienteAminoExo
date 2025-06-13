using ClienteAminoExo.Servicios.gRPC;
using ClienteAminoExo.Servicios.REST;
using ClienteAminoExo.Utils;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using static ClienteAminoExo.Servicios.REST.UsuarioRestService;

namespace ClienteAminoExo.Paginas
{
    /// <summary>
    /// Lógica de interacción para PaginaPublicar.xaml
    /// </summary>
    public partial class PaginaPublicar : Page
    {

        private readonly RecursoGrpcService _recursoGrpc = new();
        private readonly PublicacionRestService _publicacionRest = new();
        private string rutaArchivoSeleccionado;
        int usuarioId = SesionActual.UsuarioId;
        private int recursoIdSubido = 0; // se actualiza solo si se sube recurso


        public PaginaPublicar()
        {
            InitializeComponent();
        }

        private async void BtnSeleccionarArchivo_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            if (dlg.ShowDialog() != true)
            {
                TxtEstado.Text = "Selección cancelada.";
                return;
            }

            rutaArchivoSeleccionado = dlg.FileName;
            TxtArchivoSeleccionado.Text = $"Archivo: {System.IO.Path.GetFileName(rutaArchivoSeleccionado)}";

            string tipo = (CmbTipoRecurso.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrWhiteSpace(tipo))
            {
                TxtEstado.Text = "Selecciona el tipo de recurso antes de subir.";
                return;
            }

            string formato = System.IO.Path.GetExtension(rutaArchivoSeleccionado).Trim('.');
            long tamano = new FileInfo(rutaArchivoSeleccionado).Length;

            TxtEstado.Text = "Subiendo recurso al servidor...";

            var resultado = await _recursoGrpc.SubirRecursoAsync(
                rutaArchivoSeleccionado, tipo, formato, tamano, usuarioId
            );

            if (!resultado.exito)
            {
                TxtEstado.Text = $"Error al subir recurso: {resultado.mensaje}";
                recursoIdSubido = 0;
            }
            else
            {
                recursoIdSubido = resultado.identificador;
                TxtEstado.Text = $"Recurso subido correctamente (ID: {recursoIdSubido})";
            }
        }


        private async void BtnPublicar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitulo.Text) || string.IsNullOrWhiteSpace(TxtContenido.Text))
            {
                TxtEstado.Text = "Debes completar título y contenido.";
                return;
            }

            if (recursoIdSubido == 0)
            {
                TxtEstado.Text = "Primero debes seleccionar y subir un recurso.";
                return;
            }

            TxtEstado.Text = "Creando publicación...";

            var publicacion = new PublicacionDTO
            {
                titulo = TxtTitulo.Text,
                contenido = TxtContenido.Text,
                usuarioId = usuarioId,
                recursoId = recursoIdSubido
            };

            bool exito = await _publicacionRest.CrearPublicacionAsync(publicacion);
            TxtEstado.Text = exito ? "¡Publicación creada exitosamente!" : "Error al crear publicación.";
        }

    }
}
