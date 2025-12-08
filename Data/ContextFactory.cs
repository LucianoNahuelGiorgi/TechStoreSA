using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TechStoreSA.Data
{
    // Esta clase solo la usa el comando Add-Migration / Update-Database
    public class TechStoreContextFactory : IDesignTimeDbContextFactory<TechStoreContext>
    {
        public TechStoreContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<TechStoreContext>();

            optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=TechStoreDB;Trusted_Connection=True;TrustServerCertificate=True;");

            return new TechStoreContext(optionsBuilder.Options);
        }
    }
}
