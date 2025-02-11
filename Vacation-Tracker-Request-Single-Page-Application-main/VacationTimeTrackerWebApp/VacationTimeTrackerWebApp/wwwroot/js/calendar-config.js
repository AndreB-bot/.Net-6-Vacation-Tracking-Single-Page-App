/**
 * This file houses the main JS config component for the callendar app.
 * Also supplying helper functions for Bootstrap datetimepicker.
 */

const NUM_DAYS_WEEK = 7;
const FOURTH_INDEX = 4;

let calendar;

// Config options for Bootstrap DatePicker.
const datePickerOptions = {
    format: 'DD-MM-YYYY',
    icons: {
        time: "fa fa-clock-o",
        date: "fa fa-calendar",
        next: "fa fa-arrow-circle-right",
        previous: "fa fa-arrow-circle-left"
    }
}

/**
 * Calendar config and render.
 */
document.addEventListener('DOMContentLoaded', function () {
    const calendarEl = document.getElementById('calendar');

    calendar = new FullCalendar.Calendar(calendarEl, {
        timeZone: 'local',
        customButtons: {
            monthPicker: {
                text: 'Select A Month',
                click: showMonthPickerModal
            },
            year: {
                text: 'Year'
            },
            refresh: {
                text: 'Refresh',
                click: fetchCalendarEvents
            }
        },
        headerToolbar: {
            left: 'prevYear,year,nextYear refresh,today',
            center: 'title',
            right: 'prev,monthPicker,next'
        },
        buttonText: {
            today: 'Today'
        },
        // Can click day/week names to navigate views.
        navLinks: true,
        editable: true,
        height: '90vh',
        // Allow "more" link when there are too many events.
        dayMaxEvents: 6,
        // Keys such as "dateClick" can be tied to a function. e.x. dateClicked is a function.
        dateClick: dateClicked,
        eventClick: eventClicked,
        // Disable dragging.
        eventStartEditable: false,
        // Disable resizing.
        eventDurationEditable: false,
        eventSources: events,
        eventOrder: eventOrder
    });

    calendar.render();
    handleMonthPicker(calendar)
});

/**
 * Event handler for when user clicks a date.
 * 
 * @param {any} info
 *  Details of the date.
 */
const dateClicked = function (info) {
    handleDatePickerValue(info);
    $('#add-timeslot-modal').modal("show");
}

/*
 * Helper function to address some of the datetimepicker flow.
 */
function handleDatePickerValue(info) {
    let clickedDate = moment(info.dateStr, 'YYYY-MM-DD');

    const startDate = $('#start-datetime-picker');
    const endDate = $('#end-datetime-picker');

    startDate.datetimepicker(datePickerOptions);
    startDate.data("DateTimePicker").date(clickedDate);

    endDate.datetimepicker(datePickerOptions);
    const date = startDate.data("DateTimePicker").date();
    endDate.data("DateTimePicker").date(date);
    endDate.data("DateTimePicker").minDate(date);

    // On startDate change, change the min date for endDate.
    startDate.on('dp.change', () => {
        const newStartDate = startDate.data("DateTimePicker").date();
        endDate.data("DateTimePicker").minDate(newStartDate);
        endDate.val(newStartDate)
    })
}

/**
 * Event handler for when user clicks an event.
 * 
 * @param {any} info
 *  Details of the event.
 */
const eventClicked = function (info) {
    const event = info.event;
    const type = event.extendedProps.type;

    switch (type) {
        case "sick":
            popluateAndShowEventModal($('#sick-event-details-modal'), event, type);
            break;

        case "stat":
            popluateAndShowEventModal($('#stat-event-details-modal'), event, type);
            break;

        case "company":
            popluateAndShowEventModal($('#company-event-details-modal'), event, type);
            break;

        default:
            popluateAndShowEventModal($('#vacation-event-details-modal'), event);
            break;
    }

}

/**
 * Populates the respective modal with the selected event details, then displays same.
 * 
 * @param {any} eventModal
 *   JQuery instance of the given modal.
 * @param {any} event
 *   The event object.
 * @param {any} eventName
 *   The event name (vacation, sick, etc)
 */
function popluateAndShowEventModal(eventModal, event, eventName = "vacation",) {
    const endStr = event.endStr;
    const startStr = event.startStr;

    // Close the current popover.
    $('.fc-popover-close').click();

    // Event helper variables.
    let duration = event.extendedProps.length;
    let details;
    let endDateString;
    let startDateString;

    let startDate = moment(startStr, "YYYY/MM/DD");
    let endDate = moment(endStr, "YYYY/MM/DD")

    // Offsets the additional day when the Request entity was made.
    endDate.subtract(1, 'days');

    // Build out the details string.
    startDateString = startDate.format("DD/MMM/YYYY");

    details = `Starts: ${startDateString} - Ends: ${startDateString}`;

    if (endStr) {
        endDateString = endDate.format("DD/MMM/YYYY");
        details = `Starts: ${startDateString} - Ends: ${endDateString}`;
    }

    eventIdPrefix = "#" + eventName + "-event";

    // Populate confirmation prompt modal.
    let confirmationPromptBody = $(eventIdPrefix + "-confirmation-propmt-body");
    let textValue = "Are you sure you want to remove %title%?"
        .replaceAll("%title%", event.title);

    if (eventName != "stat" && eventName != "company") {
        textValue = "Are you sure you want to remove these %type% days for %user%?"
            .replaceAll("%type%", eventName);

        textValue = textValue.replaceAll("%user%", event.title);
    }

    confirmationPromptBody.text(textValue);

    // Check for num. of stat days.
    const numStatDays = event.extendedProps.numStatHolidays;

    // Footer text details.
    let dayOrDays = (duration == 1) ? "Day" : "Days";
    let footerText = `${duration} ${dayOrDays}`;

    if (numStatDays) {
        dayOrDays = (numStatDays == 1) ? "Holiday" : "Holidays";
        footerText += ` + ${numStatDays} Stat ${dayOrDays}`;
    }

    let title = event.title;
    const approveBtn = $('#' + eventName + '-event-approve-btn');
    const rejectBtn = $('#' + eventName + '-event-reject-btn');
    const removeBtn = $('.show-confirmation-prompt');

    // Changes for pending requests.
    if (event.extendedProps.status === "Pending") {
        title = title.replace("(Pending)", "");
        approveBtn.each(function () { $(this).removeClass('hidden'); });
        rejectBtn.each(function () { $(this).removeClass('hidden'); });
        removeBtn.each(function () { $(this).addClass('hidden'); });
    }
    else {
        approveBtn ? approveBtn.each(function () { $(this).addClass('hidden'); }) : null;
        rejectBtn ? rejectBtn.each(function () { $(this).addClass('hidden'); }) : null;
        removeBtn.each(function () { $(this).removeClass('hidden'); });
    }

    // Targeted elements.
    const detailsHeaderTitle = eventIdPrefix + "-details-header";
    const detailsSection = eventIdPrefix + "-details";
    const modal = detailsSection + "-modal";
    const titleSection = eventIdPrefix + "-title";
    const footerDetails = eventIdPrefix + "-duration";
    const eventSubmitId = eventIdPrefix + "-submit-id";

    eventModal.on('show.bs.modal', () => {
        //Populate the modal.
        $(detailsHeaderTitle).text(event.extendedProps.headerTitle);
        $(titleSection).text(title);
        $(detailsSection).text(details)
        $(footerDetails).text(footerText);

        // Set data.
        $(eventSubmitId).data('id', event.id);
        approveBtn ? approveBtn.data('id', event.id) : null;
        rejectBtn ? rejectBtn.data('id', event.id) : null;

        // Add colors.
        $(modal).find('.modal-header').css("background-color", event.backgroundColor);
        $(modal).find('.modal-content ').css("background-color", event.backgroundColor);
        $(modal).find('.card-footer').css("background-color", event.backgroundColor);
    }
    ).modal("show")
}

/**
 * Helper function to trigger the month picker modal.
 */
const showMonthPickerModal = function () {
    $('#month-picker-modal').modal("show");
}

/**
 * Handles the functionality of selecting a month.
 * 
 * @param {any} calendar
 *   The calendar object.
 */
function handleMonthPicker(calendar) {
    const modal = $('#month-picker-modal');

    $('#month-picker-body > li').each(function () {
        $(this).click(function () {
            const dateStr = calendar.getDate();
            const currentDate = moment(dateStr);
            const month = $(this).data('month');
            const year = currentDate.year();
            calendar.gotoDate(new Date(year, month, 1));
            modal.modal('hide');
        })
    })

    modal.on('show.bs.modal', () => {
        const dateStr = calendar.getDate();
        const currentMonth = moment(dateStr).month();

        $('#month-picker-body > li').each(function () {
            const month = $(this).data('month');

            if (month === currentMonth) {
                $(this).addClass('month-picker-active-month');
            }
            else {
                $(this).removeClass('month-picker-active-month');
            }
        });
    })
}

/**
 * Returns all approved events.
 *
 * @param {any} fetchInfo
 * @param {any} successCallback
 * @param {any} failureCallback
 */
function events(fetchInfo, successCallback, failureCallback) {
    // Check for pending requests.
    fetchPendingRequests();

    // Show notifications is any.
    getNewNotifications();

    $.ajax({
        method: "POST",
        url: "api/getEvents",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        data: {}
    })
        .then(res => {
            successCallback(res);
        });
}

/**
 * Orders how events are displayed in the Calendar UI.
 * 
 * @param {any} event1
 * @param {any} event2
 */
function eventOrder(event1, event2) {
    if (event1.type === "stat" || event1.type === "company") {
        return -1;
    }

    return 0;
}