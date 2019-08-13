using Microsoft.AspNetCore.Authorization;

namespace CommonHelper.Filter
{
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        public const string JwtAuthenticationScheme = "JwtAuthenticationScheme";

        public JwtAuthorizeAttribute()
        {
            this.AuthenticationSchemes = JwtAuthenticationScheme;
        }
    }
}
