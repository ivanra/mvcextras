using Moq;
using System;
using System.Collections.Specialized;
using System.Web.Mvc;
using Xunit;

namespace MvcExtras.Test
{
    public class RequireHttps2AttributeTest
    {
        [Fact]
        public void ThrowsIfFilterContextNull()
        {
            var attr = new RequireHttps2Attribute();

            Assert.Throws<ArgumentNullException>(() => attr.OnAuthorization(null));
        }

        [Fact]
        public void NothingIfRequestIsSecure()
        {
            var mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.HttpContext.Request.IsSecureConnection).Returns(true);

            var result = new ContentResult();
            var authContext = mockAuthContext.Object;            
            authContext.Result = result;
            var attr = new RequireHttps2Attribute();

            attr.OnAuthorization(authContext);

            Assert.Same(result, authContext.Result);
        }

        [Fact]
        public void NothingIfRequestIsSecure_XForwardedProto()
        {
            var mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.HttpContext.Request.IsSecureConnection).Returns(false);
            mockAuthContext.Setup(c => c.HttpContext.Request.Headers)
                           .Returns(new NameValueCollection {{"X-Forwarded-Proto", "https"}});

            var result = new ContentResult();
            var authContext = mockAuthContext.Object;            
            authContext.Result = result;
            // also test the value of X-Forwarded-Proto HTTP header
            var attr = new RequireHttps2Attribute {InspectXForwardedProtoHeader = true};

            attr.OnAuthorization(authContext);

            Assert.Same(result, authContext.Result);
        }

        [Fact]
        public void RedirectIfRequestIsNotSecure()
        {
            var mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.HttpContext.Request.HttpMethod).Returns("GET");
            mockAuthContext.Setup(c => c.HttpContext.Request.IsSecureConnection).Returns(false);
            mockAuthContext.Setup(c => c.HttpContext.Request.RawUrl).Returns("/hello/world/index.html");
            mockAuthContext.Setup(c => c.HttpContext.Request.Url).Returns(new Uri("http://www.example.com:8000/"));

            var authContext = mockAuthContext.Object;
            var attr = new RequireHttps2Attribute();

            attr.OnAuthorization(authContext);
            var result = authContext.Result as RedirectResult;

            Assert.NotNull(result);
            Assert.Equal("https://www.example.com/hello/world/index.html", result.Url);
        }

        [Fact]
        public void RedirectIfRequestIsNotSecure_XForwardedProto()
        {
            var mockAuthContext = new Mock<AuthorizationContext>();
            mockAuthContext.Setup(c => c.HttpContext.Request.HttpMethod).Returns("GET");
            mockAuthContext.Setup(c => c.HttpContext.Request.IsSecureConnection).Returns(false);
            mockAuthContext.Setup(c => c.HttpContext.Request.RawUrl).Returns("/hello/world/index.html");
            mockAuthContext.Setup(c => c.HttpContext.Request.Url).Returns(new Uri("http://www.example.com:8000/"));
            mockAuthContext.Setup(c => c.HttpContext.Request.Headers)
                           .Returns(new NameValueCollection {{"X-Forwarded-Proto", "http"}});

            var authContext = mockAuthContext.Object;
            // also test the value of X-Forwarded-Proto HTTP header
            var attr = new RequireHttps2Attribute {InspectXForwardedProtoHeader = true};

            attr.OnAuthorization(authContext);
            var result = authContext.Result as RedirectResult;

            Assert.NotNull(result);
            Assert.Equal("https://www.example.com/hello/world/index.html", result.Url);
        }

    }
}
