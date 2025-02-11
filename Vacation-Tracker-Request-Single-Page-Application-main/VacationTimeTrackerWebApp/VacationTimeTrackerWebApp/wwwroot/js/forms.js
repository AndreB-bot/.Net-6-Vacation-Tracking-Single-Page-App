document.addEventListener('DOMContentLoaded', function () {
    pulsateCalendar();
    preventCharCodes();
    handleUserUpdateSelectList();
    handleRequestTypeSelected();
});

/**
 * Helper function help bring attention to the calendar icon.
 */
function pulsateCalendar() {
    $('.user-start-date').each(function () {
        $(this).focus(() => {
            $('#' + this.dataset.calendar).effect("pulsate", { times: 1 }, 300);
        })
    })
}

function preventCharCodes() {
    // Prevents 'e' of '.' from being entered into number fields.
    $('[type="number"]').keypress(function (e) {
        const charCode = !e.charCode ? e.which : e.charCode;

        if (charCode === 69 || charCode === 101 || charCode === 46) {
            e.preventDefault();
        }
    })
}

/**
 * Helper function handle display current user/employee details
 * when an option from the update select list is chosen.
 */
function handleUserUpdateSelectList() {
    const employeeList = $("#update-user-employee-list");

    employeeList.change(() => {
        const fullname = employeeList.val().split(",")[0];

        $.ajax({
            url: '/user-details',
            method: 'POST',
            data: {
                name: fullname
            }
        })
            .then(res => {
                if (res.title === "Oops!") {
                    displayInfoModal(res);
                    return;
                }

                $('#update-user-access-list').val(res.accessLevel);
                $('#update-user-first-name').val(res.firstName);
                $('#update-user-last-name').val(res.lastName);
                $('#update-user-email').val(res.email);
                $('#update-user-id').val(res.id);
                $('#update-user-start-date').val(res.startDate);
                $('#update-user-vacation').val(res.vacation);
                $('#update-user-sick').val(res.sick);
            })
    })
}

/**
 * Checks if the request is Stat or Company and disable the unneeded fields.
 */
function handleRequestTypeSelected() {
    $('#select-request-type').change(function () {
        const value = this.value;
        const selectEmployees = $('#select-employees')[0];
        const selectContainer = $('#select-employees-container');
        const titleInput = $('#request-title');
        const titleContainer = $('#request-title-container');
        const endDateContainer = $("#end-datetime-picker");
        const endDate = endDateContainer.find(':input');

        if (value === 'stat' || value === 'company') {
            selectEmployees.disabled = true;
            selectContainer.hide();
            titleInput[0].type = "text";
            titleContainer.removeClass('d-none');
            endDateContainer.addClass('pe-none');
            endDate.addClass('disabled');
        }
        else if (selectEmployees) {
            selectEmployees.disabled = false;
            selectContainer.show();
            titleInput[0].type = "hidden";
            titleContainer.addClass('d-none');
            endDateContainer.removeClass('pe-none');
            endDate.removeClass('disabled');
        }
    })
}