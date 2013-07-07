using System;
using System.Web.Mvc;

namespace MvcExtras
{
    /// <summary>
    /// StrictTransportSecurityAttribute - render Strict-Transport-Security HTTP header on response (as per RFC 6797: http://tools.ietf.org/html/rfc6797).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class StrictTransportSecurityAttribute : FilterAttribute, IResultFilter
    {
        private const string StrictTransportSecurityHeader = "Strict-Transport-Security";
        private const int DefaultMaxAge = 60;

        public StrictTransportSecurityAttribute()
        {
            MaxAge = DefaultMaxAge;
        }

        /// <summary>
        /// Value of the max-age directive. Default is 60 (seconds).
        /// </summary>
        public int MaxAge { get; set; }

        /// <summary>
        /// Determines whether includeSubDomains directive is to be included as part of the header value. Default is false.
        /// </summary>
        public bool IncludeSubdomains { get; set; }

        public virtual void OnResultExecuting(ResultExecutingContext filterContext)
        {
            if (filterContext == null)
                throw new ArgumentNullException("filterContext");

            //if (!filterContext.HttpContext.Request.IsSecureConnection)
            //    return;

            var headerValue = String.Format("max-age={0}{1}", MaxAge,
                                            IncludeSubdomains ? "; includeSubDomains" : String.Empty);
            filterContext.HttpContext.Response.Headers.Set(StrictTransportSecurityHeader, headerValue);
        }

        public virtual void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}
