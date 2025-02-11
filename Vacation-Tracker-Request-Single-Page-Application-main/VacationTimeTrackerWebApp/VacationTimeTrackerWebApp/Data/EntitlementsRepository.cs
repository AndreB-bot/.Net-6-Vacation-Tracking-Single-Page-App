using VacationTimeTrackerWebApp.Models;

namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// This class is solely responsible for CRUD operations on the Entitlements table.
    /// </summary>
    public class EntitlementsRepository : IDBEntitlementsRepository
    {
        /// <summary>
        /// The Id of the company user.
        /// </summary>
        private const int CompanyUserId = 1;

        /// <summary>
        /// A instance of DBContext.
        /// </summary>
        private DBContext DbContext;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">An insatance of the DBContext class.</param>
        public EntitlementsRepository(DBContext context)
        {
            DbContext = context;
        }

        /// <inheritdoc/>
        public Entitlement Add(Entitlement entitlement)
        {
            DbContext.Entitlements.Add(entitlement);
            DbContext.SaveChanges();

            return entitlement;
        }

        /// <inheritdoc/>
        public bool Delete(Entitlement entitlement)
        {
            if (entitlement == null)
            {
                return false;
            }

            DbContext.Entitlements.Remove(entitlement);
            DbContext.SaveChanges();

            if (GetEntitlement(entitlement.EmployeeId) != null)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public Entitlement GetEntitlement(int employeeId)
        {
            var entitlement = GetAllEntitlements()
                 .Find(x => x.EmployeeId == employeeId);

            if (entitlement != null)
            {
                return entitlement;
            }

            return null;
        }

        /// <inheritdoc/>
        public List<Entitlement> GetAllEntitlements()
        {
            var removedIds = DbContext.RemovedEmployeesIds;

            return DbContext.Entitlements
                .Where(x => x.EmployeeId != CompanyUserId)
                .ToList()
                .FindAll(x => !Array.Exists(removedIds, e => e == x.EmployeeId));
        }

        /// <inheritdoc/>
        public Entitlement Update(Entitlement entitlementChanges)
        {
            DbContext.Attach(entitlementChanges);
            _ = Microsoft.EntityFrameworkCore.EntityState.Modified;
            DbContext.SaveChanges();
            return entitlementChanges;
        }
    }
}
