using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ClienteAminoExo.Servicios.gRPC;
using Estadistica;
using iTextSharp.text;
using iTextSharp.text.pdf;
using LiveCharts;
using LiveCharts.Wpf;

namespace ClienteAminoExo.Paginas
{
    public partial class PaginaEstadisticas : Page
    {
        public PaginaEstadisticas()
        {
            InitializeComponent();
            CargarEstadisticas();
            TxtTituloEstadisticas.Text = $"📊 Estadísticas generales del {DateTime.Now:dd/MM/yyyy}";
        }

        private async void CargarEstadisticas()
        {
            var servicio = new EstadisticaGrpcService();
            var estadisticas = await servicio.ObtenerEstadisticasAsync();

            TxtTopLikes.Text = $"📌 ID publicación con más likes: {estadisticas.TopLikes.PublicacionId} ({estadisticas.TopLikes.Total} likes)";
            TxtTopComentarios.Text = $"💬 ID publicación con más comentarios: {estadisticas.TopComentarios.PublicacionId} ({estadisticas.TopComentarios.Total} comentarios)";
            TxtTotalPublicaciones.Text = $"📝 Total de publicaciones: {estadisticas.TotalPublicaciones}";
            TxtDiaTop.Text = $"📅 Día con más publicaciones: {estadisticas.DiaConMasPublicaciones} ({estadisticas.PublicacionesEnEseDia})";
            TxtUsuarioTopPublicaciones.Text = $"👤 Usuario con más publicaciones: {estadisticas.UsuarioTopPublicaciones.Nombre} (ID: {estadisticas.UsuarioTopPublicaciones.UsuarioId})";
            TxtUsuarioTopComentarios.Text = $"💬 Usuario con más comentarios: {estadisticas.UsuarioTopComentarios.Nombre} (ID: {estadisticas.UsuarioTopComentarios.UsuarioId})";
            TxtNotificacionesPendientes.Text = $"🔔 Notificaciones no leídas: {estadisticas.NotificacionesPendientes}";
            TxtUsuarioTopReacciones.Text = $"❤️ Usuario con más reacciones: {estadisticas.UsuarioTopReacciones.Nombre} (ID: {estadisticas.UsuarioTopReacciones.UsuarioId})";

            GraficaRecursos.Series = new SeriesCollection();
            GraficaRecursos.AxisX.Clear();
            GraficaRecursos.AxisY.Clear();

            var etiquetas = new List<string>();

            foreach (var recurso in estadisticas.RecursosPorTipo)
            {
                GraficaRecursos.Series.Add(new ColumnSeries
                {
                    Title = recurso.Tipo,
                    Values = new ChartValues<int> { recurso.Total },
                    DataLabels = true
                });

                etiquetas.Add(recurso.Tipo);
            }

            GraficaRecursos.AxisX.Add(new Axis
            {
                Title = "Tipo de recurso",
                Labels = etiquetas
            });

            GraficaRecursos.AxisY.Add(new Axis
            {
                Title = "Total",
                LabelFormatter = value => value.ToString("N0")
            });
        }

        private void BtnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            string rutaArchivo = $"Estadisticas_{DateTime.Now:yyyyMMdd}.pdf";

            Document doc = new Document(PageSize.A4);
            PdfWriter.GetInstance(doc, new FileStream(rutaArchivo, FileMode.Create));
            doc.Open();

            var titulo = new Paragraph($"📊 Estadísticas generales del {DateTime.Now:dd/MM/yyyy}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18))
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            doc.Add(titulo);

            AgregarParrafo(doc, TxtTopLikes.Text);
            AgregarParrafo(doc, TxtTopComentarios.Text);
            AgregarParrafo(doc, TxtTotalPublicaciones.Text);
            AgregarParrafo(doc, TxtDiaTop.Text);
            AgregarParrafo(doc, TxtUsuarioTopPublicaciones.Text);
            AgregarParrafo(doc, TxtUsuarioTopComentarios.Text);
            AgregarParrafo(doc, TxtUsuarioTopReacciones.Text);
            AgregarParrafo(doc, TxtNotificacionesPendientes.Text);

            doc.Add(new Paragraph(" ", FontFactory.GetFont(FontFactory.HELVETICA, 8))); // Separador

            PdfPTable tabla = new PdfPTable(2);
            tabla.WidthPercentage = 100;
            tabla.AddCell("Tipo de recurso");
            tabla.AddCell("Total");

            foreach (var serie in GraficaRecursos.Series)
            {
                var col = serie as ColumnSeries;
                if (col != null && col.Values.Count > 0)
                {
                    tabla.AddCell(col.Title);
                    tabla.AddCell(col.Values[0].ToString());
                }
            }

            doc.Add(tabla);
            doc.Close();

            MessageBox.Show($"✅ PDF generado exitosamente en:\n{Path.GetFullPath(rutaArchivo)}", "PDF Exportado", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AgregarParrafo(Document doc, string texto)
        {
            var parrafo = new Paragraph(texto, FontFactory.GetFont(FontFactory.HELVETICA, 12))
            {
                SpacingAfter = 10f
            };
            doc.Add(parrafo);
        }
    }
}
