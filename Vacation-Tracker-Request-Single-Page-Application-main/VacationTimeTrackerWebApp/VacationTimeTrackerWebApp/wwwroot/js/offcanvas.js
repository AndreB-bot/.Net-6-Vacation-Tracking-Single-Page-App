$(document).ready(() => {
    $('.offcanvas-start').each(function () {
        this.style.left = $('header').width() + "px";
    });

    doRecalculationOfLeft();
})

/**
 * Calculates the estimated position from where each offcanvas should start.
 */
function doRecalculationOfLeft() {
    $(window).resize(() => {

        $('.offcanvas-start').each(function () {
            this.style.left = $('header').width() + "px";
        });
    })
}