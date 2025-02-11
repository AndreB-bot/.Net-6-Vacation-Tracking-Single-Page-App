using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using VacationTimeTrackerWebApp.Data;
using VacationTimeTrackerWebApp.Models;

namespace VacationTimeTrackerWebApp.Pages
{
    [Authorize("AuthorizedUser")]
    public class VacationAppModel : PageModel
    {
        /// <summary>
        /// The DBContext instance at runtime.
        /// </summary>
        private readonly IDBContext _DbContext;

        /// <summary>
        /// List of employees' name.
        /// </summary>
        public Dictionary<string, string>? EmployeesNames;

        /// <summary>
        /// List of ReportEntry objects.
        /// </summary>
        public List<ReportEntry> ReportEntries;

        /// <summary>
        /// List of Request objects with "pending" statuses.
        /// </summary>
        public List<Request> PendingRequests { get; private set; }

        /// <summary>
        /// List of reviewed Request objects with either approved/rejected (denied) statuses.
        /// </summary>
        public List<Request> ReviewedRequests { get; private set; }

        /// <summary>
        /// The currently logged in Employee/User.
        /// </summary>
        public Employee Employee { get; private set; }

        /// <summary>
        /// The number of pending requests as a string, if any.
        /// </summary>
        public string NumRequestAsString { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The DBContext instance.</param>
        public VacationAppModel(IDBContext context)
        {
            _DbContext = context;
        }

        /// <summary>
        /// Function call for GET request made against the VacationApp page.
        /// </summary>
        public void OnGet()
        {
            var userEmail = this.User.Claims.FirstOrDefault(
               x => x.Type == ClaimTypes.Email
            )?.Value;

            var employeesRepo = _DbContext.EmployeesRepo;
            Employee = employeesRepo.CreateEmployeeWithEntitlements(userEmail);

            if (Employee.IsAdmin())
            {
                EmployeesNames = employeesRepo.EmployeeNames();
                ReportEntries = _DbContext.CreateReportEntries();
                PendingRequests = _DbContext.RequestsRepo.GetAllPendingRequestsForDisplay();
                NumRequestAsString = _DbContext.RequestsRepo
                                     .GetAllPendingRequests()
                                     .Count()
                                     .ToString();
            }
            else
            {
                ReviewedRequests = _DbContext.RequestsRepo.GetAllReviewedRequests(Employee.Id);
            }
        }
    }
}
