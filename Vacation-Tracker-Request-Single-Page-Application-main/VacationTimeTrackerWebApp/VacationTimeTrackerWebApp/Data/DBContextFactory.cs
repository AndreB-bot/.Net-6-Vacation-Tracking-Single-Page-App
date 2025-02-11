using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// Provides the DBContext object need for database entity migration.
    /// </summary>
    public class DBContextFactory : IDesignTimeDbContextFactory<DBContext>
    {
        /// <summary>
        /// Creates an instance of the DBContext class.
        /// </summary>
        /// <param name="args">Arguments provided by the design-time service.</param>
        /// <returns>A DBContext object</returns>
        public DBContext CreateDbContext(string[]? args = null)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.LaunchSettings.json")
            .Build();

            var builder = new DbContextOptionsBuilder<DBContext>();
            var connectionString = configuration.GetConnectionString("Default");

            builder.UseMySQL(connectionString);

            return new DBContext(builder.Options);
        }

    }

}
