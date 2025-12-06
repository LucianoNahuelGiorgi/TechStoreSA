using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace TechStoreSA.Data
{
    // Esta clase solo la usa el comando Add-Migration / Update-Database
    public class TechStoreContextFactory : IDesignTimeDbContextFactory<TechStoreContext>
    {
        public TechStoreContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TechStoreContext>();

            // CORRECCIÓN: Cambiar el servidor a (localdb)\mssqllocaldb
            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TechStoreDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new TechStoreContext(optionsBuilder.Options);
        }
    }
}
