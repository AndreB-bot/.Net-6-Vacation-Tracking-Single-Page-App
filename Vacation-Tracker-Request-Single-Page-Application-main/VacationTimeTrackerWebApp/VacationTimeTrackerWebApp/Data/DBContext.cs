using Microsoft.EntityFrameworkCore;
using VacationTimeTrackerWebApp.Models;

namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// Creates an database controller for CRUD operations.
    /// </summary>
    public class DBContext : DbContext, IDBContext
    {
        // DbSet of models representing the DB tables:    
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Request> Requests { get; set; }
        public DbSet<Entitlement> Entitlements { get; set; }

        // The Repositories:
        public IDBEmployeesRepository EmployeesRepo { get; set; }
        public IDBRequestsRepository RequestsRepo { get; set; }
        public IDBEntitlementsRepository EntitlementsRepo { get; set; }

        // Helper properties.

        /// <summary>
        /// An array of ids belonging to employee records marked as "removed".
        /// </summary>
        public int[]? RemovedEmployeesIds { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="options">The options to be used by a Microsoft.EntityFrameworkCore.DbContext.</param>
        public DBContext(DbContextOptions<DBContext> options)
         : base(options)
        {
            RequestsRepo = new RequestsRepository(this);
            EmployeesRepo = new EmployeesRepository(this);
            EntitlementsRepo = new EntitlementsRepository(this);
            RemovedEmployeesIds = EmployeesRepo.GetRemovedEmployeesIds();
        }

        /// <inheritdoc />
        public List<ReportEntry> CreateReportEntries()
        {
            var employees = EmployeesRepo.GetAllEmployees();
            var entitlements = EntitlementsRepo.GetAllEntitlements().ToList();
            var requests = RequestsRepo.GetAllApprovedRequests();

            var reportEntries = new List<ReportEntry>();

            foreach (var employee in employees)
            {
                var entitlement = entitlements.Find(x => x.EmployeeId == employee.Id);
                reportEntries.Add(new ReportEntry(employee, entitlement, requests));
            }

            return reportEntries;
        }
    }
}
