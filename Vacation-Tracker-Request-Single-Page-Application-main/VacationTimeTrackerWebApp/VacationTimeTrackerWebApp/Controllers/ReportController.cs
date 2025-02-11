using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacationTimeTrackerWebApp.Data;

namespace VacationTimeTrackerWebApp.Controllers
{
    /// <summary>
    /// Responsible for routes associated with the Report menu.
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class ReportController : Controller
    {

        /// <summary>
        /// The DBContext instance.
        /// </summary>
        private readonly IDBContext _DbContext;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ReportController(DBContext context)
        {
            _DbContext = context;
        }

        /// <summary>
        /// Returns a List of ReportEntry objects with the most recent infomation from the db.
        /// </summary>
        /// <returns></returns>
        [HttpGet, Route("/get-report-entries")]
        public IActionResult GetUpdatedReportEntries()
        {
            var reportEntries = _DbContext.CreateReportEntries();
            var content = "";

            foreach (var entry in reportEntries)
            {
                content += "<tr>"
                + $"<td class=\"name\">{entry.EmployeeName}</td>"
                + $"<td class=\"text-center\">{entry.VacationRollover}</td>"
                + $"<td class=\"text-center\">{entry.VacationDaysTaken}</td>"
                + $"<td class=\"text-center\">{entry.UpcomingVacationDays}</td>"
                + $"<td class=\"text-center\">{entry.VacationDaysAvailable}</td>"
                + $"<td class=\"text-center\">{entry.SickDaysRemaining}</td>"
                + $"<td class=\"text-center\">{entry.SickDaysTaken}</td>"
                + "</tr>";
            }

            return Ok(new { content = content });
        }
    }
}
