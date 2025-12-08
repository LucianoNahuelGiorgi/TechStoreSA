using System.Data;
using System.Drawing.Imaging;
using TechStoreSA.Data;
using TechStoreSA.Enums;
using TechStoreSA.Models;
using TechStoreSA.Services;

namespace TechStoreSA.Views
{
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

        private readonly VentaService _ventaService;
        private readonly ProductoService _productoService;
        private readonly ClienteService _clienteService;
        private readonly SucursalService _sucursalService;

        private Cliente? _clienteSeleccionado;
        private Producto? _productoSeleccionado;
        private List<ItemCarritoDTO> _carrito;

        public VentasForm(Usuario usuario, TechStoreContext context)
        {
            InitializeComponent();
            _context = context;
            _usuarioActual = usuario;

            _clienteService = new ClienteService(context);
            _productoService = new ProductoService(context);
            _sucursalService = new SucursalService(context);
            _sucursalService = new SucursalService(context);
            _ventaService = new VentaService(context, _clienteService);

            _carrito = new List<ItemCarritoDTO>();

            InicializarFormulario();
        }

        private void InicializarFormulario()
        {
            lblFecha.Text = DateTime.Now.ToLongDateString();
            lblDatosVendedor.Text = $"Vendedor: {_usuarioActual.NombreCompleto}";
            cmbMetodoPago.DataSource = Enum.GetValues(typeof(MetodoPago));
            CargarSucursales();
            LimpiarSeleccionProducto();
            LimpiarCliente();
            ActualizarGridCarrito();
        }

        private void CargarSucursales()
        {
            var sucursales = _sucursalService.ObtenerTodas();
            cmbSucursal.DataSource = sucursales;
            cmbSucursal.DisplayMember = "Nombre";
            cmbSucursal.ValueMember = "Id";
            if (sucursales.Count > 0) cmbSucursal.SelectedIndex = 0;
        }

        private int ObtenerSucursalIdSeleccionada()
        {
            if (cmbSucursal.SelectedValue != null && int.TryParse(cmbSucursal.SelectedValue.ToString(), out int id))
                return id;
            return 0;
        }

        // --- MÉTODOS DE BUSQUEDA Y UI (Igual que tenías) ---

        private void btnBuscarCliente_Click(object sender, EventArgs e) => BuscarCliente();
        private void txtBuscarCliente_KeyPress(object sender, KeyPressEventArgs e) { if (e.KeyChar == (char)Keys.Enter) BuscarCliente(); }

        private void BuscarCliente()
        {
            string doc = txtBuscarCliente.Text.Trim();
            if (string.IsNullOrEmpty(doc)) return;
            var cliente = _clienteService.ObtenerPorDocumento(doc);
            if (cliente != null)
            {
                _clienteSeleccionado = cliente;
                lblNombreCliente.Text = $"{cliente.NombreCompleto} ({cliente.Tipo})";
                lblNombreCliente.ForeColor = Color.FromArgb(0, 150, 136);
                CalcularTotales();
            }
            else
            {
                MessageBox.Show("Cliente no encontrado.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LimpiarCliente();
            }
        }

        private void LimpiarCliente()
        {
            _clienteSeleccionado = null;
            lblNombreCliente.Text = "Cliente no seleccionado";
            lblNombreCliente.ForeColor = Color.DimGray;
            CalcularTotales();
        }

        private void btnBuscarProducto_Click(object sender, EventArgs e) => BuscarProducto();
        private void txtBuscarProducto_KeyPress(object sender, KeyPressEventArgs e) { if (e.KeyChar == (char)Keys.Enter) BuscarProducto(); }

        private void BuscarProducto()
        {
            string codigo = txtBuscarProducto.Text.Trim();
            if (string.IsNullOrEmpty(codigo)) return;
            var producto = _productoService.Buscar1(codigo);
            if (producto != null)
            {
                _productoSeleccionado = producto;
                lblNombreProducto.Text = producto.Nombre;
                lblPrecioProducto.Text = producto.PrecioActual.ToString("C2");
                int sucursalId = ObtenerSucursalIdSeleccionada();
                int stockDisponible = _productoService.ConsultarStock(producto.Id, sucursalId);
                lblStockInfo.Text = $"Stock en sucursal: {stockDisponible}";

                if (stockDisponible <= 0)
                {
                    lblStockInfo.ForeColor = Color.IndianRed;
                    btnAgregar.Enabled = false;
                }
                else
                {
                    lblStockInfo.ForeColor = Color.DimGray;
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

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            if (_productoSeleccionado == null) return;
            int cantidad = (int)numCantidad.Value;
            var itemExistente = _carrito.FirstOrDefault(i => i.ProductoId == _productoSeleccionado.Id);

            if (itemExistente != null)
            {
                if (itemExistente.Cantidad + cantidad > numCantidad.Maximum)
                {
                    MessageBox.Show("No hay suficiente stock.", "Stock Insuficiente", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            if (dgvCarrito.Columns["PrecioUnitario"] != null) dgvCarrito.Columns["PrecioUnitario"].DefaultCellStyle.Format = "C2";
            if (dgvCarrito.Columns["Total"] != null) dgvCarrito.Columns["Total"].DefaultCellStyle.Format = "C2";
            if (dgvCarrito.Columns["ProductoId"] != null) dgvCarrito.Columns["ProductoId"].Visible = false;
        }

        private void CalcularTotales()
        {
            decimal subTotal = _carrito.Sum(i => i.Total);
            decimal porcentajeDescuento = 0;
            if (_clienteSeleccionado != null)
            {
                porcentajeDescuento = _clienteService.ObtenerPorcentajeDescuento(_clienteSeleccionado.Tipo);
            }
            decimal montoDescuento = subTotal * porcentajeDescuento;
            lblSubTotal.Text = subTotal.ToString("C2");
            lblDescuento.Text = montoDescuento.ToString("C2");
            lblTotal.Text = (subTotal - montoDescuento).ToString("C2");
        }

        // --- FINALIZAR VENTA ---

        private void btnFinalizar_Click(object sender, EventArgs e)
        {
            if (_carrito.Count == 0) { MessageBox.Show("El carrito está vacío.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (_clienteSeleccionado == null) { MessageBox.Show("Debe seleccionar un cliente.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            int sucursalId = ObtenerSucursalIdSeleccionada();
            if (sucursalId <= 0) { MessageBox.Show("Debe seleccionar una sucursal válida.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            try
            {
                var itemsVenta = _carrito.Select(c => new ItemVenta { ProductoId = c.ProductoId, Cantidad = c.Cantidad }).ToList();

                // AHORA ESTO FUNCIONA PORQUE VentaService RETORNA UN OBJETO VENTA
                Venta nuevaVenta = _ventaService.CrearVenta(
                    _clienteSeleccionado.Id,
                    _usuarioActual.Id,
                    sucursalId,
                    (MetodoPago)cmbMetodoPago.SelectedItem,
                    itemsVenta
                );

                MessageBox.Show("¡Venta registrada con éxito! Guarde su factura.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // LLAMAMOS AL MÉTODO QUE AHORA SÍ EXISTE
                GenerarYDescargarFacturaImagen(nuevaVenta, cmbSucursal.Text);

                _carrito.Clear();
                InicializarFormulario();
                CalcularTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la venta: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // --- MÉTODO PARA GENERAR IMAGEN QUE FALTABA ---
        private void GenerarYDescargarFacturaImagen(Venta venta, string nombreSucursal)
        {
            int ancho = 800;
            // Altura dinámica según items
            int alto = 750 + (venta.Detalles.Count * 35);

            using (Bitmap bitmapFactura = new Bitmap(ancho, alto))
            using (Graphics g = Graphics.FromImage(bitmapFactura))
            {
                g.Clear(Color.White);
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

                Font fontTitulo = new Font("Arial", 18, FontStyle.Bold);
                Font fontSubTitulo = new Font("Arial", 14, FontStyle.Bold);
                Font fontRegular = new Font("Arial", 11, FontStyle.Regular);
                Font fontNegrita = new Font("Arial", 11, FontStyle.Bold);
                Brush brushNegro = Brushes.Black;
                Pen penLinea = new Pen(Color.Gray, 1);

                int y = 30; int margenIzq = 40; int margenDer = ancho - 40;

                // Encabezado
                g.DrawString("TECHSTORE S.A.", fontTitulo, brushNegro, margenIzq, y); y += 35;
                g.DrawString($"Sucursal: {nombreSucursal}", fontRegular, brushNegro, margenIzq, y); y += 25;
                g.DrawString($"Fecha: {venta.Fecha:dd/MM/yyyy HH:mm}", fontRegular, brushNegro, margenIzq, y);
                
                string textoId = $"Factura N°: {venta.Id.ToString("D8")}";
                float anchoId = g.MeasureString(textoId, fontTitulo).Width;
                g.DrawString(textoId, fontTitulo, brushNegro, margenDer - anchoId, 30);
                
                y += 40; g.DrawLine(penLinea, margenIzq, y, margenDer, y); y += 20;

                // Cliente
                string clienteNombre = venta.Cliente != null ? $"{venta.Cliente.NombreCompleto}" : "Consumidor Final";
                string clienteDni = venta.Cliente != null ? venta.Cliente.Documento : "-";

                g.DrawString("DATOS DEL CLIENTE", fontSubTitulo, brushNegro, margenIzq, y); y += 30;
                g.DrawString($"Cliente: {clienteNombre}", fontRegular, brushNegro, margenIzq, y); y += 25;
                g.DrawString($"DNI/CUIT: {clienteDni}", fontRegular, brushNegro, margenIzq, y); y += 25;
                g.DrawString($"Pago: {venta.MetodoPago}", fontRegular, brushNegro, margenIzq, y);
                
                y += 40;

                // Tabla Header
                int colProdX = margenIzq; int colCantX = 450; int colPrecioX = 530; int colTotalX = 660;
                g.DrawString("Producto", fontNegrita, brushNegro, colProdX, y);
                g.DrawString("Cant.", fontNegrita, brushNegro, colCantX, y);
                g.DrawString("Precio", fontNegrita, brushNegro, colPrecioX, y);
                g.DrawString("Subtotal", fontNegrita, brushNegro, colTotalX, y);
                y += 25; g.DrawLine(penLinea, margenIzq, y, margenDer, y); y += 10;

                // Items
                if (venta.Detalles != null)
                {
                    foreach (var det in venta.Detalles)
                    {
                        string nomProd = det.Producto != null ? det.Producto.Nombre : "(Prod. Eliminado)";
                        g.DrawString(nomProd, fontRegular, brushNegro, colProdX, y);
                        g.DrawString(det.Cantidad.ToString(), fontRegular, brushNegro, colCantX + 10, y);
                        g.DrawString(det.PrecioUnitario.ToString("C2"), fontRegular, brushNegro, colPrecioX, y);
                        g.DrawString(det.Importe.ToString("C2"), fontRegular, brushNegro, colTotalX, y);
                        y += 30;
                    }
                }
                
                y += 20; g.DrawLine(penLinea, margenIzq, y, margenDer, y); y += 20;

                // Totales
                StringFormat fmtDer = new StringFormat { Alignment = StringAlignment.Far };
                int xTitulo = 500; int xVal = margenDer;

                g.DrawString("Subtotal:", fontRegular, brushNegro, xTitulo, y);
                g.DrawString(venta.SubTotal.ToString("C2"), fontRegular, brushNegro, xVal, y, fmtDer);
                y += 30;

                if (venta.DescuentoAplicado > 0)
                {
                    g.DrawString("Descuento:", fontRegular, brushNegro, xTitulo, y);
                    g.DrawString("-" + venta.DescuentoAplicado.ToString("C2"), fontRegular, brushNegro, xVal, y, fmtDer);
                    y += 30;
                }

                g.DrawString("TOTAL:", fontTitulo, brushNegro, xTitulo, y);
                g.DrawString(venta.Total.ToString("C2"), fontTitulo, brushNegro, xVal, y, fmtDer);

                // Guardar
                using (SaveFileDialog sfd = new SaveFileDialog())
                {
                    sfd.Title = "Guardar Factura";
                    sfd.Filter = "Imagen PNG|*.png|Imagen JPEG|*.jpg";
                    sfd.FileName = $"Factura_{venta.Id}_{DateTime.Now:yyyyMMdd_HHmmss}";

                    if (sfd.ShowDialog() == DialogResult.OK)
                    {
                        try
                        {
                            var fmt = Path.GetExtension(sfd.FileName).ToLower() == ".jpg" ? ImageFormat.Jpeg : ImageFormat.Png;
                            bitmapFactura.Save(sfd.FileName, fmt);
                            
                            if (MessageBox.Show("Factura guardada. ¿Abrir?", "Éxito", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                            {
                                new System.Diagnostics.Process { StartInfo = new System.Diagnostics.ProcessStartInfo(sfd.FileName) { UseShellExecute = true } }.Start();
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al guardar imagen: " + ex.Message);
                        }
                    }
                }
            }
        }
    }
}