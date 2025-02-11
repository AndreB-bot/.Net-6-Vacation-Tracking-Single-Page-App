using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacationTimeTrackerWebApp.Models
{
    /// <summary>
    /// Models/Represents a Entitlement entry in the Entitlements table.
    /// </summary>
    public class Entitlement
    {
        [Key]
        [Column("EmpID")]
        public int EmployeeId { get; set; }

        public string? AccessType { get; set; }

        [Column("VacationEarned")]
        public int EarnedVacationDays { get; set; }

        public int VacationDaysAvailable { get; set; }

        public int SickDays { get; set; }

        [Column("SickTaken")]
        public int SickDaysTaken { get; set; }

        public int VacationRollover { get; set; }

        /// <summary>
        /// Adjust the amount of days taken for a request.
        /// </summary>
        /// <param name="type">The typre of request (vacation, sick days)</param>
        /// <param name="amount">The int amount of days to be deducted</param>
        public void AdjustDaysTaken(string type, int amount)
        {
            if (type == null || amount == 0)
            {
                return;
            }

            if (type == "vacation" || type == "Vacation")
            {
                VacationDaysAvailable -= amount;
            }
            else
            {
                SickDaysTaken += amount;
            }
        }

        /// <summary>
        /// Re-adds the amount of days for an emolyee from the associated deleted request
        /// by adjusting their number of taken days.
        /// </summary>
        /// <param name="amount">The int amount of days to be re-added</param>
        public void ReturnDaysTaken(string type, int amount)
        {
            if (type == null || amount == 0)
            {
                return;
            }

            if (type == "Vacation")
            {
                VacationDaysAvailable += amount;
            }
            else
            {
                SickDaysTaken -= amount;
            }
        }

        /// <summary>
        /// Populates a new entitlement record.
        /// </summary>
        /// <param name="employee">The associated Employee object</param>
        /// <param name="details">The form details for supplying additional details.</param>
        public void AddData(Employee employee, IFormCollection details)
        {
            EmployeeId = employee.Id;
            AccessType = details["AccessType"];
            VacationRollover = Int32.Parse(details["VacationRollover"]);
            EarnedVacationDays = Int32.Parse(details["VacationDays"]);
            SickDays = Int32.Parse(details["SickDays"]);
            VacationDaysAvailable = EarnedVacationDays + VacationRollover;
        }

        /// <summary>
        /// Updates the current entitlement with newly supplied data.
        /// </summary>
        /// <param name="details">Form submission data</param>
        public void UpdateData(IFormCollection details)
        {
            var oldEarnedVacationDays = EarnedVacationDays;

            AccessType = details["AccessType"];
            EarnedVacationDays = Int32.Parse(details["VacationDays"]);
            SickDays = Int32.Parse(details["SickDays"]);

            if (EarnedVacationDays != oldEarnedVacationDays)
            {
                VacationDaysAvailable += (EarnedVacationDays - oldEarnedVacationDays);
            }
        }
    }
}