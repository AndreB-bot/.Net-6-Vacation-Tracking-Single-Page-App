using Microsoft.EntityFrameworkCore;
using VacationTimeTrackerWebApp.Models;
using VacationTimeTrackerWebApp.Models.FormModels;

namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// This class is solely responsible for CRUD operations on the Requests table 
    /// as well as creating Request entities where required.
    /// </summary>
    public class RequestsRepository : IDBRequestsRepository
    {
        protected const string Vacation = "V";
        protected const string SickDay = "S";
        protected const string Stat = "P";
        protected const string Company = "C";

        /// <summary>
        /// The Id of the company user.
        /// </summary>
        private const int CompanyUserId = 1;

        /// <summary>
        /// A instance of DBContext.
        /// </summary>
        private DBContext DbContext;

        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="context">An insatance of the DBContext class.</param>
        public RequestsRepository(DBContext context)
        {
            DbContext = context;
        }

        /// <inheritdoc/>
        public Request Add(Request request)
        {
            DbContext.Requests.Add(request);
            DbContext.SaveChanges();

            return request;
        }

        /// <inheritdoc/>
        public List<Request> GetAllRequests()
        {
            var removedIds = DbContext.RemovedEmployeesIds;

            return DbContext.Requests
                .ToList()
                .FindAll(x => !Array.Exists(removedIds, e => e == x.EmployeeId));
        }

        /// <inheritdoc/>
        public Request GetRequest(int id)
        {
            var request = GetAllRequests()
                .Find(x => x.RequestId == id);

            if (request != null)
            {
                return request;
            }

            return null;
        }

        /// <inheritdoc/>
        public Request GetRequest(string id)
        {
            return GetRequest(Int32.Parse(id));
        }

        /// <inheritdoc/>
        public Request Update(Request requestChanges)
        {
            DbContext.Attach(requestChanges);
            _ = EntityState.Modified;
            DbContext.SaveChanges();
            return requestChanges;
        }

        /// <inheritdoc/>
        public Request CreateRequest(
            SubmittedRequest incomingRequest,
            string title,
            int employeeId = CompanyUserId
        )
        {
            var request = new Request();
            request.Title = title;
            request.EmployeeId = employeeId;
            request.StartDate = incomingRequest.StartDateTime();
            request.EndDate = incomingRequest.EndDateTime();
            request.SetNumberOfDays(GetAllRequests());
            request.SetPending();

            SetType(incomingRequest, request);

            // Ensure Stat & Company requests are one day events.
            if (request.GetTypeString() == "Stat" || request.GetTypeString() == "Company")
            {
                request.EndDate = request.StartDate;
            }

            return request;
        }

        /// <inheritdoc/>
        public bool Delete(Request request)
        {
            if (request == null)
            {
                return false;
            }

            DbContext.Requests.Remove(request);
            DbContext.SaveChanges();

            if (GetRequest(request.RequestId) != null)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public List<Request> GetAllApprovedRequests()
        {
            return GetAllRequests().FindAll(x => x.GetStatus() == "Approved");

        }

        /// <inheritdoc/>
        public List<Request> GetAllPendingRequests()
        {
            return GetAllRequests().FindAll(x => x.GetStatus() == "Pending");
        }

        /// <inheritdoc/>
        public List<Request> GetUserPendingRequests(int employeeId)
        {
            return GetAllPendingRequests().FindAll(x => x.EmployeeId == employeeId);
        }

        /// <inheritdoc/>
        public List<Request> GetUserPendingRequests(int employeeId, string type)
        {
            return GetUserPendingRequests(employeeId).FindAll(x => x.GetTypeString() == type);
        }

        /// <inheritdoc/>
        public List<Request> GetRequestsInTimeframe(string type, DateTime startDate, DateTime endDate)
        {
            return GetAllApprovedRequests().FindAll(x =>
            {
                return x.GetTypeString() == type &&
                    (startDate >= x.StartDate && endDate <= x.EndDate);
            });
        }

        /// <inheritdoc/>
        public List<Request> GetAllReviewedRequests(int id)
        {
            var reviewedRequests = GetAllRequests().FindAll(x =>
            {
                return (x.GetTypeString() == "Vacation" || x.GetTypeString() == "Sick")
                        && x.NotifyUser && x.EmployeeId == id;
            });

            // Update these to say user has been notified.
            foreach (var request in reviewedRequests)
            {
                request.NotifyUser = false;
                // Update the request.
                DbContext.RequestsRepo.Update(request);
            }

            // For displaying purposes, we need to offset the end-dates by -1.
            // N.B: Doing this in a separate loop ensures there are no mistake of updating these, 
            // with a new end-date, in the db.
            foreach (var request in reviewedRequests)
            {
                if (request.NumberOfDays == 1)
                {
                    request.EndDate = request.StartDate;
                    continue;
                }

                request.EndDate = request.EndDate.AddDays(-1);
            }

            return reviewedRequests;
        }

        /// <summary>
        /// Helper function to set the Type property.
        /// </summary>
        /// <param name="incomingRequest">The SubmittedRequest object from which key data will be taken</param>
        /// <param name="request">A Request instance.</param>
        private static void SetType(SubmittedRequest incomingRequest, Request request)
        {
            switch (incomingRequest.Type)
            {
                case "stat":
                    request.Type = Stat;
                    break;

                case "company":
                    request.Type = Company;
                    break;

                case "sick":
                    request.Type = SickDay;
                    break;

                default:
                    request.Type = Vacation;
                    break;
            }
        }

        /// <inheritdoc/>
        public List<Request> GetAllPendingRequestsForDisplay()
        {
            var pendingRequests = GetAllPendingRequests();

            // FullCalendar counts the day prior to the final end-date as the event's final day.
            // So originally, each request greater than a day had it's end date offset by +1.
            // For displaying purposes, we need to offset those end-dates by -1.
            foreach (var request in pendingRequests)
            {
                if (request.NumberOfDays == 1)
                {
                    request.EndDate = request.StartDate;
                    continue;
                }

                request.EndDate = request.EndDate.AddDays(-1);
            }

            return pendingRequests;
        }
    }
}
