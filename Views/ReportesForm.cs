using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TechStoreSA.Data;
using TechStoreSA.Services;
using System.Linq; // Asegurarse de tener LINQ

namespace TechStoreSA.Views
{
    public partial class ReportesForm : Form
    {
        private readonly ReporteService _reporteService;

        public ReportesForm(TechStoreContext context)
        {
            InitializeComponent();
            _reporteService = new ReporteService(context);

            ConfigurarFechas();
            cmbTipoReporte.SelectedIndex = 0; // Seleccionar el primero por defecto
        }

        private void ConfigurarFechas()
        {
            // Configurar rango por defecto: Mes Actual
            var hoy = DateTime.Now;
            dtpDesde.Value = new DateTime(hoy.Year, hoy.Month, 1);
            dtpHasta.Value = hoy;
        }

        private void btnGenerar_Click(object sender, EventArgs e)
        {
            GenerarReporte();
        }

        private void GenerarReporte()
        {
            DateTime desde = dtpDesde.Value.Date; // Inicio del día (00:00:00)
            DateTime hasta = dtpHasta.Value.Date.AddDays(1).AddTicks(-1); // Fin del día (23:59:59)

            string tipoReporte = cmbTipoReporte.SelectedItem?.ToString() ?? "";

            try
            {
                object dataSource = null;

                switch (tipoReporte)
                {
                    case "Productos Más Vendidos":
                        dataSource = _reporteService.ObtenerProductosMasVendidos(desde, hasta);
                        break;

                    case "Desempeño de Vendedores":
                        dataSource = _reporteService.ObtenerVentasPorVendedor(desde, hasta);
                        break;

                    case "Total Ventas por Sucursal":
                        // Convertir el Dictionary a una lista de objetos anónimos para que el Grid lo entienda
                        var reporteSucursal = _reporteService.ObtenerTotalVentasPorSucursal(desde, hasta);
                        dataSource = reporteSucursal.Select(x => new { Sucursal = x.Key, TotalVendido = x.Value }).ToList();
                        break;

                    case "Listado Detallado de Ventas":
                        var listaVentas = _reporteService.ObtenerVentasDetalladas(desde, hasta);
                        // Proyección para mostrar nombres en lugar de objetos complejos
                        dataSource = listaVentas.Select(v => new
                        {
                            v.Id,
                            Fecha = v.Fecha,
                            Cliente = v.Cliente.NombreCompleto,
                            Vendedor = v.Vendedor.NombreUsuario,
                            Sucursal = v.Sucursal.Nombre,
                            Pago = v.MetodoPago,
                            Total = v.Total
                        }).ToList();
                        break;

                    case "Cuenta Corriente":
                        dataSource = _reporteService.ObtenerEstadoClientes();
                        break;

                    default:
                        MessageBox.Show("Seleccione un tipo de reporte válido.");
                        return;
                }

                // Asignar datos y formatear
                dgvReportes.DataSource = dataSource;
                AplicarFormatoGrid(tipoReporte);

                // Actualizar contador
                lblTotalResultados.Text = $"Resultados: {dgvReportes.Rows.Count}";

                if (dgvReportes.Rows.Count == 0)
                {
                    MessageBox.Show("No se encontraron datos para el criterio seleccionado.", "Reporte Vacío", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar reporte: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AplicarFormatoGrid(string tipoReporte)
        {
            // Formatos generales para moneda
            if (dgvReportes.Columns["IngresosGenerados"] != null)
                dgvReportes.Columns["IngresosGenerados"].DefaultCellStyle.Format = "C2";

            if (dgvReportes.Columns["TotalFacturado"] != null)
                dgvReportes.Columns["TotalFacturado"].DefaultCellStyle.Format = "C2";

            if (dgvReportes.Columns["TotalVendido"] != null)
                dgvReportes.Columns["TotalVendido"].DefaultCellStyle.Format = "C2";

            if (dgvReportes.Columns["Total"] != null)
                dgvReportes.Columns["Total"].DefaultCellStyle.Format = "C2";

            // NUEVO: Formato para el reporte de clientes
            if (dgvReportes.Columns["TotalGastado"] != null)
                dgvReportes.Columns["TotalGastado"].DefaultCellStyle.Format = "C2";

            // Encabezados más amigables (ejemplo)
            if (dgvReportes.Columns["CantidadVendida"] != null)
                dgvReportes.Columns["CantidadVendida"].HeaderText = "Unidades";
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            // Funcionalidad placeholder para futura implementación
            if (dgvReportes.Rows.Count > 0)
            {
                MessageBox.Show("Funcionalidad de exportación a Excel/PDF pendiente de implementación.\n\n" +
                                "Los datos están listos en el DataGridView para ser procesados.",
                                "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("No hay datos para exportar. Genere un reporte primero.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}