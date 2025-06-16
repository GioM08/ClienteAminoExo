using System.Windows.Controls;
using ClienteAminoExo.Servicios.gRPC;
using System.Collections.Generic;
using System.Windows;
using System.Threading.Tasks;
using Estadistica;
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
            foreach (var recurso in estadisticas.RecursosPorTipo)
            {
                GraficaRecursos.Series.Add(new PieSeries
                {
                    Title = recurso.Tipo,
                    Values = new ChartValues<int> { recurso.Total },
                    DataLabels = true
                });
            }
        }
    }
}
