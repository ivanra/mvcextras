using System;
using System.Web;

namespace MvcExtras
{
    /// <summary>
    /// XForwardedExtensions -- a few extension methods to deal with X-Forwarded-* HTTP headers.
    /// </summary>
    public static class XForwardedExtensions
    {
        private const string XForwardedForHeader = "X-Forwarded-For";
        private const char XForwardedForSeparator = ',';
        private const string XForwardedProtoHeader = "X-Forwarded-Proto";
        private const string XForwardedPortHeader = "X-Forwarded-Port";
        
        /// <summary>
        /// Returns client IP address as reported by X-Forwarded-For HTTP header (the last IP in the chain).
        /// If the HTTP header is not present, Request.UserHostAddress is returned.
        /// </summary>
        public static string UserHostAddress(this HttpRequestBase request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            string ipAddress = null;
            var fwdFor = request.Headers[XForwardedForHeader];

            if (!String.IsNullOrWhiteSpace(fwdFor))
            {
                var idx = fwdFor.LastIndexOf(XForwardedForSeparator);
                ipAddress = fwdFor.Substring(idx > 0 ? idx + 1 : 0).Trim();
            }

            return !String.IsNullOrWhiteSpace(ipAddress) ? ipAddress : request.UserHostAddress;
        }

        /// <summary>
        /// Returns the value of X-Forwarded-Port HTTP header if one is set and valid; otherwise returns -1.
        /// </summary>
        public static int ForwardedPort(this HttpRequestBase request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            var fwdPort = request.Headers[XForwardedPortHeader];

            int port;
            if (String.IsNullOrWhiteSpace(fwdPort) || !Int32.TryParse(fwdPort, out port))
                port = -1;

            return port;
        }

        /// <summary>
        /// Returns true if a client request is made over HTTPS connection, or if a request is made over HTTP connection
        /// and X-Forwarded-Proto HTTP request header is set to "https".
        /// </summary>
        public static bool IsSecureConnection(this HttpRequestBase request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            return request.IsSecureConnection ||
                   (String.Equals(request.Headers[XForwardedProtoHeader], Uri.UriSchemeHttps /* https */,
                                  StringComparison.OrdinalIgnoreCase));
        }

    }

}
