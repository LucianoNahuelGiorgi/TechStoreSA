using Microsoft.EntityFrameworkCore;
using TechStoreSA.Data;
using TechStoreSA.Models;
using TechStoreSA.Views;

namespace TechStoreSA
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // 1. Configurar la conexión a la Base de Datos
            var optionsBuilder = new DbContextOptionsBuilder<TechStoreContext>();

            // IMPORTANTE: Usa esta cadena de conexión que es compatible con la instalación por defecto de Visual Studio
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TechStoreDB;Trusted_Connection=True;TrustServerCertificate=True;");

            using (var context = new TechStoreContext(optionsBuilder.Options))
            {
                try
                {
                    // 2. ASEGURAR QUE LA BASE DE DATOS EXISTA
                    // Esto creará la BD y el usuario 'admin' (clave '1234') definido en tu TechStoreContext
                    context.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al conectar con la Base de Datos: {ex.Message}", "Error Crítico", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return; // Salir si no hay base de datos
                }

                // 3. INICIAR EL LOGIN
                LoginForm loginForm = new LoginForm(context);

                // Mostramos el Login como ventana modal (el código se detiene aquí hasta que se cierre)
                DialogResult resultado = loginForm.ShowDialog();

                // 4. VERIFICAR RESULTADO
                if (resultado == DialogResult.OK)
                {
                    // Si el login fue exitoso, obtenemos el usuario real de la BD
                    var usuarioLogueado = loginForm.UsuarioValidado;

                    if (usuarioLogueado != null)
                    {
                        // Arrancamos la aplicación principal
                        Application.Run(new MainForm(usuarioLogueado, context));
                    }
                }
                else
                {
                    // Si el usuario cerró la ventana de login o canceló, la app termina aquí.
                    Application.Exit();
                }
            }
        }
    }
}
