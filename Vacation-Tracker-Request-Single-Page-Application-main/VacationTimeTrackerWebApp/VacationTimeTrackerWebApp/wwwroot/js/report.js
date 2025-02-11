$(document).ready(function () {
    handleSearch();
    generatePdf();
    reloadReportData()
});

/**
 * Enables search functionality of report.
 */
function handleSearch() {
    const noResults = $('#no-results');

    $(".search").keyup(function () {
        let searchTerm = $(".search").val();
        const listItems = $(".name");

        listItems.each(function () {
            let name = this.textContent;

            if (!name.match(new RegExp(searchTerm, 'i'))) {
                $(this.parentNode).addClass('d-none');
            }

            if (name.match(searchTerm)) {
                $(this.parentNode).removeClass('d-none');
            }
        })

        const parents = $('#report-body tr:not(".no-results")');
        const allItemsHidden = parents.toArray().every(el => $(el).hasClass('d-none'));

        if (allItemsHidden) {
            noResults.removeClass('d-none');
        }
        else {
            noResults.addClass('d-none');
        }
    });
}

/**
 * Creates a downloadable pdf version of the form.
 */
function generatePdf() {
    $('#pdf-report').click(() => {
        let pdf = new jspdf.jsPDF()
        pdf.autoTable({
            html: '#report-table',
            styles: {
                halign: 'center'
            },
            fontSize: 8,
        })

        const date = moment().format('MM/DD/YYYY');
        const fileName = `Vacation-SickDaysReport-${date}.pdf`;

        pdf.save(fileName,
            { returnPromise: true }
        )
            .then(alert('Download completed'));
    });
}

/**
 * Determines the click event handler for the report refresh btn.
 */
function reloadReportData() {
    $('#reload-report').click(() => {
        //First fecth the data.
        fetchReportEntries()
    });
}

/**
 * Fetches updated data from the server.
 */
function fetchReportEntries() {
    $.ajax({
        url: '/get-report-entries',
        method: 'GET'
    })
        .then(res => {
            if (res.content !== "") {
                const reportBody = $('#report-body');
                // Fade the old data and then show new data.
                reportBody.fadeTo(500, 0, () => {
                    reportBody.html(res.content);
                    reportBody.fadeTo(500, 1);
                });
            }
        });
}