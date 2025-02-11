using Microsoft.AspNetCore.Mvc;
using System.Globalization;

namespace VacationTimeTrackerWebApp.Models.FormModels
{
    /// <summary>
    /// Responsible for temporarily holding a timeoff submission data which will ultimately be passed on to an Request object.
    /// </summary>
    public class SubmittedRequest
    {
        /// <summary>
        /// The title of the request being submitted. 
        /// If there isn't a title, the employee's name is used for same.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The employee name usually derived from the employee select list or 
        /// the name of the employee submitting the current request.
        /// </summary>
        [FromForm(Name = "Employee")]
        public string EmployeeName { get; set; }

        /// <summary>
        /// This alludes to the the type of request, i.e. Vacation, Sick Day, Stat, etc.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The submitted request's start date.
        /// </summary>
        public string? StartDate { get; set; }

        /// <summary>
        /// The submitted request's end date.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Returns start date as a DateTime object.
        /// </summary>
        /// <returns>A DateTime object with parsed from the StartDate string.</returns>
        public DateTime StartDateTime()
        {
            var dateInfo = StartDate.Split('-');
            int year = Int32.Parse(dateInfo[2]);
            int month = Int32.Parse(dateInfo[1]);
            int day = Int32.Parse(dateInfo[0]);

            return new DateTime(year, month, day);
        }

        /// <summary>
        /// Returns end date as a DateTime object.
        /// </summary>
        /// <returns>A DateTime object with parsed from the StartDate string.</returns>
        public DateTime EndDateTime()
        {
            var dateInfo = EndDate.Split('-');
            int year = Int32.Parse(dateInfo[2]);
            int month = Int32.Parse(dateInfo[1]);
            int day = Int32.Parse(dateInfo[0]);

            var endDate = new DateTime(year, month, day);

            if (StartDate != EndDate)
            {
                // Adding a day to end date, allows same to be inclusive.
                return endDate.AddDays(1);
            }

            return endDate;
        }

        /// <summary>
        /// Returns true or false if the submitted request starts on a weekend.
        /// </summary>
        /// <returns>True or false this submitted request starts on a weekend</returns>
        public bool StartsOnWeekend()
        {
            var startDate = StartDateTime().DayOfWeek;
            return startDate == DayOfWeek.Sunday || startDate == DayOfWeek.Saturday;
        }

        /// <summary>
        /// Returns the full string of the IncommingRequest's type.
        /// </summary>
        /// <returns>String indicating the IncommingRequest's type</returns>
        public string GetTypeString()
        {
            switch (Type)
            {
                case "sick":
                    return "Sick";
                case "company":
                    return "Company";
                case "stat":
                    return "Stat";
                default:
                    return "Vacation";
            }
        }
    }
}
