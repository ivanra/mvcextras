using System;
using Moq;
using System.Collections.Specialized;
using System.Web;
using Xunit;
using Xunit.Extensions;

namespace MvcExtras.Test
{
    public class XForwardedExtensionsTest
    {

        [Fact]
        public void UserHostAddress_ThrowsIfRequestNull()
        {
            Assert.Throws<ArgumentNullException>(() => XForwardedExtensions.UserHostAddress(null));
        }

        [Theory]
        [InlineData("192.168.0.100", "192.168.0.100")]
        [InlineData("10.0.0.1, 192.168.0.101", "192.168.0.101")]
        [InlineData("192.168.1.100,10.0.0.1, 192.168.0.105,192.168.0.110", "192.168.0.110")]
        [InlineData("", "192.168.0.1")]
        [InlineData(null, "192.168.0.1")]
        public void UserHostAddress_ParseHeader(string headerValue, string expectedAddress)
        {
            var httpRequestMock = new Mock<HttpRequestBase>();
            httpRequestMock.Setup(r => r.UserHostAddress).Returns("192.168.0.1");
            var headers = headerValue == null
                              ? new NameValueCollection()
                              : new NameValueCollection {{"X-Forwarded-For", headerValue}};
            httpRequestMock.Setup(r => r.Headers).Returns(headers);
            var httpRequest = httpRequestMock.Object;

            var addr = httpRequest.UserHostAddress();

            Assert.NotNull(addr);
            Assert.Equal(expectedAddress, addr);
        }

        [Fact]
        public void ForwardedPort_ThrowsIfRequestNull()
        {
            Assert.Throws<ArgumentNullException>(() => XForwardedExtensions.ForwardedPort(null));
        }

        [Theory]
        [InlineData("443", 443)]
        [InlineData("invalid", -1)]
        [InlineData("", -1)]
        [InlineData(null, -1)]
        public void ForwardedPort_ParseHeader(string headerValue, int expectedPort)
        {
            var httpRequestMock = new Mock<HttpRequestBase>();
            var headers = headerValue == null
                              ? new NameValueCollection()
                              : new NameValueCollection {{"X-Forwarded-Port", headerValue}};
            httpRequestMock.Setup(r => r.Headers).Returns(headers);
            var httpRequest = httpRequestMock.Object;

            var addr = httpRequest.ForwardedPort();

            Assert.Equal(expectedPort, addr);
        }

        [Fact]
        public void IsSecureConnection_ThrowsIfRequestNull()
        {
            Assert.Throws<ArgumentNullException>(() => XForwardedExtensions.IsSecureConnection(null));
        }

        [Theory]
        [InlineData("http", false, false)]
        [InlineData("http", true, true)]
        [InlineData("https", false, true)]
        [InlineData("https", true, true)]
        [InlineData("", false, false)]
        [InlineData("", true, true)]
        [InlineData("invalid", false, false)]
        [InlineData("invalid", true, true)]
        [InlineData(null, false, false)]
        [InlineData(null, true, true)]
        public void IsSecureConnection_ParseHeader(string headerValue, bool isSecureConnection, bool expectedValue)
        {
            var httpRequestMock = new Mock<HttpRequestBase>();
            var headers = headerValue == null
                              ? new NameValueCollection()
                              : new NameValueCollection {{"X-Forwarded-Proto", headerValue}};
            httpRequestMock.Setup(r => r.Headers).Returns(headers);
            httpRequestMock.Setup(s => s.IsSecureConnection).Returns(isSecureConnection);
            var httpRequest = httpRequestMock.Object;

            var isHttps = httpRequest.IsSecureConnection();

            Assert.Equal(expectedValue, isHttps);
        }

    }
}
