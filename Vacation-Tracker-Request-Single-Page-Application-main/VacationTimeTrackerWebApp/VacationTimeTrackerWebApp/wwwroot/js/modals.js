document.addEventListener('DOMContentLoaded', function () {
    clearFormWhenClosed();
    handleUpdateUserToggle();
    handleUserActionMenus();
    pulsateCalendar();
    blurUserModal();
    updateAppOnConfirmClose();
    clearCommentsModalOnClose();
});

/***
 * Clears comments modal textarea on close.
 */
function clearCommentsModalOnClose() {
    const commentsModal = $('#comments-prompt-modal');

    if (!commentsModal.length) {
        return;
    }

    commentsModal.on('hidden.bs.modal', () => {
        $('#comments-text').val('');
    })
}

/**
 * Displays the waiting modal.
 * 
 * @param {any} idOrModal
 *   The id of the current modal being displayed, or the modal itself.
 */
function displayWaitingModal(idOrModal = null) {
    if (typeof idOrModal === 'string') {
        $('#' + idOrModal).modal("hide");
    }

    if (typeof idOrModal === 'object') {
        $(idOrModal).modal("hide");
    }

    $("#wait-modal").modal({
        backdrop: "static",
        keyboard: false,
    }).modal("show");
}

/**
 * Hides the waiting modal.
 */
function hideWaitingModal() {
    $("#wait-modal").modal("hide");
}

/**
 * Displays the confirmation modal.
 * 
 * @param {any} details
 *  The details to place in the modal; usually coming from a server response.
 */
function displayConfirmationModal(details) {
    $('#confirmation-modal-body').text(details.body);
    $('#confirmation-modal-title').text(details.title)

    $('#confirmation-modal').modal("show");
}

/**
 * Helper function that updates the App after confirmation closed.
 */
function updateAppOnConfirmClose() {
    $('#confirmation-modal').on('hidden.bs.modal', () => {
        updateApp();
    });
}

/**
 * Resets from values when closed.
 */
function clearFormWhenClosed() {
    const selectElement = $('#select-employees')[0];
    const selectContainer = $('#select-employees-container');
    const titleInput = $('#request-title');
    const titleContainer = $('#request-title-container');
    const endDateContainer = $("#end-datetime-picker");
    const endDate = endDateContainer.find(':input');

    $('#add-timeslot-modal').on('hidden.bs.modal', function () {
        $('#timeoff-submission-form')[0].reset();

        // Resets the hidden field.
        if (titleInput[0] && titleInput[0].type !== "hidden") {
            selectElement.disabled = false;
            selectContainer.show();
            titleInput[0].type = "hidden";
            titleContainer.addClass('d-none');
            endDateContainer.removeClass('pe-none');
            endDate.removeClass('disabled');
        }
    })
}

/**
 * Handle toggleOptions being checked.
 */
function handleUpdateUserToggle() {
    const updateButton = $('#update');
    const removeButton = $('#remove');

    updateButton.click(function () {
        $(".card-3d-wrapper")[0].style.transform = null;
    })

    removeButton.click(function () {
        $(".card-3d-wrapper")[0].style.transform = "rotateY(180deg)";
    })
}

/**
 * Handles displaying the respective modals in the user menu.
 */
function handleUserActionMenus() {
    const userCanvas = $('#users-canvas')[0];
    const updateUserModal = $('#update-user-modal');
    const addUserModal = $('#add-user-modal');
    const updateCard = $('#update-user-card');
    const addCard = $('#add-user-card');

    addCard.click(() => {
        addCard.mouseleave();
        addUserModal.modal('show');
        userCanvas.style.filter = "blur(8px)";
    })

    addUserModal.on('hidden.bs.modal', () => {
        // If the form isn't submitted, remove blur.
        if (!userFormSubmitted) {
            userCanvas.style.filter = null;
        }

        $('#add-user-form')[0].reset();
    })

    updateCard.click(function () {
        updateCard.mouseleave();
        updateUserModal.modal('show');
        userCanvas.style.filter = "blur(8px)";
    })

    updateUserModal.on('hidden.bs.modal', () => {
        // If the form isn't submitted, remove blur.
        if (!userFormSubmitted) {
            userCanvas.style.filter = null;
        }

        $('#update-user-form')[0].reset();
        $('#remove-user-form')[0].reset();
        $('#update').click();
    })

    updateUserModal.on('show.bs.modal', () => {
        $('#update-employee-start-date').datetimepicker(datePickerOptions);
    })

    addUserModal.on('show.bs.modal', () => {
        $('#employee-start-date').datetimepicker(datePickerOptions);
    })
}

/**
 * Display the Info modal when adding a new employee.
 * 
 * @param {any} res
 *   JSON object.
 */
function displayInfoModal(res) {
    $('#user-info-prompt-modal-title').text(res.title);
    $('#user-info-prompt-modal-body').text(res.body);
    $('#user-info-prompt-modal').modal('show');
}

/**
 * Blurs the update-modal whenever the confirmation prompt modal appears.
 */
function blurUserModal() {
    $('#remove-user-confirmation-prompt-modal').on("show.bs.modal", function () {
        $('#update-user-modal')[0].style.filter = "blur(2px)";
    })

    $('#remove-user-confirmation-prompt-modal').on("hide.bs.modal", function () {
        $('#update-user-modal')[0].style.filter = null;
    })
}

/**
 * Displays the notification modal.
 */
function displayNotificationModal() {
    $('#requests-notification-modal').modal("show");
}