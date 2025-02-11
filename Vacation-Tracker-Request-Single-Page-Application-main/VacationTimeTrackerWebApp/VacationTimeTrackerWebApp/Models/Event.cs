namespace VacationTimeTrackerWebApp.Models
{
    /// <summary>
    /// Represents an Event JSON object for FullCalendar JS.
    /// </summary>
    [Serializable()]
    public class Event
    {
        // Events color.
        private const string VacationDaysColor = "green";
        private const string VacationPendingColor = "rgb(118 155 118)";
        private const string SickDaysColor = "#ff7518";
        private const string SickDaysPendingColor = "#ff8c69";
        private const string StatHolidayColor = "#96212d";
        private const string CompanyDayColor = "cornflowerblue";

        // Events names.
        private const string Vacation = "vacation";
        private const string Sick = "sick";
        private const string Stat = "stat";
        private const string Company = "company";

        /// <summary>
        /// The value of the event-modal header title.
        /// </summary>
        public string HeaderTitle { get; private set; }

        /// <summary>
        /// The title of the event; this is usually the employee 
        /// name or the name of a stat holiday, for example.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The start date of the event.
        /// </summary>
        public string Start { get; private set; }

        /// <summary>
        /// The end date of the event.
        /// </summary>
        public string End { get; private set; }

        /// <summary>
        /// The color associated with the event.
        /// </summary>
        public string Color { get; private set; }

        /// <summary>
        /// This alludes to the the type of requests/event, i.e. Vacation, Sick Day, Stat, etc.
        /// </summary>
        public string Type { get; private set; }

        /// <summary>
        /// The unique ID associated with the associated request.
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// The total stat holidays that the event overlaps with. This is only applicable to vacation requests.
        /// </summary>
        public int NumStatHolidays { get; private set; }

        /// <summary>
        /// The duration of the event.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Indicates whether the event/request is Approved, Pending or Rejected.
        /// </summary>
        public string Status { get; private set; }

        /// <summary>
        /// Any CSS class(es) that is associated with the event.
        /// </summary>
        public string ClassName { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="request">The Request entity from which information such as start and end date will be taken from.</param>
        /// <param name="title">The Event's title. This could be employee name or company holiday.</param>
        /// <param name="numStatHolidays">The number of stat holidays the Request overlaps with.</param
        public Event(Request request, string title, int numStatHolidays = 0)
        {
            Start = request.StartDate.ToString("yyyy-MM-dd");
            End = request.EndDate.ToString("yyyy-MM-dd");
            Length = request.NumberOfDays;
            Id = request.RequestId;
            ClassName = Id.ToString();
            NumStatHolidays = numStatHolidays;

            Status = request.GetStatus();
            SetColorAndType(request);
            SetHeaderTitle(request);

            Title = (Status == "Pending") ? title + "\t(Pending)" : title;
        }

        /// <summary>
        /// Healper function that sets the event header title.
        /// </summary>
        /// <param name="request">The Request entity from which information such as start and end date is derived from.</param>
        private void SetHeaderTitle(Request request)
        {
            HeaderTitle = request.GetTypeString();

            switch (Type)
            {
                case "stat":
                    HeaderTitle += "\tHoliday";
                    break;
                case "company":
                    HeaderTitle += "\tDay";
                    break;
                case "sick":
                    HeaderTitle += "\tDay";
                    break;
            }
            HeaderTitle += (request.GetStatus() == "Pending") ? "\t(Pending Approval)" : "";
        }

        /// <summary>
        /// Helper function that sets the Color and Type props.
        /// </summary>
        /// <param name="request">The Request entity from which information such as start and end date is derived from.</param>
        private void SetColorAndType(Request request)
        {
            switch (request.GetTypeString())
            {
                case "Sick":
                    Color = (Status == "Pending") ? SickDaysPendingColor : SickDaysColor;
                    Type = Sick;
                    break;
                case "Stat":
                    Color = StatHolidayColor;
                    Type = Stat;
                    break;
                case "Company":
                    Color = CompanyDayColor;
                    Type = Company;
                    break;
                default:
                    Type = Vacation;
                    Color = (Status == "Pending") ? VacationPendingColor : VacationDaysColor;
                    break;
            }
        }
    }
}
