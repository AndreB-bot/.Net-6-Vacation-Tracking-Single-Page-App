using VacationTimeTrackerWebApp.Models;

namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// A specialised class that is responsible for CRUD operations on the Employees table.
    /// </summary>
    public interface IDBEmployeesRepository : IDBRepository<Employee>
    {
        /// <summary>
        /// Fetches an Employee object by email.
        /// </summary>
        /// <param name="email">Represents the employee email.</param>
        /// <returns>An Employee object</returns>
        public Employee GetEmployeeByEmail(string email);

        /// <summary>
        /// Returns an Employee entity fetched by id.
        /// </summary>
        /// <param name="id">Represenst the employee id (int)</param>
        /// <returns>An Employee object</returns>
        public Employee GetEmployeeById(int id);

        /// <summary>
        /// Returns an List of active employees.
        /// </summary>
        /// <returns>A List containing Employee objects</returns>
        public List<Employee>? GetAllEmployees();

        /// <summary>
        /// Returns a list of employees' names, sorted by surname.
        /// </summary>
        /// <returns>Returns a Dictionary of employees' name</returns>
        public Dictionary<string, string>? EmployeeNames();

        /// <summary>
        /// Returns an Employee object, fetched by full name.
        /// </summary>
        /// <param name="employeeName">Represents the name of the employee</param>
        /// <returns></returns>
        public Employee GetEmployeeByName(string employeeName);

        /// <summary>
        /// Creates an Employee object and sets its Entitlement.
        /// </summary>
        /// <param name="email">The email of the existing employee.</param>
        /// <returns>An Employee object</returns>
        public Employee CreateEmployeeWithEntitlements(string email);

        /// <summary>
        /// Creates an Employee object and sets its Entitlement.
        /// </summary>
        /// <param name="id">The id of the existing employee.</param>
        /// <returns>An Employee object</returns>
        public Employee CreateEmployeeWithEntitlements(int id);


        /// <summary>
        /// Returns an array of removed employees ids.
        /// </summary>
        /// <returns>An array of removed employees ids.</returns>
        public int[]? GetRemovedEmployeesIds();

        /// <summary>
        /// Returns a List of removed Employees.
        /// </summary>
        /// <returns>A List of removed Employees.</returns>
        public List<Employee> GetRemovedEmployees();

        /// <summary>
        /// Returns a removed Employee.
        /// </summary>
        /// <returns>An Employee entity marked as removed</returns>
        public Employee? GetRemovedEmployeeByEmail(string email);
    }
}
