using VacationTimeTrackerWebApp.Models;

namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// Provides an iterface for the DBContext concrete class.
    /// </summary>
    public interface IDBContext
    {
        // The Repositories.
        public IDBEmployeesRepository EmployeesRepo { get; set; }

        public IDBRequestsRepository RequestsRepo { get; set; }

        public IDBEntitlementsRepository EntitlementsRepo { get; set; }

        /// <summary>
        /// Retruns a List of ReportEntry objects.
        /// </summary>
        /// <returns>A List of ReportEntry entities</returns>
        public List<ReportEntry> CreateReportEntries();

        /// <summary>
        /// An array of employee records marked as "Deleted". 
        /// </summary>
        public int[]? RemovedEmployeesIds { get; set; }
    }
}
