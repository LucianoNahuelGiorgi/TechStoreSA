using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using TechStoreSA.Data;
using TechStoreSA.Models;

namespace TechStoreSA.Views
{
    public partial class MainForm : Form
    {
        // Variables globales del formulario
        private Button? currentButton;
        private Form? activeForm;
        private readonly Usuario _usuarioActual;
        private readonly TechStoreContext _context;

        public MainForm(Usuario usuario, TechStoreContext context)
        {
            InitializeComponent();
            _usuarioActual = usuario;
            _context = context;

            ConfigurarPermisos();
            CargarDatosUsuario();
        }

        // Configuración inicial visual
        private void CargarDatosUsuario()
        {
            lblUsuarioActual.Text = $"Usuario: {_usuarioActual.NombreCompleto} | Rol: {(_usuarioActual.EsAdministrador ? "Admin" : "Vendedor")}";
        }

        // Restringir acceso según rol
        private void ConfigurarPermisos()
        {
            if (!_usuarioActual.EsAdministrador)
            {
                // Si no es admin, ocultamos el acceso a gestión de usuarios
                btnUsuarios.Visible = false;
                // Opcional: Ocultar reportes sensibles
                // btnReportes.Visible = false; 
            }
        }

        // Método genérico para resaltar el botón seleccionado
        private void ActivateButton(object btnSender)
        {
            if (btnSender != null)
            {
                if (currentButton != (Button)btnSender)
                {
                    DisableButton();
                    currentButton = (Button)btnSender;
                    currentButton.BackColor = Color.FromArgb(70, 70, 90); // Color activo
                    currentButton.ForeColor = Color.White;
                    currentButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
                    panelTitleBar.BackColor = Color.FromArgb(0, 150, 136); // Color del tema
                    lblTitle.Text = currentButton.Text.Trim().ToUpper();
                }
            }
        }

        // Método para regresar los botones a su estado original
        private void DisableButton()
        {
            foreach (Control previousBtn in panelMenu.Controls)
            {
                if (previousBtn.GetType() == typeof(Button))
                {
                    previousBtn.BackColor = Color.FromArgb(51, 51, 76);
                    previousBtn.ForeColor = Color.Gainsboro;
                    previousBtn.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
                }
            }
        }

        // MOTOR DE NAVEGACIÓN: Abre formularios hijos dentro del panel
        private void OpenChildForm(Form childForm, object btnSender)
        {
            if (activeForm != null)
                activeForm.Close();

            ActivateButton(btnSender);
            activeForm = childForm;

            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.Dock = DockStyle.Fill;

            this.panelDesktop.Controls.Add(childForm);
            this.panelDesktop.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }

        // --- EVENTOS DE BOTONES (MENÚ) ---

        private void btnVentas_Click(object sender, EventArgs e)
        {
            OpenChildForm(new VentasForm(_usuarioActual, _context), sender);

            ActivateButton(sender);
        }

        private void btnProductos_Click(object sender, EventArgs e)
        {
            OpenChildForm(new ProductosForm(_context), sender);

            ActivateButton(sender);
        }

        private void btnClientes_Click(object sender, EventArgs e)
        {
            OpenChildForm(new ClientesForm(_context), sender);

            ActivateButton(sender);
        }

        private void btnReportes_Click(object sender, EventArgs e)
        {
            OpenChildForm(new ReportesForm(_context), sender);

            ActivateButton(sender);
        }

        private void btnUsuarios_Click(object sender, EventArgs e)
        {
            OpenChildForm(new UsuariosForm(_context), sender);

            ActivateButton(sender);
        }

        private void btnSalir_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("¿Está seguro que desea cerrar sesión?", "Cerrar Sesión",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close(); // Cierra el Main, regresando al Program.cs (donde volverá al Login)
            }
        }
    }
}
