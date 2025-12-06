using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TechStoreSA.Data;
using TechStoreSA.Models;
using TechStoreSA.Services;

namespace TechStoreSA.Views
{
    public partial class ProductosForm : Form
    {
        private readonly ProductoService _productoService;
        private readonly CategoriaService _categoriaService;
        private readonly SucursalService _sucursalService;

        private int _productoIdSeleccionado = 0; // 0 = Nuevo

        public ProductosForm(TechStoreContext context)
        {
            InitializeComponent();
            _productoService = new ProductoService(context);
            _categoriaService = new CategoriaService(context);
            _sucursalService = new SucursalService(context);

            CargarCombos();
            CargarProductos();
            LimpiarFormulario();
        }

        private void CargarCombos()
        {
            // 1. Combo de Categorías
            var categorias = _categoriaService.ObtenerTodas();
            cmbCategoria.DataSource = categorias;
            cmbCategoria.DisplayMember = "Nombre";
            cmbCategoria.ValueMember = "Id";

            // 2. Combo de Sucursales (Para consulta de Stock)
            var sucursales = _sucursalService.ObtenerTodas();
            cmbSucursalStock.DataSource = sucursales;
            cmbSucursalStock.DisplayMember = "Nombre";
            cmbSucursalStock.ValueMember = "Id";
        }

        private void CargarProductos(string criterio = "")
        {
            List<Producto> lista;
            if (string.IsNullOrWhiteSpace(criterio))
                lista = _productoService.ObtenerTodos();
            else
                lista = _productoService.Buscar(criterio);

            // Proyección para mostrar nombre de categoría y formatear precio
            var listaGrid = lista.Select(p => new
            {
                p.Id,
                p.Codigo,
                p.Nombre,
                Precio = p.PrecioActual,
                Categoria = p.Categoria != null ? p.Categoria.Nombre : "Sin Categoría"
            }).ToList();

            dgvProductos.DataSource = listaGrid;

            // Ajustes visuales de columnas
            if (dgvProductos.Columns["Id"] != null) dgvProductos.Columns["Id"].Visible = false;
            if (dgvProductos.Columns["Precio"] != null) dgvProductos.Columns["Precio"].DefaultCellStyle.Format = "C2";
        }

        // --- ACCIONES ---

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            CargarProductos(txtBuscar.Text);
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtCodigo.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // Validaciones básicas
                if (cmbCategoria.SelectedItem == null) throw new Exception("Seleccione una categoría.");
                if (string.IsNullOrWhiteSpace(txtCodigo.Text)) throw new Exception("El código es obligatorio.");

                var producto = new Producto
                {
                    Codigo = txtCodigo.Text.Trim(),
                    Nombre = txtNombre.Text.Trim(),
                    Descripcion = txtDescripcion.Text.Trim(),
                    PrecioActual = numPrecio.Value,
                    CategoriaId = (int)cmbCategoria.SelectedValue
                };

                if (_productoIdSeleccionado == 0)
                {
                    // Crear
                    _productoService.Crear(producto);
                    MessageBox.Show("Producto registrado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Editar
                    producto.Id = _productoIdSeleccionado;
                    _productoService.Editar(producto);
                    MessageBox.Show("Producto actualizado.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarProductos();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_productoIdSeleccionado == 0) return;

            if (MessageBox.Show("¿Desea eliminar este producto?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _productoService.Eliminar(_productoIdSeleccionado);
                    CargarProductos();
                    LimpiarFormulario();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        // --- SELECCIÓN Y CONSULTA DE STOCK ---

        private void dgvProductos_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProductos.SelectedRows.Count > 0)
            {
                var row = dgvProductos.SelectedRows[0];
                int id = (int)row.Cells["Id"].Value;

                var producto = _productoService.ObtenerPorId(id);
                if (producto != null)
                {
                    _productoIdSeleccionado = producto.Id;

                    // Rellenar campos
                    txtCodigo.Text = producto.Codigo;
                    txtNombre.Text = producto.Nombre;
                    txtDescripcion.Text = producto.Descripcion;
                    numPrecio.Value = producto.PrecioActual;
                    cmbCategoria.SelectedValue = producto.CategoriaId;

                    // Actualizar UI
                    lblTituloPanel.Text = "Editar Producto";
                    btnGuardar.Text = "Actualizar";
                    btnEliminar.Visible = true;

                    // Consultar Stock en la sucursal seleccionada actualmente
                    ConsultarStockActual();
                }
            }
        }

        private void cmbSucursalStock_SelectedIndexChanged(object sender, EventArgs e)
        {
            ConsultarStockActual();
        }

        private void ConsultarStockActual()
        {
            if (_productoIdSeleccionado == 0 || cmbSucursalStock.SelectedItem == null)
            {
                lblCantidadStock.Text = "-";
                return;
            }

            try
            {
                int sucursalId = (int)cmbSucursalStock.SelectedValue;
                // Usamos el servicio para ver disponibilidad real
                int cantidad = _productoService.ConsultarStock(_productoIdSeleccionado, sucursalId);

                lblCantidadStock.Text = cantidad.ToString();

                // Color visual: Rojo si no hay stock, Verde si hay
                lblCantidadStock.ForeColor = cantidad > 0
                    ? System.Drawing.Color.FromArgb(0, 150, 136)
                    : System.Drawing.Color.IndianRed;
            }
            catch
            {
                lblCantidadStock.Text = "?";
            }
        }

        private void LimpiarFormulario()
        {
            _productoIdSeleccionado = 0;
            txtCodigo.Clear();
            txtNombre.Clear();
            txtDescripcion.Clear();
            numPrecio.Value = 0;
            if (cmbCategoria.Items.Count > 0) cmbCategoria.SelectedIndex = 0;

            lblTituloPanel.Text = "Nuevo Producto";
            btnGuardar.Text = "Guardar";
            btnEliminar.Visible = false;
            dgvProductos.ClearSelection();
            lblCantidadStock.Text = "-";
        }
    }
}
