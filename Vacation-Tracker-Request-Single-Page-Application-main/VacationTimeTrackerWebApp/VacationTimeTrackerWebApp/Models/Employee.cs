using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacationTimeTrackerWebApp.Models
{
    /// <summary>
    /// Models/Represents an Employee entry in the Employees table.
    /// </summary>
    public class Employee
    {
        private string _FirstName;
        private string _LastName;

        [Key]
        [Column("EmployeeID")]
        public int Id { get; set; }

        [Column("EmployeeEmail")]
        public string Email { get; set; }

        [Column("SystemRemovalDate")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? RemovalDate { get; set; }
        public DateTime StartDate { get; set; }
        public Entitlement Entitlement { private set; get; }

        public string FirstName
        {
            get => _FirstName;
            set
            {
                // Remove any extra spaces.
                value = value.Trim();

                if (!char.IsUpper(value[0]))
                {
                    _FirstName = char.ToUpper(value[0]) + value.Substring(1).ToLower();
                }
                else
                {
                    _FirstName = value;
                }
            }
        }

        public string LastName
        {
            get => _LastName;
            set
            {
                // Remove any extra spaces.
                value = value.Trim();

                if (!char.IsUpper(value[0]))
                {
                    _LastName = char.ToUpper(value[0]) + value.Substring(1).ToLower();
                }
                else
                {
                    _LastName = value;
                }
            }
        }

        /// <summary>
        /// Indicates whether the Employee has Admin access.
        /// </summary>
        /// <returns>True if object has Admin access, False otherwise.</returns>
        public bool IsAdmin()
        {
            if (Entitlement != null)
            {
                return Entitlement.AccessType == "Admin";
            }

            return false;
        }

        /// <summary>
        /// Returns the full name of the employee.
        /// </summary>
        /// <returns>Employee's full name</returns>
        public string FullName()
        {
            return $"{FirstName} {LastName}";
        }

        /// <summary>
        /// Updates the current employee information with newly supplied data.
        /// </summary>
        /// <param name="updatedEmployee">The Employee object containing </param>
        public void UpdateDetails(Employee updatedEmployee)
        {
            Email = updatedEmployee.Email;
            FirstName = updatedEmployee.FirstName;
            LastName = updatedEmployee.LastName;
            StartDate = updatedEmployee.StartDate;
        }

        /// <summary>
        /// Sets the Entitlement property.
        /// </summary>
        /// <param name="entitlement"></param>
        public void SetEntitlement(Entitlement entitlement)
        {
            Entitlement = entitlement;
        }

        /// <summary>
        /// Ensures the correct DateTime is always parsed.
        /// </summary>
        /// <param name="expectedDate">The incoming date string value</param>
        public void SetStartDateTime(string dateString)
        {
            var dateSplit = dateString.Split('-');

            // Expected format "dd-MM-yyyy"
            var year = Int32.Parse(dateSplit[2]);
            var month = Int32.Parse(dateSplit[1]);
            var day = Int32.Parse(dateSplit[0]);

            StartDate = new DateTime(year, month, day);
        }

        /// <summary>
        /// Indicates if the user has a removal date, therefore marked as "removed".
        /// </summary>
        /// <returns>A bool Indicating if the user has a removal date, therefore marked as "removed".</returns>
        public bool IsRemoved()
        {
            return RemovalDate != null;
        }
    }
}
