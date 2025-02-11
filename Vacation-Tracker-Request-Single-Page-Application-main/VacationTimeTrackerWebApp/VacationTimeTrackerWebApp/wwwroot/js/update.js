/**
 * Resets and update the app.
 */
function updateApp() {
    // Remove any blur.
    const userCanavas = $('#users-canvas');

    if (userCanavas.length) {
        userCanavas[0].style.filter = null;
    }

    // Reset global variables.
    userFormSubmitted = false;

    // Update the calendar.
    fetchCalendarEvents();
}

/**
 * Fetch and updates the employee select lists.
 */
function fetchEmployeeNames() {
    const selectEmployees = $('#select-employees');
    const updateEmployees = $('#update-user-employee-list');
    const removeEmployees = $('#remove-user-employee-list');

    if (!selectEmployees.length || !updateEmployees.length || !removeEmployees.length) {
        return;
    }

    $.ajax({
        url: "/get-employee-names",
        method: 'POST',
    })
        .then(res => {
            const html = res.html;
            selectEmployees.html(html);
            updateEmployees.html(html.replaceAll("-", ""));
            removeEmployees.html(html.replaceAll("-", ""));
        });
}

/**
 * Fetches events from the sever. 
 * NB. The calendar-config::events() function for the calendar is triggered once again:
 * this contains a call to fetchPendingRequests().
 */
function fetchCalendarEvents() {
    // Update calendar.
    calendar.refetchEvents();
}