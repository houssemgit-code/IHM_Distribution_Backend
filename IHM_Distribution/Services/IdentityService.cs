using System.IdentityModel.Tokens.Jwt;

namespace IHM_Distribution.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly JwtSecurityToken token;

        public IdentityService(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor != null)
            {
                var context = httpContextAccessor.HttpContext;

                if (context != null && context.Request != null)
                {
                    // Retrieve the authorization header
                    string authorizationHeader = context.Request.Headers["Authorization"];
                    var bearerToken = string.Empty;

                    // Check if the authorization header is present and in the correct format
                    if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract the token
                        bearerToken = authorizationHeader.Substring("Bearer ".Length).Trim();
                        this.token = new JwtSecurityTokenHandler().ReadJwtToken(bearerToken);
                    }

                    this.IPAddress = context?.Connection?.RemoteIpAddress?.ToString();
                }
            }
        }

        public string IPAddress { get; }

        public string GetCurrentUserEmail()
        {
            var result = this.token?
                 .Claims?
                 .FirstOrDefault(c => c.Type == "email")?
                 .Value;

            return result ?? "DEBUG USER OR SWAGGER USER";
        }

        public string GetCurrentUserName()
        {
            return this.token?
                .Claims?
                .FirstOrDefault(c => c.Type == "name")?
                .Value;
        }
    }
}
