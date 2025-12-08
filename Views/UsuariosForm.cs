using System.Data;
using TechStoreSA.Data;
using TechStoreSA.Models;
using TechStoreSA.Services;

namespace TechStoreSA.Views
{
    public partial class UsuariosForm : Form
    {
        private readonly UsuarioService _usuarioService;
        private int _usuarioIdSeleccionado = 0; // 0 = Modo Creación

        public UsuariosForm(TechStoreContext context)
        {
            InitializeComponent();
            _usuarioService = new UsuarioService(context);

            CargarUsuarios();
            LimpiarFormulario();
        }

        private void CargarUsuarios()
        {
            var usuarios = _usuarioService.ObtenerTodos();

            // Proyección para no mostrar el Hash de la contraseña en la grilla
            var listaVisible = usuarios.Select(u => new
            {
                u.Id,
                u.NombreCompleto,
                Usuario = u.NombreUsuario,
                Rol = u.EsAdministrador ? "Administrador" : "Vendedor"
            }).ToList();

            dgvUsuarios.DataSource = listaVisible;

            if (dgvUsuarios.Columns["Id"] != null)
                dgvUsuarios.Columns["Id"].Visible = false;
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(txtNombre.Text) || string.IsNullOrWhiteSpace(txtUsuario.Text))
            {
                MessageBox.Show("El nombre y el usuario son obligatorios.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Validar contraseñas
            if (txtPassword.Text != txtConfirmarPass.Text)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Si es nuevo, la contraseña es obligatoria
            if (_usuarioIdSeleccionado == 0 && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Debe asignar una contraseña al nuevo usuario.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var usuario = new Usuario
                {
                    Id = _usuarioIdSeleccionado,
                    NombreCompleto = txtNombre.Text.Trim(),
                    NombreUsuario = txtUsuario.Text.Trim(),
                    EsAdministrador = chkEsAdmin.Checked
                };

                if (_usuarioIdSeleccionado == 0)
                {
                    // Crear Nuevo (pasamos la contraseña en texto plano para que el servicio la guarde)
                    _usuarioService.Crear(usuario, txtPassword.Text);
                    MessageBox.Show("Usuario creado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    // Editar (pasamos la contraseña solo si el usuario escribió una nueva)
                    string? nuevaPass = string.IsNullOrWhiteSpace(txtPassword.Text) ? null : txtPassword.Text;

                    _usuarioService.Editar(usuario, nuevaPass);
                    MessageBox.Show("Usuario actualizado exitosamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                CargarUsuarios();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_usuarioIdSeleccionado == 0) return;

            if (MessageBox.Show("¿Está seguro de eliminar este usuario? Esta acción no se puede deshacer.",
                "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _usuarioService.Eliminar(_usuarioIdSeleccionado);
                    CargarUsuarios();
                    LimpiarFormulario();
                    MessageBox.Show("Usuario eliminado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void dgvUsuarios_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsuarios.SelectedRows.Count > 0)
            {
                var row = dgvUsuarios.SelectedRows[0];
                int id = (int)row.Cells["Id"].Value;

                var usuario = _usuarioService.ObtenerPorId(id);
                if (usuario != null)
                {
                    _usuarioIdSeleccionado = usuario.Id;

                    // Rellenar campos
                    txtNombre.Text = usuario.NombreCompleto;
                    txtUsuario.Text = usuario.NombreUsuario;
                    chkEsAdmin.Checked = usuario.EsAdministrador;

                    // Limpiar campos de contraseña (no mostramos la actual por seguridad)
                    txtPassword.Clear();
                    txtConfirmarPass.Clear();

                    lblTituloPanel.Text = "Editar Usuario";
                    btnGuardar.Text = "Actualizar";
                    btnEliminar.Visible = true;
                    lblInfoPass.Visible = true; // Mostrar aviso de "dejar en blanco"
                }
            }
        }

        private void LimpiarFormulario()
        {
            _usuarioIdSeleccionado = 0;
            txtNombre.Clear();
            txtUsuario.Clear();
            txtPassword.Clear();
            txtConfirmarPass.Clear();
            chkEsAdmin.Checked = false;

            lblTituloPanel.Text = "Nuevo Usuario";
            btnGuardar.Text = "Guardar";
            btnEliminar.Visible = false;
            lblInfoPass.Visible = false;
            dgvUsuarios.ClearSelection();
        }
    }
}
