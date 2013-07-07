using Moq;
using System;
using System.Web.Mvc;
using Xunit;
using Xunit.Extensions;

namespace MvcExtras.Test
{
    public class StrictTransportSecurityAttributeTest
    {

        [Fact]
        public void ThrowsIfFilterContextNull()
        {
            Assert.Throws<ArgumentNullException>(() => new StrictTransportSecurityAttribute().OnResultExecuting(null));
        }

        [Theory]
        [InlineData(null, null, "max-age=60")]
        [InlineData(3600, null, "max-age=3600")]
        [InlineData(null, true, "max-age=60; includeSubDomains")]
        [InlineData(3600, true, "max-age=3600; includeSubDomains")]
        public void DefaultMaxAge(int? maxAge, bool? includeSubdomains, string expectedHeaderValue)
        {
            var mockResultExecutingContext = new Mock<ResultExecutingContext>();
            mockResultExecutingContext.Setup(
                c => c.HttpContext.Response.AddHeader("Strict-Transport-Security", expectedHeaderValue))
                                      .Verifiable("Invalid Strict-Transport-Security header.");
            var resultExecutingContext = mockResultExecutingContext.Object;
            
            var attr = new StrictTransportSecurityAttribute();            
            if (maxAge.HasValue)
                attr.MaxAge = maxAge.Value;
            if (includeSubdomains.HasValue)
                attr.IncludeSubdomains = includeSubdomains.Value;

            attr.OnResultExecuting(resultExecutingContext);

            mockResultExecutingContext.Verify();
        }

    }
}
