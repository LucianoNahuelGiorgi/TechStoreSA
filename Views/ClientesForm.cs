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
    public partial class ClientesForm : Form
    {
        private readonly ClienteService _clienteService;
        private int _clienteIdSeleccionado = 0; // 0 indica modo "Nuevo"

        public ClientesForm(TechStoreContext context)
        {
            InitializeComponent();
            _clienteService = new ClienteService(context);

            ConfigurarGrid();
            CargarComboTipoCliente();
            CargarClientes();
            LimpiarFormulario(); // Iniciar en estado "Nuevo"
        }

        private void ConfigurarGrid()
        {
            // Ocultar columnas automáticas innecesarias si usas DataSource directo
            // (Se ajusta dinámicamente al cargar los datos)
        }

        private void CargarComboTipoCliente()
        {
            cmbTipoCliente.DataSource = Enum.GetValues(typeof(TipoCliente));
        }

        private void CargarClientes(string filtro = "")
        {
            List<Cliente> lista;
            if (string.IsNullOrWhiteSpace(filtro))
            {
                lista = _clienteService.ObtenerTodos();
            }
            else
            {
                lista = _clienteService.Buscar(filtro);
            }

            // Usamos una proyección anónima o DTO para mostrar solo lo necesario en la grilla
            // Esto evita problemas con columnas virtuales (Lazy Loading) como 'Compras'
            var listaVisible = lista.Select(c => new
            {
                c.Id,
                c.NombreCompleto,
                c.Documento,
                c.Email,
                Tipo = c.Tipo.ToString()
            }).ToList();

            dgvClientes.DataSource = listaVisible;

            // Ocultar columna ID visualmente pero tenerla disponible
            if (dgvClientes.Columns["Id"] != null)
                dgvClientes.Columns["Id"].Visible = false;
        }

        // --- EVENTOS DE BOTONES ---

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            CargarClientes(txtBuscar.Text);
        }

        private void btnNuevo_Click(object sender, EventArgs e)
        {
            LimpiarFormulario();
            txtNombre.Focus();
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Recoger datos del formulario
                var cliente = new Cliente
                {
                    NombreCompleto = txtNombre.Text.Trim(),
                    Documento = txtDocumento.Text.Trim(),
                    Email = txtEmail.Text.Trim(),
                    Tipo = (TipoCliente)cmbTipoCliente.SelectedItem
                };

                // 2. Decidir si es Crear o Editar
                if (_clienteIdSeleccionado == 0)
                {
                    _clienteService.Crear(cliente);
                    MessageBox.Show("Cliente creado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    cliente.Id = _clienteIdSeleccionado; // Importante asignar el ID para editar
                    _clienteService.Editar(cliente);
                    MessageBox.Show("Cliente actualizado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                // 3. Refrescar y limpiar
                CargarClientes();
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error de Validación", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (_clienteIdSeleccionado == 0) return;

            if (MessageBox.Show("¿Está seguro de eliminar este cliente?", "Confirmar Eliminación",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                try
                {
                    _clienteService.Eliminar(_clienteIdSeleccionado);
                    CargarClientes();
                    LimpiarFormulario();
                    MessageBox.Show("Cliente eliminado.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        // --- SELECCIÓN EN GRILLA ---

        private void dgvClientes_SelectionChanged(object sender, EventArgs e)
        {
            // Si hay filas seleccionadas, cargar datos en el panel derecho
            if (dgvClientes.SelectedRows.Count > 0)
            {
                var row = dgvClientes.SelectedRows[0];
                int id = (int)row.Cells["Id"].Value;

                // Llamamos al servicio para obtener el objeto completo (por si faltan datos en la grilla)
                var cliente = _clienteService.ObtenerPorId(id);

                if (cliente != null)
                {
                    _clienteIdSeleccionado = cliente.Id;

                    // Rellenar campos
                    txtNombre.Text = cliente.NombreCompleto;
                    txtDocumento.Text = cliente.Documento;
                    txtEmail.Text = cliente.Email;
                    cmbTipoCliente.SelectedItem = cliente.Tipo;

                    // Cambiar estado visual
                    lblTituloPanel.Text = "Editar Cliente";
                    btnGuardar.Text = "Actualizar";
                    btnEliminar.Visible = true;
                }
            }
        }

        private void LimpiarFormulario()
        {
            _clienteIdSeleccionado = 0;
            txtNombre.Clear();
            txtDocumento.Clear();
            txtEmail.Clear();
            cmbTipoCliente.SelectedIndex = 0; // Seleccionar el primero por defecto

            // Resetear estado visual
            lblTituloPanel.Text = "Nuevo Cliente";
            btnGuardar.Text = "Guardar";
            btnEliminar.Visible = false;
            dgvClientes.ClearSelection();
        }
    }
}
