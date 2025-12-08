using System.Data;
using TechStoreSA.Data;
using TechStoreSA.Models;
using TechStoreSA.Services;

namespace TechStoreSA.Views
{
    public partial class SucursalesForm : Form
    {
        private readonly SucursalService _sucursalService;
        private int _sucursalIdSeleccionada = 0; // 0 = Modo Creación

        public SucursalesForm(TechStoreContext context)
        {
            InitializeComponent();
            _sucursalService = new SucursalService(context);

            CargarSucursales();
            LimpiarFormulario();
        }

        private void CargarSucursales()
        {
            var lista = _sucursalService.ObtenerTodas();

            var listaVisible = lista.Select(s => new
            {
                s.Id,
                s.Nombre,
                s.Direccion
            }).ToList();

            dgvSucursales.DataSource = listaVisible;

            if (dgvSucursales.Columns["Id"] != null)
                dgvSucursales.Columns["Id"].Visible = false;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("El nombre de la sucursal es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var sucursal = new Sucursal
                {
                    Id = _sucursalIdSeleccionada,
                    Nombre = txtNombre.Text.Trim(),
                    Direccion = txtDireccion.Text.Trim()
                };

                if (_sucursalIdSeleccionada == 0)
                {
                    _sucursalService.Agregar(sucursal);
                    MessageBox.Show("Sucursal creada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _sucursalService.Editar(sucursal);
                    MessageBox.Show("Sucursal actualizada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarSucursales();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_sucursalIdSeleccionada == 0) return;

            if (MessageBox.Show("¿Está seguro de eliminar esta sucursal?", "Confirmar Eliminación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _sucursalService.Eliminar(_sucursalIdSeleccionada);
                    CargarSucursales();
                    LimpiarFormulario();
                    MessageBox.Show("Sucursal eliminada.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"No se puede eliminar: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnCancelar_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
        }

        private void dgvSucursales_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvSucursales.SelectedRows.Count > 0)
            {
                var row = dgvSucursales.SelectedRows[0];
                int id = (int)row.Cells["Id"].Value;

                var sucursal = _sucursalService.ObtenerPorId(id);
                if (sucursal != null)
                {
                    _sucursalIdSeleccionada = sucursal.Id;
                    txtNombre.Text = sucursal.Nombre;
                    txtDireccion.Text = sucursal.Direccion;

                    lblTituloPanel.Text = "Editar Sucursal";
                    btnGuardar.Text = "Actualizar";
                    btnEliminar.Visible = true;
                }
            }
        }

        private void LimpiarFormulario()
        {
            _sucursalIdSeleccionada = 0;
            txtNombre.Clear();
            txtDireccion.Clear();

            lblTituloPanel.Text = "Nueva Sucursal";
            btnGuardar.Text = "Guardar";
            btnEliminar.Visible = false;
            dgvSucursales.ClearSelection();
        }
    }
}
