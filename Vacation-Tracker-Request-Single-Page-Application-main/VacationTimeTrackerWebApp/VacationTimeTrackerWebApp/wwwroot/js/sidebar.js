const shrink_btn = document.querySelector(".shrink-btn");
const sidebar_links = document.querySelectorAll(".sidebar-links a");
const active_tab = document.querySelector(".active-tab");
const shortcuts = document.querySelector(".sidebar-links h4");
const tooltip_elements = document.querySelectorAll(".tooltip-element");

let activeIndex;

document.addEventListener('DOMContentLoaded', function () {
    //setShrinkBtnListner()
    sidebar_links.forEach((link) => handleClick(link));
    removeBackdrop();
});

/**
 * Sets up the click event for the shrink btn.
 */
function setShrinkBtnListner() {
    shrink_btn.addEventListener("click", () => {
        document.body.classList.toggle("shrink");
        setTimeout(moveActiveTab, 400);

        shrink_btn.classList.add("hovered");

        setTimeout(() => {
            shrink_btn.classList.remove("hovered");
        }, 500);
    });
}

/**
 * Helper function to handle click event on menu items.
 * 
 * @param {any} el
 */
function handleClick(el) {
    el.addEventListener("click", function (e) {
        e.preventDefault();

        // Travers all sidebar links.
        sidebar_links.forEach(function (link) {
            link.classList.remove("active");

            const relatedCanvas = document.getElementById(link.dataset.href);

            // Allows for the same button to be clicked mulitple times 
            // without toggling the associated canvas.
            if (relatedCanvas) {
                if (relatedCanvas.classList.contains('show') && link !== el) {
                    relatedCanvas.style.removeProperty("visibility");
                    relatedCanvas.classList.remove('show');
                }

                if (link === el) {
                    relatedCanvas.classList.add('show');
                    relatedCanvas.style.visibility = "visible";
                }
            }
        });

        el.classList.add("active");
        activeIndex = el.dataset.active;

        moveActiveTab();
    });
}

/**
 * Moves the active background to the current menu item.
 */
function moveActiveTab() {
    let topPosition = activeIndex * 58 + 2.5;

    if (activeIndex > 3) {
        topPosition += shortcuts.clientHeight;
    }

    active_tab.style.top = `${topPosition}px`;
}

/**
 * Dispolays the tool tip. 
 */
function showTooltip() {
    let tooltip = this.parentNode.lastElementChild;
    let spans = tooltip.children;
    let tooltipIndex = this.dataset.tooltip;

    Array.from(spans).forEach((sp) => sp.classList.remove("show"));
    spans[tooltipIndex].classList.add("show");

    tooltip.style.top = `${(100 / (spans.length * 2)) * (tooltipIndex * 2 + 1)}%`;
}

/**
 * Helper function remove off canvas backdrop.
 */
function removeBackdrop() {
    setTimeout(() => {
        const backdrop = $(".offcanvas-backdrop");

        if (!backdrop) {
            removeBackdrop();
        }

        $(backdrop).each(function () {
            this.remove();
        });

    }, 500)
}