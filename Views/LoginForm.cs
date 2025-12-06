using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using TechStoreSA.Data;
using TechStoreSA.Models;
using TechStoreSA.Services;

namespace TechStoreSA.Views
{
    public partial class LoginForm : Form
    {
        private readonly TechStoreContext _context;
        private readonly UsuarioService _usuarioService;

        // Propiedad pública para retornar el usuario validado a Program.cs
        public Usuario? UsuarioValidado { get; private set; }

        public LoginForm(TechStoreContext context)
        {
            InitializeComponent();
            _context = context;
            _usuarioService = new UsuarioService(_context);
        }

        // --- FUNCIONALIDAD VISUAL: PLACEHOLDERS ---
        // Simular texto de placeholder que desaparece al hacer clic
        private void txtUser_Enter(object sender, EventArgs e)
        {
            if (txtUser.Text == "USUARIO")
            {
                txtUser.Text = "";
                txtUser.ForeColor = Color.LightGray;
            }
        }

        private void txtUser_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text))
            {
                txtUser.Text = "USUARIO";
                txtUser.ForeColor = Color.DimGray;
            }
        }

        private void txtPass_Enter(object sender, EventArgs e)
        {
            if (txtPass.Text == "CONTRASEÑA")
            {
                txtPass.Text = "";
                txtPass.ForeColor = Color.LightGray;
                txtPass.UseSystemPasswordChar = true; // Ocultar caracteres
            }
        }

        private void txtPass_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPass.Text))
            {
                txtPass.Text = "CONTRASEÑA";
                txtPass.ForeColor = Color.DimGray;
                txtPass.UseSystemPasswordChar = false; // Mostrar texto plano "CONTRASEÑA"
            }
        }

        // --- LÓGICA DE LOGIN ---
        private void btnLogin_Click(object sender, EventArgs e)
        {
            lblErrorMessage.Visible = false;

            if (txtUser.Text == "USUARIO" || txtPass.Text == "CONTRASEÑA")
            {
                MostrarError("Por favor ingrese usuario y contraseña.");
                return;
            }

            try
            {
                var usuario = _usuarioService.Login(txtUser.Text, txtPass.Text);

                if (usuario != null)
                {
                    // Login exitoso
                    this.UsuarioValidado = usuario;
                    this.DialogResult = DialogResult.OK; // Esto indica al Program.cs que todo salió bien
                    this.Close();
                }
                else
                {
                    MostrarError("Usuario o contraseña incorrectos.");
                    txtPass.Clear();
                    txtPass_Leave(null, null); // Restaurar placeholder
                    txtUser.Focus();
                }
            }
            catch (Exception ex)
            {
                MostrarError($"Error de conexión: {ex.Message}");
            }
        }

        private void MostrarError(string msg)
        {
            lblErrorMessage.Text = "    " + msg;
            lblErrorMessage.Visible = true;
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // --- MOVER VENTANA (DRAG FORM) ---
        // Necesario porque FormBorderStyle = None quita la barra nativa para mover
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hwnd, int wmsg, int wparam, int lparam);

        private void LoginForm_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void panelSide_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }
    }
}
