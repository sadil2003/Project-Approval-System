using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using PAS_BlindMatching.Models;

namespace PAS_BlindMatching.Data
{
    /// <summary>
    /// Design-time factory for AppDbContext.
    /// EF Core will use this when running migrations or update-database.
    /// This avoids loading appsettings.json at design time.
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // 1️⃣ Create options builder
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            // 2️⃣ Hardcode connection string
            // Change Server=.; if you use SQL Express (e.g., .\SQLEXPRESS)
            optionsBuilder.UseSqlServer("Server=MIHISARA\\SQLEXPRESS;Database=PAS_DB;Trusted_Connection=True;TrustServerCertificate=True;");

            // 3️⃣ Return new AppDbContext with options
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}