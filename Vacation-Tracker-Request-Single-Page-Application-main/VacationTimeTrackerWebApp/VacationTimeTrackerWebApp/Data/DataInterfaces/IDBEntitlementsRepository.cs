using VacationTimeTrackerWebApp.Models;

namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// A specialised class that is responsible for CRUD operations on the Entitlements table.
    /// </summary>
    public interface IDBEntitlementsRepository : IDBRepository<Entitlement>
    {
        /// <summary>
        /// Returns an Entitlement entity fetch by id.
        /// </summary>
        /// <param name="employeeId">Represents the associated employee id</param>
        /// <returns>An Entitlement object</returns>
        public Entitlement GetEntitlement(int employeeId);

        /// <summary>
        /// Returns an unsorted List of Entitlements.
        /// </summary>
        /// <returns>An IEnumberable containing Entitlement objects</returns>
        public List<Entitlement> GetAllEntitlements();
    }
}
