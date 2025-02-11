using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace VacationTimeTrackerWebApp.Pages
{
    /// <summary>
    /// Reponsible for HTTP requests to the homepage.
    /// </summary>
    [AllowAnonymous]
    public class IndexModel : PageModel
    {
        /// <summary>
        /// The IActionResult handler for GET requests to the homepage.
        /// </summary>
        /// <returns></returns>
        public IActionResult OnGet()
        {
            // Check if user is aleready logged in and redirect.
            if (this.User.HasClaim("Authorised", "True"))
            {
                return LocalRedirect("/VacationApp");
            }

            return Page();
        }

    }
}