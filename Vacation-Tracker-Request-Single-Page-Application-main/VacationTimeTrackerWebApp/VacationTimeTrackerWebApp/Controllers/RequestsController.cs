using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VacationTimeTrackerWebApp.Data;
using VacationTimeTrackerWebApp.Models;
using VacationTimeTrackerWebApp.Models.FormModels;

namespace VacationTimeTrackerWebApp.Controllers
{
    /// <summary>
    /// Handles POST requests for approval or rejection of a Request.
    /// </summary>
    [Authorize]
    public class RequestsController : Controller
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
        private string? ConfirmationMessage { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="context">The DBContext instance</param>
        public RequestsController(IDBContext context)
        {
            _DbContext = context;
        }

        /// <summary>
        /// Updates a Request to approved.
        /// </summary>
        /// <param name="requestId">The id of the pending Request.</param>
        /// <param name="action">String incating if the Request is being approved or rejected</param>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure</returns>
        [HttpPost]
        [Route("/process-pending-request"), Authorize(Policy = "AdminOnly")]
        public IActionResult ProcessPendingRequest(int requestId, string action, string comments)
        {
            var request = _DbContext.RequestsRepo.GetRequest(requestId);
            if (request == null)
            {
                ConfirmationMessage = "Unfornately this request doesn't exists. Please try again or contact your database admin.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            // Returns failure if the request was already reviewed.
            if (request.IsReviewed())
            {
                ConfirmationMessage = $"This request was already reviewed and has a status of \"{request.GetStatus()}\".";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            string? dayOrDays;

            if (action == "reject")
            {
                request.Reject();
                request.Comments = comments;

                // Update the action.
                action = "rejected";
            }
            else
            {

                var employee = _DbContext.EmployeesRepo.CreateEmployeeWithEntitlements(request.EmployeeId);
                var availableDays = CalAvailableDays(request, employee.Entitlement);

                if ((availableDays - request.NumberOfDays) < 0)
                {
                    dayOrDays = (availableDays == 1) ? "day" : "days";

                    ConfirmationMessage = $"Sorry, there aren't enough {request.GetTypeString()} Days to cover this request.\n";
                    ConfirmationMessage += $"The user has {availableDays} {dayOrDays} available.";
                    return Ok(new { title = FailureTitle, body = ConfirmationMessage });
                }

                request.Approve();

                // Deduct days from employee's entitlements.
                employee.Entitlement.AdjustDaysTaken(request.GetTypeString(), request.NumberOfDays);
                _DbContext.EntitlementsRepo.Update(employee.Entitlement);

                // Update the action.
                action = "approved";
            }

            // Update "NotifyUser" value and update the request.
            request.Notify();
            _DbContext.RequestsRepo.Update(request);

            // Prepare confirmation message.
            var requestType = request.GetTypeString();
            dayOrDays = (request.NumberOfDays > 1) ? "\tDays were" : "\tDay was";
            requestType += (requestType == "Sick") ? dayOrDays : "";

            ConfirmationMessage = $"{requestType} for {request.Title} {dayOrDays} {action}";
            return Ok(new { title = SuccessTitle, body = ConfirmationMessage, content = GetPendingRequestHtml() });
        }

        /// <summary>
        ///  Helper function that returns availabel days based on the request type.
        /// </summary>
        /// <param name="request">The pending request</param>
        /// <param name="entitlement">The associated employee's entitlement</param>
        /// <returns>The number of avaiable days an employee has for sick/vacation days</returns>
        private static int CalAvailableDays(Request request, Entitlement entitlement)
        {
            var availableDays = entitlement.VacationDaysAvailable;

            if (request.GetTypeString() == "Sick")
            {
                availableDays = entitlement.SickDays - entitlement.SickDaysTaken;
            }

            return availableDays;
        }

        /// <summary>
        /// Returns any pending requests as a JSON object.
        /// </summary>
        /// <returns>A ActionResult contain a JSON object with html content or empty string</returns>
        [HttpPost]
        [Route("/get-pending-requests"), Authorize(Policy = "AdminOnly")]
        public IActionResult GetPendingRequests()
        {
            return Ok(new { content = GetPendingRequestHtml() });
        }

        /// <summary>
        /// Returns the updated html for the requests menu.
        /// </summary>
        /// <returns>The html content containing details of the remaing pending requests.</returns>
        [Authorize(Policy = "AdminOnly")]
        private object GetPendingRequestHtml()
        {
            var html = "";
            var pendingRequests = _DbContext.RequestsRepo.GetAllPendingRequestsForDisplay();
            var count = pendingRequests.Count();

            foreach (var request in pendingRequests)
            {
                var typeIndicator = (request.GetTypeString() == "Vacation") ? "bg-success" : "bg-sick";
                var dayOrDays = (request.NumberOfDays > 1) ? "Days" : "Day";

                var typeTitle = request.GetTypeString();
                typeTitle += (typeTitle == "Sick") ? $"\t{dayOrDays}" : "";

                var inThePastAlert = "";

                if (request.EndDate < DateTime.Now && request.GetTypeString() == "Vacation")
                {
                    inThePastAlert = "<div class=\"rounded-3 fw-bold py-1 alert-danger\" role=\"alert\">In the past</div>";
                }

                html += "<div class=\"row justify-content-center mt-2\">"
                                + "<div class=\"content d-flex shadow\">"
                                    + "<div class=\"card bg-light\" style=\"max-width: 18rem;\">"
                                        + "<div class=\"card-body just-card-body employee-card-body\">"
                                            + $"<h5 class=\"card-title\">{request.Title}</h5>"
                                        + "</div>"
                                    + "</div>"
                                    + $"<div class=\"card text-white {typeIndicator}\" style=\"max-width: 18rem;\">"
                                        + "<div class=\"card-body text-center\">"
                                            + $"<h5 class=\"card-title\">{typeTitle} Request</h5>"
                                            + "<p class=\"card-text\">"
                                                + request.StartDate.ToString("dd\\/MMM\\/yyyy")
                                                + "<br />To<br />"
                                                + request.EndDate.ToString("dd\\/MMM\\/yyyy")
                                            + "</p>"
                                        + "</div>"
                                    + "</div>"
                                    + "<div class=\"card bg-light\" style=\"max-width: 18rem;\">"
                                        + "<div class=\"card-body just-card-body flex-column\">"
                                            + $"<h5 class=\"card-title\">{request.NumberOfDays} {dayOrDays}</h5>"
                                            + inThePastAlert
                                        + "</div>"
                                    + "</div>"
                                    + "<div class=\"card text-white bg-light\" style=\"max-width: 18rem;\">"
                                        + "<div class=\"card-body\" style=\"display: inline-grid;row-gap: 0.3rem;\">"
                                            + $"<button class=\"btn btn-success fw-bold {request.GetTypeString().ToLower()}-approve-btn\" data-request-id=\"{request.RequestId}\">Approve</button>"
                                            + $"<button class=\"btn btn-reject fw-bold {request.GetTypeString().ToLower()}-reject-btn\" data-request-id=\"{request.RequestId}\">Reject</button>"
                                            + $"<button class=\"btn btn-primary fw-bold {request.GetTypeString().ToLower()}-goto-btn\" data-request-id=\"{request.RequestId}\"\t"
                                            + $"data-start-date=\"{request.StartDate.ToShortDateString()}\">View In Calendar</button>"
                                        + "</div>"
                                    + "</div>"
                                + "</div>"
                            + "</div>";
            }

            return new { html = html, count = count };
        }

        /// <summary>
        /// Returns any new notification to user sign last logged in.
        /// </summary>
        /// <returns>A ActionResult contain a JSON object with html content or empty string</returns>
        [HttpPost]
        [Route("/get-notifications")]
        public IActionResult GetNotifications()
        {
            var userEmail = this.User.Claims.FirstOrDefault(
              x => x.Type == ClaimTypes.Email
           )?.Value;

            var user = _DbContext.EmployeesRepo.GetEmployeeByEmail(userEmail);

            if (user == null)
            {
                return Ok();
            }

            var requests = _DbContext.RequestsRepo.GetAllReviewedRequests(user.Id);
            var html = "";

            foreach (var request in requests)
            {
                var typeIndicator = (request.GetTypeString() == "Vacation") ? "bg-success" : "bg-sick";
                var dayOrDays = (request.NumberOfDays > 1) ? "Days" : "Day";

                var typeTitle = request.GetTypeString();
                typeTitle += (typeTitle == "Sick") ? $"\t{dayOrDays}" : "";

                var commentsDropdown = "";

                if (request.Comments != null)
                {
                    commentsDropdown = $"<button type=\"button\" class=\"btn-info-custom w-100 \" id=\"comment-{request.RequestId}\""
                        + $" data-bs-toggle=\"dropdown\" aria-expanded=\"false\">View Comments</button>"
                        + $"<ul class=\"dropdown-menu w-75\" aria-labelledby=\"comment-{request.RequestId}\">"
                        + $"<li style=\"padding: 0 1rem;\">{request.Comments}</li></ul>";
                }

                html += "<div class=\"row justify-content-center mt-2 w-75\">"
                             + "<div class=\"content d-flex shadow\">"
                                 + $"<div class=\"card text-white {typeIndicator} first-card card-40\" style=\"max-width: 18rem;\">"
                                     + "<div class=\"card-body text-center\">"
                                         + $"<h5 class=\"card-title\">{typeTitle} Request</h5>"
                                         + "<p class=\"card-text\">"
                                             + request.StartDate.ToString("dd\\/MMM\\/yyyy")
                                             + "<br />To<br />"
                                             + request.EndDate.ToString("dd\\/MMM\\/yyyy")
                                         + "</p>"
                                     + "</div>"
                                 + "</div>"
                                 + "<div class=\"card bg-light card-20\" style=\"max-width: 18rem;\">"
                                     + "<div class=\"card-body d-flex align-items-center justify-content-center\">"
                                         + $"<h5 class=\"card-title\">{request.NumberOfDays} {dayOrDays}</h5>"
                                     + "</div>"
                                 + "</div>";

                if (request.IsApproved())
                {
                    html += "<div class=\"card text-white approved d-flex align-items-center justify-content-center card-40\" style=\"max-width: 18rem;\">"
                            + "<i class=\"bi bi-patch-check text-white display-3\"></i>"
                            + "<h3>Approved</h3>"
                            + "</div>";
                }
                else
                {
                    html += "<div class=\"card text-white declined d-flex align-items-center justify-content-center card-40\" style=\"max-width: 18rem;\">"
                            + "<i class=\"fa fa-ban text-white display-4\"></i>"
                            + "<h3>Declined</h3>"
                            + "</div>";
                }

                html += $"</div><div class=\"w-100 px-0\">{commentsDropdown}</div></div>";
            }

            return Ok(new { content = html });
        }
    }

}
