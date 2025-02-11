$(document).ready(() => {
    processApproveRequests();
    processRejectRequests();
    displayRequestCount();
    setUpRejectConfirmBtn();
    viewInCalendar();
    refreshPendingRequests();
    processApproveRejectRequestOnEventModal();
    handleProceedClick();
});

/**
 * Helper function that determines the click handler for the reject confirm btn.
 */
function setUpRejectConfirmBtn() {
    rejectBtn = $('#request-reject');

    if (rejectBtn.length) {
        rejectBtn.click((e) => {
            e.preventDefault();
            $(rejectBtn.data().parent).modal('hide');
            $('#request-reject-confirmation-prompt-modal').modal('hide')
            $('#comments-prompt-modal').modal('show');
        });
    }
}

/**
 * Sets up the click handle functionb for the proceed btn. 
 */
function handleProceedClick() {
    const proceedBtn = $('#comments-proceed');

    if (!proceedBtn.length) {
        return;
    }

    proceedBtn.click((e) => {
        $('#comments-prompt-modal').modal('hide');
        proceedWithRejecting(e);
    })
}

/**
 * Helper function to submit a rejection after comments are added or not.
 */
function proceedWithRejecting(e) {
    rejectBtn = $('#request-reject');
    submitAndHandleResponse(e, rejectBtn, "reject")
}

/**
 * Helper function for setting the click event handler for the reject 
 * and approve btns on the event modal.
 */
function processApproveRejectRequestOnEventModal() {
    sickDayApproveBtn_2 = $('#sick-event-approve-btn');
    vacationApproveBtn_2 = $('#vacation-event-approve-btn');

    sickDayApproveBtn_2.click(function (e) {
        submitAndHandleResponse(e, $(this));
    });

    vacationApproveBtn_2.click(function (e) {
        console.log($(this));
        submitAndHandleResponse(e, $(this));
    });
}

/**
 * Process the click event when approving a  request.
 */
function processApproveRequests() {
    // The approve btns.
    vacationApproveBtn_1 = $('.vacation-approve-btn');
    sickDayApproveBtn_1 = $('.sick-approve-btn');

    vacationApproveBtn_1.each(function () {
        const btn = $(this);
        btn.click((e) => submitAndHandleResponse(e, btn));
    });
    sickDayApproveBtn_1.each(function () {
        const btn = $(this);
        btn.click((e) => submitAndHandleResponse(e, btn));
    });

    vacationRejectBtn_2 = $('#vacation-event-reject-btn');
    sickDayRejectBtn_2 = $('#sick-event-reject-btn');

    vacationRejectBtn_2.click(function (e) {
        displayRejectPrompt(e, $(this));
    });
    sickDayRejectBtn_2.click(function (e) {
        displayRejectPrompt(e, $(this));
    });

}

/**
 * Process the click event when approving a  request.
 */
function processRejectRequests() {
    // The reject btns.
    vacationRejectBtn_1 = $('.vacation-reject-btn');
    sickDayRejectBtn_1 = $('.sick-reject-btn');

    vacationRejectBtn_1.each(function () {
        const btn = $(this);
        btn.click((e) => displayRejectPrompt(e, btn));
    });
    sickDayRejectBtn_1.each(function () {
        const btn = $(this);
        btn.click((e) => displayRejectPrompt(e, btn));
    });
}

/**
 * Helper function that passes on the id to the confirmation btn on the reject propmt modal.
 * 
 * @param {any} e
 *  The default click event.
 * @param {any} submitBtn
 *  The btn being clicked.
 */
function displayRejectPrompt(e, btn) {
    e.preventDefault();

    let id = btn.data().requestId

    if (!id) {
        id = btn.data().id;
    }

    rejectModal = $('#request-reject-confirmation-prompt-modal');
    rejectModalBody = $('#request-reject-confirmation-propmt-body');
    rejectBtn = $('#request-reject');

    const parentModal = btn[0].closest('.modal')
    let textValue = "Are you sure you want to reject this request?";

    rejectModalBody.text(textValue);
    rejectBtn.data('id', id);
    rejectBtn.data('parent', parentModal);
    rejectModal.modal('show');
}

/**
 * Helper function that submits a pending request for approval.
 * 
 * @param {any} e
 *  The default click event.
 * @param {any} submitBtn
 *  The btn being clicked. 
 * @param {any} action
 *  The action to be taken i.e. "reject", "approve", etc.
 */
function submitAndHandleResponse(e, submitBtn, action = "approve") {
    e.preventDefault();

    const parentModal = submitBtn[0].closest('.modal')
    displayWaitingModal(parentModal);

    let id = submitBtn.data().requestId

    if (!id) {
        id = submitBtn.data().id;
    }

    let comments = null;
    let commentsBox = null;

    if (action == "reject") {
        commentsBox = $('#comments-text');
        comments = commentsBox.val();
    }

    $.ajax({
        url: "/process-pending-request",
        method: 'POST',
        data: {
            requestId: id,
            action: action,
            comments: comments
        }
    })
        .then(res => {
            setTimeout(() => {
                commentsBox ? commentsBox.val('') : null;
                hideWaitingModal();
                displayConfirmationModal(res);
            }, 800);
        });
}

/**
 * Helper function that updates the pending requests container.
 * 
 * @param {any} res
 *   The response from the server.
 */
function updatePendingRquests(res) {
    const wrapper = $('#request-wrapper');
    const requestContainer = $('#request-container');
    const requestCount = $('#pending-request-count');
    const allDoneContainer = $('#all-done-container');
    const spinner = $('#spinner');
    const refreshBtn2 = $('#refresh-pending-btn2');

    // Set spinner if not already set.
    requestContainer.hide();
    wrapper.addClass('vh-100 d-flex align-items-center justify-content-center');
    allDoneContainer.hide();
    spinner.show();
    refreshBtn2.hide();

    if (res.content) {
        count = res.content.count;

        if (count) {
            setTimeout(function () {
                // Remove spinner and readjust container.
                wrapper.removeClass('vh-100 d-flex align-items-center justify-content-center');
                spinner.hide();

                // Show requests and refresh btn2.
                requestContainer.html(res.content.html)
                requestContainer.show();
                requestCount.text(count).show();
                refreshBtn2.show();
                allDoneContainer.hide();

                // Attach click events for the new elements.
                processApproveRequests();
                processRejectRequests();
                viewInCalendar();
            }, 500)
        }
        else {
            setTimeout(function () {
                // Remove spinner and readjust container.
                wrapper.removeClass('d-flex align-items-center justify-content-center');
                wrapper.addClass('vh-100')
                spinner.hide();

                requestCount.hide();
                allDoneContainer.show();
            }, 500)
        }
    }
}

/**
 * Helper function to that hides the counter element.
 */
function displayRequestCount() {
    const requestCount = $('#pending-request-count');

    if (requestCount.text() !== "0") {
        requestCount.show();
    }
}

/**
 * Takes the user to the request/event in the calendar.
 */
function viewInCalendar() {
    sickGoToBtn = $('.sick-goto-btn');
    vacationGoToBtn = $('.vacation-goto-btn');

    sickGoToBtn.each(function () {
        const btn = $(this);
        btn.click(() => {
            showInCalendar(btn);
        })
    })

    vacationGoToBtn.each(function () {
        const btn = $(this);
        btn.click(() => {
            showInCalendar(btn);
        })
    })
}

/**
 * Helper function to show event in calendar by adding an animation.
 * 
 * @param {any} btn
 *   A Jquery object.
 */
function showInCalendar(btn) {
    $('#calendar-menu-item')[0].click();

    const date = btn.data().startDate;
    const id = `.${btn.data().requestId}`;

    // Use the global calendar object.
    calendar.gotoDate(new Date(date));

    // Animate event.
    setTimeout(() => {
        $(id).effect("bounce", { distance: 50, times: 2 }, "slow");
    }, 500);
}

/**
 * Helper function for click event handler on the refresh pending requests btn.
 */
function refreshPendingRequests() {
    const wrapper = $('#request-wrapper');
    const requestContainer = $('#request-container');
    const allDoneContainer = $('#all-done-container');
    const spinner = $('#spinner');
    const refreshBtn2 = $('#refresh-pending-btn2');

    $('#refresh-pending-btn').click(() => {
        setUpAndPerformRefresh(requestContainer, allDoneContainer, refreshBtn2, wrapper, spinner);
    });

    $('#refresh-pending-btn2').click(() => {
        setUpAndPerformRefresh(requestContainer, allDoneContainer, refreshBtn2, wrapper, spinner)
    });
}

/**
 * Helper function that setup the dom elements associated with the request menu.
 * 
 * @param {any} requestContainer
 *   JQuery object of the request container.
 * @param {any} allDoneContainer
 *   JQuery object of the all done container.
 * @param {any} refreshBtn2
 *   JQuery object of the second refresh btn.
 * @param {any} wrapper
 *   JQuery object of the requests wrapper div.
 * @param {any} spinner
 *   JQuery object of the spinner element.
 */
function setUpAndPerformRefresh(
    requestContainer,
    allDoneContainer,
    refreshBtn2,
    wrapper,
    spinner
) {
    // Hide existing requests, if any.
    requestContainer.hide();

    // Hide all done if being displayed.
    allDoneContainer.hide();

    // Hide the second refresh btn.
    refreshBtn2.hide();

    wrapper.addClass('vh-100 d-flex align-items-center justify-content-center');
    spinner.show();

    fetchCalendarEvents();
}

/**
 * Checks if there's any new notifications and display them to the user.
 */
function getNewNotifications() {
    const notificationBody = $('#requests-notification-body');

    if (!notificationBody.length) {
        return;
    }

    $.ajax({
        url: '/get-notifications',
        method: 'POST'
    })
        .then(res => {
            const content = res.content;
            if (content) {
                notificationBody.html(content);
                displayNotificationModal();
            }
        });
}

/**
 * Fetch and updates any pending requests.
 */
function fetchPendingRequests() {
    const requestContainer = $('#request-container');
    const requestCount = $('#pending-request-count');
    const allDoneContainer = $('#all-done-container');

    if ((!requestCount.length || !requestContainer.length) && !allDoneContainer.length) {
        return;
    }

    $.ajax({
        url: "/get-pending-requests",
        method: 'POST',
    })
        .then(res => {
            updatePendingRquests(res);
        });
}