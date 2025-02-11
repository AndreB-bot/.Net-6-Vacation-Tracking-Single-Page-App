using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationTimeTrackerWebApp.Data;
using VacationTimeTrackerWebApp.Models;

/// <summary>
/// Handles POST requests submission from "users" forms .
/// </summary>
namespace VacationTimeTrackerWebApp.Controllers
{
    /// <summary>
    /// Handles are routes related to the user menu options.
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class UsersFormSubmissionController : Controller
    {
        protected const string FailureTitle = "Oops!";
        protected const string SuccessTitle = "Success!";

        /// <summary>
        /// The DBContext instance.
        /// </summary>
        private readonly IDBContext _DbContext;

        /// <summary>
        /// The message to display after a form submission.
        /// </summary>
        public string? ConfirmationMessage { get; set; }

        /// <summary>
        ///  Constructor.
        /// </summary>
        /// <param name="context">The DBContext instance needed for CRUD operations.</param>
        public UsersFormSubmissionController(IDBContext context)
        {
            _DbContext = context;
        }

        /// <summary>
        /// Returns a JSON object with details indicating if a user name or email already exits in the db.
        /// </summary>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure</returns>
        [HttpPost, Route("/existing-user")]
        public IActionResult CheckForExistingUser()
        {
            var nameFromList = Request.Form["nameFromList"];
            var email = Request.Form["email"];
            var name = Request.Form["fullName"];

            if (string.IsNullOrEmpty(nameFromList))
            {
                return DoAddingChecking(name, email);
            }

            return DoUpdatingCheck(name, email, nameFromList);
        }

        /// <summary>
        /// Checks if any user already have the provided unique identities, i.e. name, email.
        /// </summary>
        /// <param name="name">The sumbitted name</param>
        /// <param name="email">The submitted email address</param>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure</returns>
        private IActionResult DoAddingChecking(string name, string email)
        {
            // Emails are unique to each user.
            var existingEmployee = _DbContext.EmployeesRepo.GetEmployeeByEmail(email);
            var removedEmployee = _DbContext.EmployeesRepo.GetRemovedEmployeeByEmail(email);

            if (existingEmployee != null || removedEmployee != null)
            {
                // Provide a request status message.
                ConfirmationMessage = $"A record with {email} already exist.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            existingEmployee = _DbContext.EmployeesRepo.GetEmployeeByName(name);

            if (existingEmployee != null)
            {
                // Provide a request status message.
                ConfirmationMessage = $"A record for {name} already exists.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            return Ok(new { title = "Success" });
        }

        /// <summary>
        /// Checks if any user already have the provided unique identities.
        /// </summary>
        /// <param name="name">The sumbitted name</param>
        /// <param name="email">The submitted email address</param>
        /// <param name="fullName">The original name of the employee to update.</param>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure</returns>
        private IActionResult DoUpdatingCheck(string name, string email, string fullName)
        {
            // Emails are unique to each user.
            var existingEmployee = _DbContext.EmployeesRepo.GetEmployeeByEmail(email);
            var removedEmployee = _DbContext.EmployeesRepo.GetRemovedEmployeeByEmail(email);

            if ((existingEmployee != null && existingEmployee.FullName() != fullName) ||
                 removedEmployee != null
            )
            {
                // Provide a request status message.
                ConfirmationMessage = $"A record with {email} already exist.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            existingEmployee = _DbContext.EmployeesRepo.GetEmployeeByName(name);

            if (existingEmployee != null && existingEmployee.FullName() != fullName)
            {
                // Provide a request status message.
                ConfirmationMessage = $"A record for {name} already exists.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            return Ok(new { title = "Success" });
        }

        /// <summary>
        /// Add a new Employee entry in the database
        /// </summary>
        /// <param name="updatedEmployee">The Employee object with props. mapped to the keys of incoming request form</param>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure.</returns>
        [HttpPost, Route("/add-user")]
        public IActionResult AddUser(Employee newEmployee)
        {
            // Ensure the correct DateTime is parsed.
            var dateString = Request.Form["StartDate"];
            newEmployee.SetStartDateTime(dateString);

            _DbContext.EmployeesRepo.Add(newEmployee);
            var entitlement = new Entitlement();
            entitlement.AddData(newEmployee, Request.Form);
            _DbContext.EntitlementsRepo.Add(entitlement);

            // Provide a request status message.
            ConfirmationMessage = $"{newEmployee.FullName()} was successfully added.";

            return Ok(new { title = SuccessTitle, body = ConfirmationMessage });
        }

        /// <summary>
        /// Removes a user from the database.
        /// </summary>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure.</returns>
        [HttpPost]
        [Route("/remove-user")]
        public IActionResult RemoveUser()
        {
            var name = Request.Form["employee"];
            var employee = _DbContext.EmployeesRepo.GetEmployeeByName(name);

            if (employee == null)
            {
                ConfirmationMessage = "Unfornately this user doesn't exists. Please try again or contact your database admin.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            // Mark the user/emplyee as removed. This allows us to maintain a record of their request
            // per database structure and app requirements.
            employee.RemovalDate = DateTime.Now;
            _DbContext.EmployeesRepo.Update(employee);

            if (employee.IsRemoved())
            {
                ConfirmationMessage = $"{employee.FullName()} was successfully removed";
                return Ok(new { title = SuccessTitle, body = ConfirmationMessage });
            }

            ConfirmationMessage = $"{employee.FullName} was NOT removed";
            return Ok(new { title = FailureTitle, body = ConfirmationMessage });
        }

        /// <summary>
        /// Gets details of an employee entry from the database. The Employee object is fetched by name.
        /// </summary>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure.</returns>
        [HttpPost, Route("/user-details")]
        public IActionResult GetUserDetails()
        {
            var name = Request.Form["name"];
            var employee = _DbContext.EmployeesRepo.GetEmployeeByName(name);

            if (employee == null)
            {
                ConfirmationMessage = "Unfornately this user doesn't exists. Please try again or contact your database admin.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            var entitlements = _DbContext.EntitlementsRepo.GetEntitlement(employee.Id);

            var userDetails = new
            {
                id = employee.Id,
                firstName = employee.FirstName,
                lastName = employee.LastName,
                email = employee.Email,
                startDate = employee.StartDate.ToString("dd-MM-yyyy"),
                accessLevel = entitlements.AccessType,
                vacation = entitlements.EarnedVacationDays,
                sick = entitlements.SickDays
            };

            return Ok(userDetails);
        }

        /// <summary>
        /// Updates an Employee entry in the database
        /// </summary>
        /// <param name="updatedEmployee">The Employee object with props. mapped to the keys of incoming request form</param>
        /// <returns>A ActionResult contain a JSON object with a string that indicates success or failure.</returns>
        [HttpPost, Route("/update-user")]
        public IActionResult UpdateUserDetails(Employee updatedEmployee)
        {
            // Ensure the correct DateTime is parsed.
            var dateString = Request.Form["StartDate"];
            updatedEmployee.SetStartDateTime(dateString);

            var employee = _DbContext.EmployeesRepo.GetEmployeeById(updatedEmployee.Id);

            if (employee == null)
            {
                ConfirmationMessage = "Unfornately this user doesn't exists. Please try again or contact your database admin.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            employee.UpdateDetails(updatedEmployee);
            _DbContext.EmployeesRepo.Update(employee);

            // Update the associated entitlements.
            var entitlements = _DbContext.EntitlementsRepo.GetEntitlement(employee.Id);

            if (entitlements != null)
            {
                entitlements.UpdateData(Request.Form);
                _DbContext.EntitlementsRepo.Update(entitlements);
            }

            ConfirmationMessage = $"Employee details for {employee.FullName()} were successfully updated.";
            return Ok(new { title = SuccessTitle, body = ConfirmationMessage });
        }

        /// <summary>
        /// Returns a html string of options for the employee select lists.
        /// </summary>
        /// <returns>A ActionResult contain a JSON object.</returns>
        [HttpPost, Route("/get-employee-names")]
        public IActionResult GetEmployeeNames()
        {
            var employeesNames = _DbContext.EmployeesRepo.EmployeeNames();
            var html = "<option value=\"\" disabled selected hidden> - Employee - </option>";

            if (employeesNames != null)
            {
                foreach (var employee in employeesNames.OrderBy(key => key.Value))
                {
                    html += $"<option value=\"{employee.Key}\" >{employee.Value}</option>";
                }
            }

            return Ok(new { html = html });
        }
    }
}
