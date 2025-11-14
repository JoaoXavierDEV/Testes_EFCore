using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WebApplication1.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            // Construir IConfiguration para o design-time
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<AppDbContextFactory>(optional: true)
                .AddEnvironmentVariables() // fallback para CI/CD
                .Build();

            var connectionString =
                configuration.GetConnectionString("DefaultConnection") ??
                configuration["ConnectionStrings:DefaultConnection"] ??
                throw new InvalidOperationException("Connection string 'DefaultConnection' não encontrada nos User Secrets ou variáveis de ambiente.");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}