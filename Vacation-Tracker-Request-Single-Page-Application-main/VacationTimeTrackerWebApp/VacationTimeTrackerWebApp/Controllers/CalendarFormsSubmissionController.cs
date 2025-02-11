using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VacationTimeTrackerWebApp.Data;
using VacationTimeTrackerWebApp.Models;
using VacationTimeTrackerWebApp.Models.FormModels;

namespace VacationTimeTrackerWebApp.Controllers
{
    /// <summary>
    ///  Handles POST requests submission from "calendar" forms.
    /// </summary>
    [Authorize]
    public class CalendarFormsSubmissionController : ControllerBase
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
        public CalendarFormsSubmissionController(IDBContext context)
        {
            _DbContext = context;
        }

        /// <summary>
        /// Handles POST request to api/getEvents.
        /// This function converts all approved and pending requests from the db to Event objects.
        /// </summary>
        /// <returns>An ActionResult object with containing Event objects</returns>
        [HttpPost, Route("api/getEvents")]
        public IActionResult GetEvents()
        {
            // First, get the access type of the user.
            var accessType = this.User.Claims.FirstOrDefault(
              x => x.Type == "AccessType"
           )?.Value;

            List<Event> events = new List<Event>();
            AddAllApprovedAndPendingRequests(events, accessType);

            return Ok(events);
        }

        /// <summary>
        /// Helper function that adds all approved and pending requests.
        /// </summary>
        /// <param name="events">The List of Events for populating the Calendar UI</param>
        private void AddAllApprovedAndPendingRequests(List<Event> events, string accessType = "User")
        {
            var requestsRepo = _DbContext.RequestsRepo;
            var requests = requestsRepo.GetAllApprovedRequests();

            if (accessType == "Admin")
            {
                requests.AddRange(requestsRepo.GetAllPendingRequests());
            }
            else
            {
                requests.AddRange(GetUserPendingRequests());
            }

            foreach (var request in requests)
            {
                var numStatHolidays = 0;

                // Check for overlapping stat holidays for vacation requests.
                if (request.GetTypeString() == "Vacation")
                {
                    numStatHolidays = requests.FindAll(x =>
                    {
                        return x.GetTypeString() == "Stat" &&
                               x.StartDate >= request.StartDate &&
                               x.EndDate <= request.EndDate &&
                               !x.StartsOnWeekend();
                    })
                    .Count();
                }

                events.Add(new Event(request, request.Title, numStatHolidays));
            }
        }

        /// <summary>
        /// Helper function that gets the current user's/employee's pending requests.
        /// </summary>
        /// <returns>A List of Result objects (if any) associated with the current user</returns>
        private List<Request> GetUserPendingRequests()
        {
            var userEmail = this.User.Claims.FirstOrDefault(
               x => x.Type == ClaimTypes.Email
            )?.Value;

            var employee = _DbContext.EmployeesRepo.GetEmployeeByEmail(userEmail);
            return _DbContext.RequestsRepo.GetUserPendingRequests(employee.Id);
        }

        /// <summary>
        ///  Process a "Add timeoff" request from the UI made by Admin.
        /// </summary>
        /// <param name="incomingRequest">A SubmittedRequest object mapping to the incoming request form.</param>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure</returns>
        [HttpPost, Route("/request-submission")]
        public IActionResult RequestSubmission(SubmittedRequest incomingRequest)
        {
            // Check if request is vacation/sick and if it's starting on a weekend.
            if ((incomingRequest.Type == "vacation" || incomingRequest.Type == "sick")
                 && incomingRequest.StartsOnWeekend()
            )
            {
                return Ok(new { title = FailureTitle, body = "Vacation/Sick days cannot start on a weekend." });
            }

            // Next, see if the current user has admin privillege.
            var accessType = this.User.Claims.FirstOrDefault(
               x => x.Type == "AccessType"
            )?.Value;

            Request request = null;
            var requestsRepo = _DbContext.RequestsRepo;

            if (accessType == "Admin")
            {
                if (incomingRequest.Type == "stat" || incomingRequest.Type == "company")
                {
                    return ProcessStatCompanyDay(incomingRequest, out request, requestsRepo);
                }

                return ProcessVacationSickDayRequest(incomingRequest, ref request, requestsRepo);
            }

            // Continue to process the request as an employee submission.
            var userEmail = this.User.Claims.FirstOrDefault(
               x => x.Type == ClaimTypes.Email
            )?.Value;

            var currentUser = _DbContext.EmployeesRepo.CreateEmployeeWithEntitlements(userEmail);

            return PocessUserSubmittedRequest(incomingRequest, ref request, requestsRepo, currentUser);
        }

        /// <summary>
        /// Helper function to process stat or company day requests made by an Admin user.
        /// </summary>
        /// <param name="incomingRequest">A SubmittedRequest mapped to the Form object.</param>
        /// <param name="request">Request object containing no data</param>
        /// <param name="requestsRepo">A specialised class that is responsible for CRUD operations on the Requests table.</param>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure</returns>
        private IActionResult PocessUserSubmittedRequest(
            SubmittedRequest incomingRequest,
            ref Request? request,
            IDBRequestsRepository requestsRepo,
            Employee employee
        )
        {
            // Create the Request object.
            request = requestsRepo.CreateRequest(incomingRequest, employee.FullName(), employee.Id);

            var totalDays = request.NumberOfDays;
            var availableDays = CalAvailableDays(request, employee.Entitlement);
            var potentialAvailableDays = availableDays;
            var numPendingDays = 0;

            if (availableDays > 0)
            {
                var pendingRequests = requestsRepo.GetUserPendingRequests(
                    employee.Id, incomingRequest.GetTypeString()
                    );

                foreach (var req in pendingRequests)
                {
                    // Deduct days that are already pending approval.
                    // This stops user from making too many unnecessary requests.
                    potentialAvailableDays -= req.NumberOfDays;

                    numPendingDays += req.NumberOfDays;
                }
            }

            // Check if the request can be taken.
            if (availableDays == 0 || (potentialAvailableDays - totalDays) < 0)
            {
                var dayOrDays = (availableDays == 1) ? "day" : "days";

                ConfirmationMessage = $"Sorry, there aren't enough {request.GetTypeString()} Days to cover this request.\n";
                ConfirmationMessage += $"You have {availableDays} {dayOrDays} available";

                if (numPendingDays > 0)
                {
                    var verb = numPendingDays > 1 ? "are" : "is";
                    ConfirmationMessage += $", of which {numPendingDays} {verb} pending approval.";
                }

                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            requestsRepo.Add(request);

            ConfirmationMessage = "Thanks for your submission. Your request is now pending approval.";
            return Ok(new { title = SuccessTitle, body = ConfirmationMessage });
        }

        /// <summary>
        /// Helper function to process vacation or sick day requests made by an Admin user.
        /// </summary>
        /// <param name="incomingRequest">A SubmittedRequest mapped to the Form object.</param>
        /// <param name="request">Request object containing no data</param>
        /// <param name="requestsRepo">A specialised class that is responsible for CRUD operations on the Requests table.</param>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure</returns>
        private IActionResult ProcessVacationSickDayRequest(
            SubmittedRequest incomingRequest,
            ref Request request,
            IDBRequestsRepository requestsRepo
        )
        {
            var employeesRepo = _DbContext.EmployeesRepo;
            var entitlementsRepo = _DbContext.EntitlementsRepo;

            // Retrieve the employee from the form submission.
            var employeeName = incomingRequest.EmployeeName;

            // Retrieve the employee object.
            var employee = employeesRepo.GetEmployeeByName(employeeName);

            if (employee == null)
            {
                // Provide a request status message.
                ConfirmationMessage = $"Employee ({employeeName}) was not found";

                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }
            // Grab the employee entitilement details.
            var entitlement = entitlementsRepo.GetEntitlement(employee.Id);

            // Create the Request object.
            request = requestsRepo.CreateRequest(incomingRequest, employeeName, employee.Id);

            int availableDays = CalAvailableDays(request, entitlement);
            var totalDays = request.NumberOfDays;

            // Check if request can be approved.
            if ((availableDays - totalDays) < 0)
            {
                var dayOrDays = (availableDays == 1) ? "day" : "days";

                // Provide a request status message.
                ConfirmationMessage = $"{employee.FullName()} does not have enough {request.GetTypeString()} Days to cover this request.\n";
                ConfirmationMessage += $"The user has {availableDays} {dayOrDays} available.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            // Approve the request and update the employee's entitlements.
            request.Approve();
            requestsRepo.Add(request);

            entitlement.AdjustDaysTaken(incomingRequest.Type, totalDays);
            entitlementsRepo.Update(entitlement);

            // Provide a successful request status message.
            var dayswere = (totalDays == 1) ? "Day was" : "Days were";
            ConfirmationMessage = $"{request.GetTypeString()} {dayswere} added for {employee.FullName()}";
            return Ok(new { title = SuccessTitle, body = ConfirmationMessage });
        }

        /// <summary>
        ///  Helper function that returns availabel days based on the request type.
        /// </summary>
        /// <param name="request">The Request object</param>
        /// <param name="entitlement">The associated employee's entitlement</param>
        /// <returns></returns>
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
        /// Helper function to process stat or company day requests made by an Admin user.
        /// </summary>
        /// <param name="incomingRequest">A SubmittedRequest mapped to the Form object.</param>
        /// <param name="request">Request object containing no data</param>
        /// <param name="requestsRepo">A specialised class that is responsible for CRUD operations on the Requests table.</param>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure</returns>
        [Authorize(Policy = "AdminOnly")]
        private IActionResult ProcessStatCompanyDay(
            SubmittedRequest incomingRequest,
            out Request request,
            IDBRequestsRepository requestsRepo
        )
        {
            // Create the stat/company Request object.
            request = requestsRepo.CreateRequest(incomingRequest, incomingRequest.Title);

            // There can only be one stat/company day per day.
            var existingRequest = requestsRepo.GetRequestsInTimeframe(
                request.GetTypeString(), request.StartDate, request.EndDate
                );

            if (existingRequest.Count() != 0)
            {
                var type = request.GetTypeString();

                if (type == "Stat")
                {
                    type += " Holiday";
                }
                else
                {
                    type += " Day";
                }

                ConfirmationMessage = $"There already exists a {type} for this day.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            // Check if this stat holiday impacts approved/pending vacation requests.
            ApplyStatHolidays(request);

            request.Approve();
            requestsRepo.Add(request);

            ConfirmationMessage = $"{request.Title} was added";
            return Ok(new { title = SuccessTitle, body = ConfirmationMessage });
        }

        /// <summary>
        /// Helper function to check if the request being added/removed is a stat holiday and updates impacted vacation days.
        /// </summary>
        /// <param name="request">The current request object being processed</param>
        [Authorize(Policy = "AdminOnly")]
        private void ApplyStatHolidays(Request request, string action = "add")
        {
            // Only interested in stat holidays that are not on the weekend.
            // If removing, check if the request has pass.
            if (request.GetTypeString() != "Stat" ||
               (action == "remove" && request.StartDate < DateTime.Now) ||
               request.StartsOnWeekend()
            )
            {
                return;
            }

            // Get all vacation requests that are impacted (including pending ones).
            var allVacationRequests = _DbContext.RequestsRepo
                .GetRequestsInTimeframe(
                "Vacation", request.StartDate, request.EndDate
                );

            if (!allVacationRequests.Any())
            {
                return;
            }

            // Get all entitlements.
            var entitlements = _DbContext.EntitlementsRepo
                .GetAllEntitlements()
                .ToList();

            foreach (var vacation in allVacationRequests)
            {
                var entitlement = entitlements.Find(x => x.EmployeeId == vacation.EmployeeId);

                if (action == "remove")
                {
                    vacation.NumberOfDays += 1;
                    entitlement.VacationDaysAvailable -= vacation.IsApproved() ? 1 : 0;
                }
                else
                {
                    vacation.NumberOfDays -= 1;
                    entitlement.VacationDaysAvailable += vacation.IsApproved() ? 1 : 0;
                }

                _DbContext.RequestsRepo.Update(vacation);
                _DbContext.EntitlementsRepo.Update(entitlement);
            };
        }

        /// <summary>
        /// Process the removal of an event.
        /// </summary>
        /// <returns>A ActionResult contain a JSON object that indicates success or failure</returns>
        [HttpPost, Route("/remove-event")]
        [Authorize(Policy = "AdminOnly")]
        public IActionResult RemoveSubmission()
        {
            var id = Request.Form["id"];

            var request = _DbContext.RequestsRepo.GetRequest(id);

            if (request == null)
            {
                // Provide a request status message.
                ConfirmationMessage = $"The request was not found. Please contact your database admin.";
                return Ok(new { title = FailureTitle, body = ConfirmationMessage });
            }

            var isDeleted = _DbContext.RequestsRepo.Delete(request);
            var type = request.GetTypeString();

            var title = "";

            switch (type)
            {
                case "Sick":
                    title = $"{type}\t Day request";
                    break;
                case "Company":
                    title = $"{request.Title}\t(Company Day)";
                    break;
                case "Stat":
                    title += $"{request.Title}\t(Stat Holiday)";
                    break;
                default:
                    title = $"{type}\t request";
                    break;
            }

            // If the request is not a stat. holiday or company day.
            if (isDeleted && request.GetTypeString() != "Stat" && request.GetTypeString() != "Company")
            {
                var employee = _DbContext.EmployeesRepo.GetEmployeeById(request.EmployeeId);

                if (employee == null)
                {
                    ConfirmationMessage = $"{title} was deleted.";
                    return Ok(new { title = SuccessTitle, body = ConfirmationMessage });
                }

                // Grab employee name.
                string? name = employee.FullName();

                // In order for days to be returned to employee, the request being deleted must 
                // be >= today.
                if (request.StartDate >= DateTime.Now)
                {
                    // Add back the number of days for the respective employee.
                    var entitlements = _DbContext.EntitlementsRepo.GetEntitlement(employee.Id);
                    entitlements.ReturnDaysTaken(request.GetTypeString(), request.NumberOfDays);
                    _DbContext.EntitlementsRepo.Update(entitlements);
                }

                // Provide a status message.
                ConfirmationMessage = $"{title} was deleted for {name}";
                return Ok(new { title = SuccessTitle, body = ConfirmationMessage });
            }

            // For stat/company days.
            if (isDeleted)
            {
                ApplyStatHolidays(request, "remove");
                ConfirmationMessage = $"{title} was deleted.";
                return Ok(new { title = SuccessTitle, body = ConfirmationMessage });
            }

            return Ok(new { title = FailureTitle, body = "Failed to delete this request/event." });
        }
    }
}

