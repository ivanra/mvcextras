using System;
using System.Web.Mvc;

namespace MvcExtras
{
    /// <summary>
    /// StrictTransportSecurityAttribute -- render Strict-Transport-Security HTTP header on response (as per RFC 6797: http://tools.ietf.org/html/rfc6797)
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class StrictTransportSecurityAttribute : FilterAttribute, IResultFilter
    {
        private const string StrictTransportPolicyHeader = "Strict-Transport-Security";
        private const int DefaultMaxAge = 60;

        public StrictTransportSecurityAttribute()
        {
            MaxAge = DefaultMaxAge;
        }

        public int MaxAge { get; set; }

        public bool IncludeSubdomains { get; set; }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            //if (!filterContext.HttpContext.Request.IsSecureConnection)
            //    return;

            var headerValue = String.Format("max-age={0}{1}", MaxAge,
                                            IncludeSubdomains ? "; includeSubDomains" : String.Empty);
            filterContext.HttpContext.Response.AddHeader(StrictTransportPolicyHeader, headerValue);
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}
