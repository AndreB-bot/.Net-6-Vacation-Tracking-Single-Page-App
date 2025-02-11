using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using VacationTimeTrackerWebApp.Data;

namespace VacationTimeTrackerWebApp.ClaimsTransformation
{
    /// <summary>
    /// Provides new claims for each authorized account/user.
    /// </summary>
    public class GoogleClaimsTransformation : IClaimsTransformation
    {
        /// <summary>
        /// The DBContext instance at runtime.
        /// </summary>
        private readonly IDBContext _DbContext;

        /// <summary>
        /// Contructor.
        /// </summary>
        /// <param name="context">The DBContext instance.</param>
        public GoogleClaimsTransformation(IDBContext dbContext)
        {
            _DbContext = dbContext;
        }

        /// <inheritDoc />
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // Check for email cliam.
            if (!principal.HasClaim(c => c.Type == ClaimTypes.Email))
            {
                return Task.FromResult(principal);
            }

            var currentEmail = principal.Claims.FirstOrDefault(
               x => x.Type == ClaimTypes.Email
            )?.Value;

            var employee = _DbContext.EmployeesRepo.CreateEmployeeWithEntitlements(currentEmail);

            if (employee != null)
            {
                var authorisedClaimType = "Authorized";
                ClaimsIdentity authorisedClaimTye = new ClaimsIdentity();
                authorisedClaimTye.AddClaim(new Claim(authorisedClaimType, "True"));
                principal.AddIdentity(authorisedClaimTye);

                if (employee.IsAdmin())
                {
                    ClaimsIdentity accessIdentity = new ClaimsIdentity();
                    var accessClaimType = "AccessType";
                    accessIdentity.AddClaim(new Claim(accessClaimType, "Admin"));
                    principal.AddIdentity(accessIdentity);
                }
            }

            return Task.FromResult(principal);
        }
    }
}
