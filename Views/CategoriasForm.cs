using System.Data;
using TechStoreSA.Data;
using TechStoreSA.Models;
using TechStoreSA.Services;

namespace TechStoreSA.Views
{
    public partial class CategoriasForm : Form
    {
        private readonly CategoriaService _categoriaService;
        private int _categoriaIdSeleccionada = 0; // 0 = Modo Creación

        public CategoriasForm(TechStoreContext context)
        {
            InitializeComponent();
            _categoriaService = new CategoriaService(context);

            CargarCategorias();
            LimpiarFormulario();
        }

        private void CargarCategorias()
        {
            var lista = _categoriaService.ObtenerTodas();

            // Proyección simple
            var listaVisible = lista.Select(c => new
            {
                c.Id,
                c.Nombre,
                c.Descripcion
            }).ToList();

            dgvCategorias.DataSource = listaVisible;

            if (dgvCategorias.Columns["Id"] != null)
                dgvCategorias.Columns["Id"].Visible = false;
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
                MessageBox.Show("El nombre de la categoría es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var categoria = new Categoria
                {
                    Id = _categoriaIdSeleccionada,
                    Nombre = txtNombre.Text.Trim(),
                    Descripcion = txtDescripcion.Text.Trim()
                };

                if (_categoriaIdSeleccionada == 0)
                {
                    _categoriaService.Agregar(categoria);
                    MessageBox.Show("Categoría creada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    _categoriaService.Editar(categoria);
                    MessageBox.Show("Categoría actualizada exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarCategorias();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_categoriaIdSeleccionada == 0) return;

            if (MessageBox.Show("¿Está seguro de eliminar esta categoría?", "Confirmar Eliminación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _categoriaService.Eliminar(_categoriaIdSeleccionada);
                    CargarCategorias();
                    LimpiarFormulario();
                    MessageBox.Show("Categoría eliminada.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void dgvCategorias_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvCategorias.SelectedRows.Count > 0)
            {
                var row = dgvCategorias.SelectedRows[0];
                int id = (int)row.Cells["Id"].Value;

                var categoria = _categoriaService.ObtenerPorId(id);
                if (categoria != null)
                {
                    _categoriaIdSeleccionada = categoria.Id;
                    txtNombre.Text = categoria.Nombre;
                    txtDescripcion.Text = categoria.Descripcion;

                    lblTituloPanel.Text = "Editar Categoría";
                    btnGuardar.Text = "Actualizar";
                    btnEliminar.Visible = true;
                }
            }
        }

        private void LimpiarFormulario()
        {
            _categoriaIdSeleccionada = 0;
            txtNombre.Clear();
            txtDescripcion.Clear();

            lblTituloPanel.Text = "Nueva Categoría";
            btnGuardar.Text = "Guardar";
            btnEliminar.Visible = false;
            dgvCategorias.ClearSelection();
        }
    }
}
