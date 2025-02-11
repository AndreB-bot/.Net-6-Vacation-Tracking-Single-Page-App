using VacationTimeTrackerWebApp.Models;
using VacationTimeTrackerWebApp.Models.FormModels;

namespace VacationTimeTrackerWebApp.Data
{
    /// <summary>
    /// A specialised class that is responsible for CRUD operations on the Requests table.
    /// </summary>
    public interface IDBRequestsRepository : IDBRepository<Request>
    {
        /// <summary>
        /// The Id of the company user.
        /// </summary>
        private const int CompanyUserId = 1;

        /// <summary>
        /// Returns a Request object by id.
        /// </summary>
        /// <param name="id">Represents the int ID of the request being retireved</param>
        /// <returns>A Request object</returns>
        public Request GetRequest(int id);

        /// <summary>
        /// Overloaded Method: Returns a Request object by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A Request object</returns>
        public Request GetRequest(string id);

        /// <summary>
        /// Returns a List of Request entities belonging to active employees.
        /// </summary>
        /// <returns>A List of Request entities belonging to active employees.</returns>
        public List<Request> GetAllRequests();

        /// <summary>
        /// Helper function to create Request object from a SubmittedRequest and employee ID.
        /// </summary>
        /// <param name="request">The SubmittedRequest object from which key data will be taken</param>
        /// <param name="employeeId">The empoyee ID</param>
        /// <returns>A Request object</returns>
        public Request CreateRequest(SubmittedRequest request, string title, int employeeId = CompanyUserId);

        /// <summary>
        /// Returns a List of Request entities marked as approved.
        /// </summary>
        /// <returns>A List containg Request objects</returns>
        public List<Request> GetAllApprovedRequests();

        /// <summary>
        /// Returns a List of Request entities marked as pending.
        /// </summary>
        /// <returns>A List containg Request objects</returns>
        public List<Request> GetAllPendingRequests();

        /// <summary>
        /// Returns a List of "pending" Request entities filtered by employee id.
        /// </summary>
        /// <returns>A List containg Request objects</returns>
        public List<Request> GetUserPendingRequests(int employeeId);

        /// <summary>
        /// Returns a List of "pending" Request entities filtered by employee id and type.
        /// </summary>
        /// <returns>A List containg Request objects</returns>
        public List<Request> GetUserPendingRequests(int employeeId, string type);

        /// <summary>
        /// Fecthes all requests within a given timeframe.
        /// </summary>
        /// <param name="type">The request type</param>
        /// <param name="startDate">The beginging date for the query</param>
        /// <param name="endDate">The end date for the query</param>
        /// <returns>A List of Requests within a given timeframe.</returns>
        public List<Request> GetRequestsInTimeframe(string type, DateTime startDate, DateTime endDate);

        /// <summary>
        /// Get all reviewed Request objects that have "NotifyUser" set to true.
        /// </summary>
        /// <param name="id">The current employee's/user's ID</param>
        /// <returns>A List of reviewed employee's Requests for notifying the current user</returns>
        public List<Request> GetAllReviewedRequests(int id);

        /// <summary>
        /// Returns a list of Requests with their end dates altered to reflect what's in the calendar.
        /// </summary>
        /// <returns></returns>
        public List<Request> GetAllPendingRequestsForDisplay();
    }
}
