﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClienteAminoExo.Servicios.REST;
using ClienteAminoExo.Utils;
using static ClienteAminoExo.Servicios.REST.NotificacionRestService;

namespace ClienteAminoExo.Paginas
{
    public partial class PaginaNotificaciones : Page
    {
        private readonly NotificacionRestService _notificacionService;
        private ObservableCollection<NotificacionRestService.Notificacion> notificaciones = new ObservableCollection<NotificacionRestService.Notificacion>();
        private int usuarioId;

        public PaginaNotificaciones()
        {
            InitializeComponent();
            _notificacionService = new NotificacionRestService();
            CargarDatosUsuarioYNotificaciones();
        }

        private async void CargarDatosUsuarioYNotificaciones()
        {
            try
            {
                var servicio = new UsuarioRestService(SesionActual.Token);
                var perfil = await servicio.ObtenerPerfilAsync();
                var _usuarioActual = perfil.usuario;
                SesionActual.UsuarioId = _usuarioActual.usuarioId;
                usuarioId = _usuarioActual.usuarioId;

                await CargarNotificaciones();
            }
            catch (Exception ex)
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

        private async Task CargarNotificaciones()
        {
            try
            {
                var respuesta = await _notificacionService.ObtenerNotificacionesAsync(usuarioId, leida: null);
                notificaciones = new ObservableCollection<NotificacionRestService.Notificacion>(respuesta.resultados);
                ListaNotificaciones.ItemsSource = notificaciones;

                ListaNotificaciones.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
            }
            catch (Exception ex)
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

        private void ItemContainerGenerator_StatusChanged(object sender, EventArgs e)
        {
            if (ListaNotificaciones.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
            {
                foreach (var item in ListaNotificaciones.Items)
                {
                    var container = ListaNotificaciones.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                    if (container != null)
                    {
                        var noti = item as NotificacionRestService.Notificacion;
                        if (noti != null)
                        {
                            container.Foreground = noti.leida ? new SolidColorBrush(Colors.Gray) : new SolidColorBrush(Colors.White);
                        }
                    }
                }

                ListaNotificaciones.ItemContainerGenerator.StatusChanged -= ItemContainerGenerator_StatusChanged;
            }
        }

        private async void BtnMarcarLeida_Click(object sender, RoutedEventArgs e)
        {
            if (ListaNotificaciones.SelectedItem is NotificacionRestService.Notificacion seleccionada)
            {
                await _notificacionService.MarcarComoLeidaAsync(seleccionada.notificacionId);
                await CargarNotificaciones();
            }
        }

        private async void BtnMarcarMultiples_Click(object sender, RoutedEventArgs e)
        {
            var seleccionadas = ListaNotificaciones.SelectedItems.Cast<NotificacionRestService.Notificacion>().ToList();
            if (!seleccionadas.Any())
            {
                MessageBox.Show("Selecciona al menos una notificación.");
                return;
            }

            var ids = seleccionadas.Select(n => n._id).ToList();
            await _notificacionService.MarcarMultiplesComoLeidasAsync(ids);
            await CargarNotificaciones();
        }

        private async void BtnEliminar_Click(object sender, RoutedEventArgs e)
        {
            if (ListaNotificaciones.SelectedItem is NotificacionRestService.Notificacion seleccionada)
            {
                await _notificacionService.EliminarNotificacionAsync(seleccionada.notificacionId);
                await CargarNotificaciones();
            }
        }
    }
}
