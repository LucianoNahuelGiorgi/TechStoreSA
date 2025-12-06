using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TechStoreSA.Data;
using TechStoreSA.Enums;
using TechStoreSA.Models;
using TechStoreSA.Services;

namespace TechStoreSA.Views
{
    // DTO simple para mostrar en la Grilla del Carrito
    public class ItemCarritoDTO
    {
        public int ProductoId { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public decimal Total => PrecioUnitario * Cantidad;
    }

    public partial class VentasForm : Form
    {
        private readonly TechStoreContext _context;
        private readonly Usuario _usuarioActual;

        // Servicios
        private readonly VentaService _ventaService;
        private readonly ProductoService _productoService;
        private readonly ClienteService _clienteService;
        private readonly SucursalService _sucursalService; // <--- AGREGADO

        // Estado de la Venta actual
        private Cliente? _clienteSeleccionado;
        private Producto? _productoSeleccionado;
        private List<ItemCarritoDTO> _carrito;

        public VentasForm(Usuario usuario, TechStoreContext context)
        {
            InitializeComponent();
            _context = context;
            _usuarioActual = usuario;

            // Inicializar servicios
            _clienteService = new ClienteService(context);
            _productoService = new ProductoService(context);
            _sucursalService = new SucursalService(context); // <--- AGREGADO
            // Pasamos null en clienteService si el constructor de VentaService no lo pide, 
            // pero en tu código anterior sí lo pedía. Ajusta según tu VentaService real.
            _ventaService = new VentaService(context, _clienteService);

            _carrito = new List<ItemCarritoDTO>();

            InicializarFormulario();
        }

        private void InicializarFormulario()
        {
            lblFecha.Text = DateTime.Now.ToLongDateString();

            // CORREGIDO: Ya no usamos _usuarioActual.SucursalId porque no existe.
            lblDatosVendedor.Text = $"Vendedor: {_usuarioActual.NombreCompleto}";

            // Cargar combo de Métodos de Pago
            cmbMetodoPago.DataSource = Enum.GetValues(typeof(MetodoPago));

            // --- AGREGADO: Cargar Sucursales ---
            CargarSucursales();

            LimpiarSeleccionProducto();
            LimpiarCliente();
            ActualizarGridCarrito();
        }

        private void CargarSucursales()
        {
            // Llenamos el combo con las sucursales disponibles
            var sucursales = _sucursalService.ObtenerTodas();

            // Asumiendo que agregaste el control 'cmbSucursal' al formulario
            cmbSucursal.DataSource = sucursales;
            cmbSucursal.DisplayMember = "Nombre";
            cmbSucursal.ValueMember = "Id";

            if (sucursales.Count > 0)
            {
                cmbSucursal.SelectedIndex = 0; // Seleccionar la primera por defecto
            }
        }

        // Helper para obtener el ID de la sucursal seleccionada en el Combo
        private int ObtenerSucursalIdSeleccionada()
        {
            if (cmbSucursal.SelectedValue != null && int.TryParse(cmbSucursal.SelectedValue.ToString(), out int id))
            {
                return id;
            }
            return 0;
        }

        // --- 1. BUSQUEDA DE CLIENTE ---

        private void btnBuscarCliente_Click(object sender, EventArgs e)
        {
            BuscarCliente();
        }

        private void txtBuscarCliente_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) BuscarCliente();
        }

        private void BuscarCliente()
        {
            string doc = txtBuscarCliente.Text.Trim();
            if (string.IsNullOrEmpty(doc)) return;

            var cliente = _clienteService.ObtenerPorDocumento(doc);

            if (cliente != null)
            {
                _clienteSeleccionado = cliente;
                lblNombreCliente.Text = cliente.NombreCompleto;
                // Ajuste visual: Manejo seguro de TipoCliente
                lblNombreCliente.Text += $" ({cliente.Tipo})";
                lblNombreCliente.ForeColor = System.Drawing.Color.FromArgb(0, 150, 136); // Verde

                CalcularTotales();
            }
            else
            {
                MessageBox.Show("Cliente no encontrado con ese Documento.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LimpiarCliente();
            }
        }

        private void LimpiarCliente()
        {
            _clienteSeleccionado = null;
            lblNombreCliente.Text = "Cliente no seleccionado";
            lblNombreCliente.ForeColor = System.Drawing.Color.DimGray;
            // lblTipoCliente.Text = "-"; // Comentado si no tienes este label
            CalcularTotales();
        }

        // --- 2. BUSQUEDA DE PRODUCTO ---

        private void btnBuscarProducto_Click(object sender, EventArgs e)
        {
            BuscarProducto();
        }

        private void txtBuscarProducto_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter) BuscarProducto();
        }

        private void BuscarProducto()
        {
            string codigo = txtBuscarProducto.Text.Trim();
            if (string.IsNullOrEmpty(codigo)) return;

            var producto = _productoService.ObtenerPorCodigo(codigo);

            if (producto != null)
            {
                _productoSeleccionado = producto;
                lblNombreProducto.Text = producto.Nombre;
                lblPrecioProducto.Text = producto.PrecioActual.ToString("C2");

                // CORREGIDO: Consultar Stock en la Sucursal SELECCIONADA
                int sucursalId = ObtenerSucursalIdSeleccionada();
                int stockDisponible = _productoService.ConsultarStock(producto.Id, sucursalId);

                lblStockInfo.Text = $"Stock en sucursal: {stockDisponible}";

                if (stockDisponible <= 0)
                {
                    lblStockInfo.ForeColor = System.Drawing.Color.IndianRed;
                    btnAgregar.Enabled = false;
                }
                else
                {
                    lblStockInfo.ForeColor = System.Drawing.Color.DimGray;
                    btnAgregar.Enabled = true;
                    numCantidad.Maximum = stockDisponible;
                    numCantidad.Value = 1;
                    numCantidad.Focus();
                }
            }
            else
            {
                MessageBox.Show("Producto no encontrado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LimpiarSeleccionProducto();
            }
        }

        private void LimpiarSeleccionProducto()
        {
            _productoSeleccionado = null;
            lblNombreProducto.Text = "---";
            lblPrecioProducto.Text = "$0.00";
            lblStockInfo.Text = "Stock: -";
            txtBuscarProducto.Clear();
            txtBuscarProducto.Focus();
            btnAgregar.Enabled = false;
        }

        // --- 3. AGREGAR AL CARRITO ---

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (_productoSeleccionado == null) return;

            int cantidad = (int)numCantidad.Value;
            var itemExistente = _carrito.FirstOrDefault(i => i.ProductoId == _productoSeleccionado.Id);

            if (itemExistente != null)
            {
                if (itemExistente.Cantidad + cantidad > numCantidad.Maximum)
                {
                    MessageBox.Show("No hay suficiente stock para agregar más unidades.", "Stock Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                itemExistente.Cantidad += cantidad;
            }
            else
            {
                _carrito.Add(new ItemCarritoDTO
                {
                    ProductoId = _productoSeleccionado.Id,
                    Codigo = _productoSeleccionado.Codigo,
                    Nombre = _productoSeleccionado.Nombre,
                    PrecioUnitario = _productoSeleccionado.PrecioActual,
                    Cantidad = cantidad
                });
            }

            ActualizarGridCarrito();
            LimpiarSeleccionProducto();
            CalcularTotales();
        }

        private void ActualizarGridCarrito()
        {
            dgvCarrito.DataSource = null;
            dgvCarrito.DataSource = _carrito;

            if (dgvCarrito.Columns["PrecioUnitario"] != null)
                dgvCarrito.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
            if (dgvCarrito.Columns["Total"] != null)
                dgvCarrito.Columns["Total"].DefaultCellStyle.Format = "C2";
            if (dgvCarrito.Columns["ProductoId"] != null)
                dgvCarrito.Columns["ProductoId"].Visible = false;
        }

        private void CalcularTotales()
        {
            decimal subTotal = _carrito.Sum(i => i.Total);
            decimal porcentajeDescuento = 0;

            if (_clienteSeleccionado != null)
            {
                // Asegúrate que tu ClienteService tenga este método, si no, usa lógica directa
                // porcentajeDescuento = _clienteService.ObtenerPorcentajeDescuento(_clienteSeleccionado.Tipo);
                porcentajeDescuento = (_clienteSeleccionado.Tipo == TipoCliente.Mayorista) ? 0.10m : 0m;
            }

            decimal montoDescuento = subTotal * porcentajeDescuento;
            decimal total = subTotal - montoDescuento;

            lblSubTotal.Text = subTotal.ToString("C2");
            lblDescuento.Text = montoDescuento.ToString("C2");
            lblTotal.Text = total.ToString("C2");
        }

        // --- 4. FINALIZAR VENTA ---

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            if (_carrito.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_clienteSeleccionado == null)
            {
                MessageBox.Show("Debe seleccionar un cliente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar que se haya seleccionado sucursal
            int sucursalId = ObtenerSucursalIdSeleccionada();
            if (sucursalId <= 0)
            {
                MessageBox.Show("Debe seleccionar una sucursal válida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var itemsVenta = _carrito.Select(c => new ItemVenta
                {
                    ProductoId = c.ProductoId,
                    Cantidad = c.Cantidad
                }).ToList();

                // CORREGIDO: Usamos la sucursal del Combo, NO la del usuario
                _ventaService.CrearVenta(
                    _clienteSeleccionado.Id,
                    _usuarioActual.Id,
                    sucursalId, // <--- Aquí pasamos la sucursal elegida
                    (MetodoPago)cmbMetodoPago.SelectedItem,
                    itemsVenta
                );

                MessageBox.Show("¡Venta registrada con éxito!", "Venta Finalizada", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _carrito.Clear();
                InicializarFormulario();
                CalcularTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la venta: {ex.Message}", "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
