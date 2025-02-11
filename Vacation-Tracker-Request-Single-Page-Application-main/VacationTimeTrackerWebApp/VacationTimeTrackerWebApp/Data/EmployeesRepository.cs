using Microsoft.EntityFrameworkCore;
using VacationTimeTrackerWebApp.Models;

namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// This class is solely responsible for CRUD operations on the Employees table.
    /// </summary>
    public class EmployeesRepository : IDBEmployeesRepository
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
        /// Constructor.
        /// </summary>
        /// <param name="context">The DBConext entity needed for CRUD operations.</param>
        public EmployeesRepository(DBContext context)
        {
            DbContext = context;
        }

        /// <inheritdoc/>
        public Employee Add(Employee employee)
        {
            DbContext.Employees.Add(employee);
            DbContext.SaveChanges();

            return employee;
        }

        /// <inheritdoc/>
        public bool Delete(Employee employee)
        {
            if (employee == null)
            {
                return false;
            }

            DbContext.Employees.Remove(employee);
            DbContext.SaveChanges();

            if (GetEmployeeById(employee.Id) != null)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public Dictionary<string, string> EmployeeNames()
        {
            var names = new Dictionary<string, string>();
            var employees = DbContext.EmployeesRepo.GetAllEmployees();

            foreach (var employee in employees)
            {
                names.Add(
                    employee.FullName(),
                    $"{employee.LastName}, {employee.FirstName}"
                    );
            }

            return names;
        }

        /// <inheritdoc/>
        public Employee Update(Employee employeeChanges)
        {
            DbContext.Attach(employeeChanges);
            _ = Microsoft.EntityFrameworkCore.EntityState.Modified;
            DbContext.SaveChanges();
            return employeeChanges;
        }

        /// <inheritdoc/>
        public List<Employee> GetAllEmployees()
        {
            return DbContext.Employees.Where(x =>
            (x.Id != CompanyUserId && x.RemovalDate == null)
            )
            .ToList();
        }

        /// <inheritdoc/>
        public int[]? GetRemovedEmployeesIds()
        {
            return GetRemovedEmployees()
                .Select(x => x.Id)
                .ToArray();
        }

        /// <inheritdoc/>
        public List<Employee> GetRemovedEmployees()
        {
            return DbContext.Employees.Where(
                x => (x.Id != CompanyUserId && x.RemovalDate != null)
                ).ToList();
        }

        /// <inheritdoc/>
        public Employee? GetRemovedEmployeeByEmail(string email)
        {
            return GetRemovedEmployees().Find(x => x.Email == email);
        }

        /// <inheritdoc/>
        public Employee GetEmployeeByEmail(string email)
        {
            var employee = GetAllEmployees()
                .ToList()
                .Find(x => x.Email == email);

            if (employee != null)
            {
                return employee;
            }

            return null;
        }

        /// <inheritdoc/>
        public Employee GetEmployeeById(int id)
        {
            var employee = GetAllEmployees()
                .ToList()
                .Find(x => x.Id == id);

            if (employee != null)
            {
                return employee;
            }

            return null;
        }

        /// <inheritdoc/>
        public Employee GetEmployeeByName(string employeeName)
        {
            var employee = GetAllEmployees()
                .ToList()
                .Find(x => $"{x.FirstName} {x.LastName}" == employeeName);

            if (employee != null)
            {
                return employee;
            }

            return null;
        }

        /// <summary>
        /// Creates an Employee object and sets its Entitlement.
        /// </summary>
        /// <returns>An Employee object</returns>
        public Employee CreateEmployeeWithEntitlements(string email)
        {
            var employee = GetEmployeeByEmail(email);

            if (employee != null)
            {
                var entitlement = DbContext.EntitlementsRepo.GetEntitlement(employee.Id);
                employee.SetEntitlement(entitlement);
            }

            return employee;
        }

        /// <summary>
        /// Creates an Employee object and sets its Entitlement.
        /// </summary>
        /// <returns>An Employee object</returns>
        public Employee CreateEmployeeWithEntitlements(int id)
        {
            var employee = GetEmployeeById(id);

            if (employee != null)
            {
                var entitlement = DbContext.EntitlementsRepo.GetEntitlement(employee.Id);
                employee.SetEntitlement(entitlement);
            }

            return employee;
        }
    }
}
