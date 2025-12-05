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

            // 1. Configurar la Base de Datos (Contexto)
            // Ajusta "Server=..." con tu cadena de conexión real de SQL Server
            var optionsBuilder = new DbContextOptionsBuilder<TechStoreContext>();
            optionsBuilder.UseSqlServer("Server=.;Database=TechStoreDB;Trusted_Connection=True;TrustServerCertificate=True;");

            using (var context = new TechStoreContext(optionsBuilder.Options))
            {
                // 2. Crear un Usuario "Fake" para pruebas (ya que aún no tenemos pantalla de Login)
                // Esto simula que alguien se logueó exitosamente.
                var usuarioPrueba = new Usuario
                {
                    Id = 1,
                    NombreCompleto = "Administrador Sistema",
                    NombreUsuario = "admin",
                    EsAdministrador = true // Cambia a false para probar la vista de Vendedor
                };

                // 3. Iniciar el MainForm pasando los objetos que requiere
                // Nota: Cambié "new Form1()" por "new MainForm(...)" porque así se llama tu clase
                Application.Run(new MainForm(usuarioPrueba, context));
            }
        }
    }
}
