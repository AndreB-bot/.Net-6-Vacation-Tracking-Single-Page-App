using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacationTimeTrackerWebApp.Models
{
    /// <summary>
    /// Models/Represents a Request entry in the Requests table.
    /// Requests are eventually transformed into Events for the Calendar UI to render.
    /// </summary>
    public class Request
    {
        protected const string Approved = "A";
        protected const string Rejected = "R";
        protected const string Pending = "P";

        [Key]
        [Column("RequestID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RequestId { get; set; }
        public int EmployeeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Status { get; set; }
        public string? Type { get; set; }
        public string? Title { get; set; }

        [Column("Days")]
        public int NumberOfDays { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public bool NotifyUser { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string? Comments { get; set; }

        /// <summary>
        /// Sets the status of the request to "Approved";
        /// </summary>
        public void Approve()
        {
            Status = Approved;
        }

        /// <summary>
        /// Sets the status of the request to "Rejected";
        /// </summary>
        public void Reject()
        {
            Status = Rejected;
        }

        /// <summary>
        /// Sets the status of the request to "Pending";
        /// </summary>
        public void SetPending()
        {
            Status = Pending;
        }

        /// <summary>
        /// Updates the "NotifyUser" value of the Request.
        /// </summary>
        internal void Notify()
        {
            NotifyUser = true;
        }

        /// <summary>
        /// Returns a boolean indicating that the request was already reviewed.
        /// </summary>
        /// <returns>A boolean indicating that the request was already reviewed.</returns>
        public bool IsReviewed()
        {
            return Status != Pending;
        }

        /// <summary>
        /// Calculates the number of workdays the request covers.
        /// </summary>
        /// <returns>The duration/length of the request</returns>
        public int CalTotalNumberOfDays(IEnumerable<Request> requests)
        {
            int duration = (EndDate - StartDate).Days;

            if (StartDate == EndDate)
            {
                duration = 1;
            }

            // Calculate business days.
            var start = StartDate;

            while (start != EndDate)
            {
                if (start.DayOfWeek == DayOfWeek.Saturday || start.DayOfWeek == DayOfWeek.Sunday)
                {
                    duration--;
                }

                start = start.AddDays(1);
            }

            var numStatHolidays = 0;

            // Check for Stat holidays and add back those days.
            if (GetTypeString() == "Vacation")
            {
                numStatHolidays = requests.Where(x =>
                {
                    return x.GetTypeString() == "Stat" &&
                    (x.StartDate >= this.StartDate && x.EndDate <= this.EndDate) &&
                    !x.StartsOnWeekend();
                })
                .Count();
            }

            return duration -= numStatHolidays;
        }

        /// <summary>
        /// Sets the total number of days.
        /// </summary>
        public void SetNumberOfDays(IEnumerable<Request> requests)
        {
            NumberOfDays = CalTotalNumberOfDays(requests);
        }

        /// <summary>
        /// Returns the full string of the Request's status.
        /// </summary>
        /// <returns>String indicating the Request's status</returns>
        public string GetStatus()
        {
            switch (Status)
            {
                case "P":
                    return "Pending";
                case "R":
                    return "Rejected";
                default:
                    return "Approved";
            }
        }

        /// <summary>
        /// Returns the full string of the Request's type.
        /// </summary>
        /// <returns>String indicating the Request's type</returns>
        public string GetTypeString()
        {
            switch (Type)
            {
                case "S":
                    return "Sick";
                case "C":
                    return "Company";
                case "P":
                    return "Stat";
                default:
                    return "Vacation";
            }
        }

        /// <summary>
        /// Indicates whether this Request is approved.
        /// </summary>
        /// <returns>A boolean indicating whether this Request is approved</returns>
        internal bool IsApproved()
        {
            return GetStatus() == "Approved";
        }

        /// <summary>
        /// Returns true or false if the request starts on a weekend.
        /// </summary>
        /// <returns>True or false this request starts on a weekend</returns>
        public bool StartsOnWeekend()
        {
            var startDate = StartDate.DayOfWeek;
            return startDate == DayOfWeek.Sunday || startDate == DayOfWeek.Saturday;
        }
    }

}
