let userFormSubmitted = false;
let modalOptions = {};

document.addEventListener('DOMContentLoaded', function () {
    handleTimeoffSubmission();
    handleRemoveEventSubmission();
    handleAddUserFormSubmission();
    displayRemoveUserConfirmationModal();
    handleRemoveUserFormSubmission();
    handleUpdateUserSubmission();
});

/**
 * Handles timeoff submission and the sequence of modals to follow.
 */
function handleTimeoffSubmission() {
    $('#add-timeslot-submit').click(function (e) {
        e.preventDefault();

        const submissionForm = $('#timeoff-submission-form');

        if (!checkFormValidity(submissionForm)) {
            return;
        }

        displayWaitingModal("add-timeslot-modal");

        $.ajax({
            method: "POST",
            url: "/request-submission",
            data: submissionForm.serialize()
        })
            .then(res => {
                triggerConfirmationModal(res);
            });
    })
}

/**
 * Handles remove form submission.
 */
function handleRemoveEventSubmission() {

    // We need know which form is currently open.
    setUpEventRemoveModals("vacation");
    setUpEventRemoveModals("sick");
    setUpEventRemoveModals("stat");
    setUpEventRemoveModals("company");

    // Submit post request for remove an event via the confirmation modal.
    $('.remove-event').each(function () {
        $(this).click(function (e) {
            e.preventDefault();

            modalOptions.modal.modal('hide');
            displayWaitingModal(modalOptions.confirmationPropmtName);

            $.ajax({
                url: '/remove-event',
                method: 'POST',
                data: modalOptions.btn.data()
            })
                .then(res => {
                    triggerConfirmationModal(res);
                })
        })
    })
}

function setUpEventRemoveModals(type) {
    $('#' + type + '-event-details-modal').on('show.bs.modal', function () {
        modalOptions.btn = $('#' + type + '-event-submit-id');
        modalOptions.modal = $('#' + type + '-event-details-modal');
        modalOptions.confirmationPropmtName = type + "-event-confirmation-prompt-modal";
    });
}

/**
 * Displays the confirmation propmt modal for removing an employee/user.
 */
function displayRemoveUserConfirmationModal() {
    $('#remove-user-submit-btn').click(function (e) {
        e.preventDefault();

        const removeUserForm = $('#remove-user-form');

        if (!checkFormValidity(removeUserForm)) {
            return;
        }

        const name = $('#remove-user-form').serializeArray()[0].value;

        // Populate confirmation prompt modal.
        let confirmationPromptBody = $('#remove-user-confirmation-propmt-body');
        let textValue = "Are you sure you want to remove %user%?"
            .replaceAll("%user%", name);

        confirmationPromptBody.text(textValue);

        // Display the modal.
        $('#remove-user-confirmation-prompt-modal').modal('show');

    });
}

/**
 * Handles the remove user form submission.
 */
function handleRemoveUserFormSubmission() {
    $('#remove-user-submit').click(() => {

        //Submit the form details.
        userFormSubmitted = true;
        displayWaitingModal('update-user-modal');

        // Hide the confirmation prompt modal.
        $('#remove-user-confirmation-prompt-modal').modal('hide');

        $.ajax({
            url: '/remove-user',
            method: 'POST',
            data: $('#remove-user-form').serializeArray()
        })
            .then(res => {
                triggerConfirmationModal(res, true);
            });
    });
}


/**
 * Helper function for form submission in the user menu.
 */
function handleAddUserFormSubmission() {
    $('#add-user-submit-btn').click(function (e) {
        e.preventDefault();

        const addUserForm = $('#add-user-form');

        if (!checkFormValidity(addUserForm)) {
            return;
        }

        let firstName = $("#add-user-firstname").val();
        firstName = firstName.trim();
        firstName = firstName.charAt(0).toUpperCase() + firstName.slice(1);

        let lastName = $("#add-user-lastname").val();
        lastName = lastName.trim();
        lastName = lastName.charAt(0).toUpperCase() + lastName.slice(1);

        const email = $("#add-user-email").val();
        const fullName = `${firstName} ${lastName}`;

        // Perfom a preprocess check on name and email.
        $.ajax({
            method: 'POST',
            url: '/existing-user',
            data: {
                'email': email,
                'fullName': fullName
            }
        })
            .then(res => {
                if (res.title === "Oops!") {
                    displayInfoModal(res);
                    return;
                }

                //Submit the form details.
                userFormSubmitted = true;
                displayWaitingModal('add-user-modal');

                $.ajax({
                    method: 'POST',
                    url: '/add-user',
                    data: addUserForm.serialize()
                })
                    .then(res => {
                        triggerConfirmationModal(res, true);
                    })
            })
    })
}


/**
 * Helper function for form submission in the user menu.
 */
function handleUpdateUserSubmission() {
    $('#update-user-submit-btn').click(function (e) {
        e.preventDefault();

        const updateUserForm = $('#update-user-form');

        if (!checkFormValidity(updateUserForm)) {
            return;
        }

        let firstName = $("#update-user-first-name").val();
        firstName = firstName.trim();
        firstName = firstName.charAt(0).toUpperCase() + firstName.slice(1);

        let lastName = $("#update-user-last-name").val();
        lastName = lastName.trim();
        lastName = lastName.charAt(0).toUpperCase() + lastName.slice(1);

        const nameFromList = $('#update-user-employee-list').val();
        const email = $("#update-user-email").val();
        const fullName = `${firstName} ${lastName}`;

        // Perfom a preprocess check on name and email.
        $.ajax({
            method: 'POST',
            url: '/existing-user',
            data: {
                'email': email,
                'fullName': fullName,
                'nameFromList': nameFromList,
            }
        })
            .then(res => {
                if (res.title === "Oops!") {
                    displayInfoModal(res);
                    return;
                }

                //Submit the form details.
                userFormSubmitted = true;
                displayWaitingModal('update-user-modal');

                $.ajax({
                    url: '/update-user',
                    method: 'POST',
                    data: updateUserForm.serialize()
                })
                    .then(res => {
                        if (res.title === "Oops!") {
                            triggerConfirmationModal(res)
                            return;
                        }

                        triggerConfirmationModal(res, true);
                    });
            });
    })
}

/**
 * Helper to help trigger the display modal after a few seconds.
 * 
 * @param {any} res
 *   The incoming reponse from the server.
 */
function triggerConfirmationModal(res, getEmployees = false) {
    setTimeout(() => {
        hideWaitingModal();
        displayConfirmationModal(res);
    }, 800);

    // Update employee list.
    if (getEmployees) {
        fetchEmployeeNames();
    }
}

/**
 * Helper function to check for input validity.
 * 
 * @param {any} form
 *  JQuery form object.
 */
function checkFormValidity(form) {
    return form[0].reportValidity();
}