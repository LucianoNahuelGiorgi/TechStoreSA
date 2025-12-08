using TechStoreSA.Data;
using TechStoreSA.Services;

namespace TechStoreSA.Views
{
    public partial class StockForm : Form
    {
        private readonly ProductoService _productoService;
        private readonly SucursalService _sucursalService;

        public StockForm(TechStoreContext context)
        {
            InitializeComponent();
            _productoService = new ProductoService(context);
            _sucursalService = new SucursalService(context);

            CargarCombos();
        }

        private void CargarCombos()
        {
            // 1. Configurar Sucursales
            // Primero definimos qué campos usar
            cmbSucursal.DisplayMember = "Nombre";
            cmbSucursal.ValueMember = "Id";
            // Al final cargamos los datos. Esto disparará el evento pero ya sabrá que Value es "Id"
            cmbSucursal.DataSource = _sucursalService.ObtenerTodas();

            // 2. Configurar Productos
            cmbProducto.DisplayMember = "Nombre";
            cmbProducto.ValueMember = "Id";
            // DataSource siempre al final
            cmbProducto.DataSource = _productoService.ObtenerTodos();
        }

        private void ConsultarStockActual()
        {
            if (cmbSucursal.SelectedValue != null && cmbProducto.SelectedValue != null)
            {
                int sucursalId = (int)cmbSucursal.SelectedValue;
                int productoId = (int)cmbProducto.SelectedValue;

                int cantidad = _productoService.ConsultarStock(productoId, sucursalId);

                lblStockActual.Text = $"Stock Actual: {cantidad}";
                numCantidad.Value = cantidad; // Pre-cargar el valor actual
            }
        }

        private void cmbSucursal_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConsultarStockActual();
        }

        private void cmbProducto_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConsultarStockActual();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (cmbSucursal.SelectedValue == null || cmbProducto.SelectedValue == null)
            {
                MessageBox.Show("Seleccione sucursal y producto.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int sucursalId = (int)cmbSucursal.SelectedValue;
                int productoId = (int)cmbProducto.SelectedValue;
                int nuevaCantidad = (int)numCantidad.Value;

                // Llamada al método de ajuste manual en ProductoService
                _productoService.AjustarStockManual(productoId, sucursalId, nuevaCantidad);

                MessageBox.Show("Stock actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ConsultarStockActual();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
