using System;
using System.Web.Mvc;

namespace MvcExtras
{
    /// <summary>
    /// RequireHttps2Attribute - optionally inspect X-Forwarded-Proto HTTP header to determine if HTTPS is used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RequireHttps2Attribute : RequireHttpsAttribute
    {
        private const string XForwardedProtoHeader = "X-Forwarded-Proto";

        /// <summary>
        /// Determines whether X-Forwarded-Proto HTTP header will be tested for "https" value as well.
        /// The HTTP header is not inspected by default.
        /// </summary>
        public bool InspectXForwardedProtoHeader { get; set; }

        public override void OnAuthorization(AuthorizationContext filterContext) {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            // if not secure connection, test the value of X-Forwarded-Proto HTTP header but only if InspectXForwardedProtoHeader is set to true
            if (!filterContext.HttpContext.Request.IsSecureConnection &&
                (!InspectXForwardedProtoHeader ||
                 !String.Equals(filterContext.HttpContext.Request.Headers[XForwardedProtoHeader],
                                Uri.UriSchemeHttps /* https */, StringComparison.OrdinalIgnoreCase)))
                HandleNonHttpsRequest(filterContext);
        }
    }
}
