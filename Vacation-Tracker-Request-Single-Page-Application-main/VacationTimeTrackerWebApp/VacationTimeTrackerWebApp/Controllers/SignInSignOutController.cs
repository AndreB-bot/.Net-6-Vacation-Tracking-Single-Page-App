using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VacationTimeTrackerWebApp.Controllers
{
    /// <summary>
    /// Responsible for routing to Google Sign-In.
    /// </summary>
    [AllowAnonymous, Route("user")]
    public class SignInSignOutController : Controller
    {

        /// <summary>
        /// The default route for Google Sign-in OAuth.
        /// </summary>
        /// <returns>The created Microsoft.AspNetCore.Mvc.ChallengeResult for the response.</returns>
        [Route("google-signin")]
        public IActionResult GoogleLogin()
        {
            var properties = new AuthenticationProperties
            {
                RedirectUri = "user/google-response"
            };

            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        /// <summary>
        /// Helper function to allow time for all details to properly come over from Google.
        /// </summary>
        /// <returns> The created Microsoft.AspNetCore.Mvc.LocalRedirectResult for the response.</returns>
        [Route("google-response")]
        public IActionResult GoogleResponse()
        {
            return LocalRedirect("~/VacationApp");
        }

        /// <summary>
        /// Logout an user and redirects them to the homepage.
        /// </summary>
        /// <returns>The created Microsoft.AspNetCore.Mvc.RedirectToActionResult for the response.</returns>
        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return LocalRedirect("~/");
        }
    }
}
