namespace VacationTimeTrackerWebApp.Models
{
    /// <summary>
    /// Represents an entry into the reports table;
    /// </summary>
    public class ReportEntry
    {
        public string EmployeeName { get; set; }
        public int VacationRollover { get; set; }
        public int VacationDaysAvailable { get; set; }
        public int VacationDaysTaken { get; set; }
        public int UpcomingVacationDays { get; set; }
        public int SickDaysRemaining { get; set; }
        public int SickDaysTaken { get; set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="name">The name of the employee</param>
        /// <param name="entitlement">The associated entitlements of the employee</param>
        public ReportEntry(Employee employee, Entitlement entitlement, List<Request> request)
        {
            EmployeeName = employee.FullName();
            VacationDaysAvailable = entitlement.VacationDaysAvailable;
            SickDaysRemaining = entitlement.SickDays - entitlement.SickDaysTaken;
            SickDaysTaken = entitlement.SickDaysTaken;
            VacationDaysTaken = GetVacationDaysTaken(employee, request);
            UpcomingVacationDays = TotalUpcomingVacationDays(employee, request);
            VacationRollover = CalculateRemainingRollover(entitlement.EarnedVacationDays);
        }

        /// <summary>
        /// Helper function that calculates the correct Rollover for the the current record.
        /// </summary>
        /// <returns>The calculation ot the Rollover amount</returns>
        private int CalculateRemainingRollover(int earnedDays)
        {
            var total = VacationDaysAvailable - earnedDays;

            if (total < 0)
            {
                return 0;
            }

            return total;
        }

        /// <summary>
        /// Gets the number for upcoming vacation requests.
        /// </summary>
        /// <param name="employee">The associated employee</param>
        /// <param name="request">A List of Request objects</param>
        /// <returns>A count of all upcoming vacation request for the user</returns>
        private int TotalUpcomingVacationDays(Employee employee, List<Request> requests)
        {
            var currentDate = DateTime.Now;
            var upcomingVacation = requests.Where(x =>
           {
               return x.StartDate > currentDate &&
               x.EmployeeId == employee.Id &&
               x.GetTypeString() == "Vacation" &&
               x.StartDate.Year >= currentDate.Year;
           });

            var count = 0;

            foreach (var vacation in upcomingVacation)
            {
                count += vacation.CalTotalNumberOfDays(requests);
            }

            return count;
        }

        /// <summary>
        /// Returns the total number of past and current vacation requests for the year.
        /// </summary>
        /// <param name="employee">The associated employee</param>
        /// <param name="request">A List of Request objects</param>
        /// <returns>A count of all past and current vacation requests for the year.</returns>
        private int GetVacationDaysTaken(Employee employee, List<Request> requests)
        {
            var currentDate = DateTime.Now;
            var takenVacation = requests.Where(x =>
            {
                return x.StartDate <= currentDate &&
                x.EmployeeId == employee.Id &&
                x.GetTypeString() == "Vacation" &&
                x.StartDate.Year == currentDate.Year;
            });

            var count = 0;

            foreach (var vacation in takenVacation)
            {
                count += vacation.CalTotalNumberOfDays(requests);
            }

            return count;
        }
    }
}
