using Microsoft.VisualStudio.TestTools.UnitTesting;
using FunMetrics.Helpers;

namespace FunMetrics.Tests.Helpers
{
    [TestClass]
    public class RedirectTests
    {
        [TestMethod]
        public void IsWhitelisted_EmptyUrlAndWhitelist_ReturnsFalse() 
            => Assert.IsFalse(RedirectHelper.IsWhitelisted("", ""));

        [TestMethod]
        public void IsWhitelisted_NullUrlAndWhitelist_ReturnsFalse()
            => Assert.IsFalse(RedirectHelper.IsWhitelisted(null, null));

        [TestMethod]
        public void IsWhitelisted_SpaceUrlAndWhitelistStartsWithSpace_ReturnsFalse()
            => Assert.IsFalse(RedirectHelper.IsWhitelisted(" ", " https://localtest.me"));

        [TestMethod]
        public void IsWhitelisted_Matches1Of1WhitelistedURL_ReturnsTrue()
            => Assert.IsTrue(RedirectHelper.IsWhitelisted("https://localtest.me/blob/1234", "https://localtest.me"));

        [TestMethod]
        public void IsWhitelisted_Matches1Of2WhitelistedURL_ReturnsTrue()
            => Assert.IsTrue(RedirectHelper.IsWhitelisted("https://localtest.me/blob/1234", "https://localtest.me;http://localtest.me"));

        [TestMethod]
        public void IsWhitelisted_Matches0Of1WhitelistedURL_ReturnsTrue()
            => Assert.IsFalse(RedirectHelper.IsWhitelisted("http://localtest.me/blob/1234", "https://localtest.me"));

        [TestMethod]
        public void IsWhitelisted_Matches0Of2WhitelistedURL_ReturnsTrue()
            => Assert.IsFalse(RedirectHelper.IsWhitelisted("https://foo.bar/blob/1234", "https://localtest.me;http://localtest.me"));

    }
}
