using System;
using CargoWise.eHub.Core.Orchestrations.HttpRetry.Contract;
using CargoWise.eHub.Products.GBCustoms.Core.BT.Helpers;
using NUnit.Framework;

namespace CargoWise.eHub.Products.GBCustoms.Core.BT.Tests.Helpers
{
	[TestFixture]
	public class HttpHelpersTests
	{
		[TestFixture]
		public class ExtractEORIMethod
		{
			[Test]
			public void ValidInput_ShouldExtractEORI()
			{
				// Arrange.
				var credentialKey = "HYECMT.GB123456789000.CAW";

				// Act.
				var eori = HttpHelpers.ExtractEORI(credentialKey);

				// Assert.
				Assert.AreEqual("GB123456789000", eori);
			}

			[Test]
			public void InvalidInput_ShouldReturnNull()
			{
				// Arrange.
				var credentialKey = "HYECMTGB123456789000.CAW";

				// Act.
				var eori = HttpHelpers.ExtractEORI(credentialKey);

				// Assert.
				Assert.IsNull(eori, "EORI should be null");
			}
        }

        [TestFixture]
        public class ExtractClientSystemMethod
        {
            [Test]
            public void ValidInput_ShouldExtractClientSystem()
            {
                // Arrange.
                var credentialKey = "HYECMT.GB123456789000.CAW";

                // Act.
                var clientSystem = HttpHelpers.ExtractClientSystem(credentialKey);

                // Assert.
                Assert.AreEqual("HYECMT", clientSystem);
            }

            [Test]
            public void InvalidInput_ExtractClientSystemShouldReturnNull()
            {
                // Arrange.
                var credentialKey = "HYECMTGB123456789000CAW";

                // Act.
                var clientSystem = HttpHelpers.ExtractClientSystem(credentialKey);

                // Assert.
                Assert.IsNull(clientSystem, "Client System should be null");
            }
        }

        [TestFixture]
        public class CreateHttpHeadersMethod
        {
            [Test]
            public void CreateHttpHeader_ShouldConstructAllHttpHeaders()
            {
                var httpHeaders = HttpHelpers.CreateHttpHeaders("Authorization", "Bearer 32dabf34789g893h2a9ab2389c", "Accept", "application/json");

                Assert.That(httpHeaders, Is.Not.Null.And.Length.EqualTo(2));
                Assert.That(httpHeaders, TestHelpers.Has.HttpHeader("Authorization", "Bearer 32dabf34789g893h2a9ab2389c"));
                Assert.That(httpHeaders, TestHelpers.Has.HttpHeader("Accept", "application/json"));
            }
        }

        [TestFixture]
		public class CreateRegistrationHeadersMethod
		{
			[Test]
			public void WhenGettingValidInput_ShouldConstructAllHttpHeaders()
			{
				// Arrange.

				var userSystemCode = "[_MOCK_USER_SYSTEM_CODE_]";
				var username = "[_MOCK_USERNAME_]";
				var password = "[_MOCK_PASSWORD_]";
				var badge = "[_MOCK_BADGE_]";
				var eori = "[_MOCK_EORI_]";

				// Act.

				var httpHeaders = HttpHelpers.CreateRegistrationHeaders(userSystemCode, username, password, badge, eori);

				// Assert.

				Assert.That(
					httpHeaders,
					Is.Not.Null.And.Length.EqualTo(5));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("Authorization", "Basic W19NT0NLX1VTRVJOQU1FX106W19NT0NLX1BBU1NXT1JEX10="));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("Accept", "application/vnd.csp.1.0+xml"));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("X-Badge-ID", "[_MOCK_BADGE_]"));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("User-Agent", "Vendor=WiseTech Global, Application=eHub, Version=3.0.0.0, Badge=[_MOCK_BADGE_], ClientID=[_MOCK_USER_SYSTEM_CODE_]"));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("X-Submitter-Identifier", "[_MOCK_EORI_]"));
			}

            [Test]
            public void WhenGettingInvalidInput_ShouldTrowException()
            {
                // Arrange.

                var userSystemCode = "[_MOCK_USER_SYSTEM_CODE_]";
                string username = null;
                var password = "[_MOCK_PASSWORD_]";
                var badge = "[_MOCK_BADGE_]";
                var eori = "[_MOCK_EORI_]";

                // Act & Assert

                var ex = Assert.Throws<ArgumentNullException>(delegate { HttpHelpers.CreateRegistrationHeaders(userSystemCode, username, password, badge, eori); });
                Assert.That(ex.Message, Does.Contain("Username was not supplied"));
            }
        }

        [TestFixture]
		public class CreateSubmissionHeadersMethod
		{
			[Test]
			public void WhenGettingValidInput_ShouldConstructAllHttpHeaders()
			{
				// Arrange.

				var userSystemCode = "[_MOCK_USER_SYSTEM_CODE_]";
				var username = "[_MOCK_USERNAME_]";
				var password = "[_MOCK_PASSWORD_]";
				var badge = "[_MOCK_BADGE_]";
				var eori = "[_MOCK_EORI_]";

				// Act.

				var httpHeaders = HttpHelpers.CreateSubmissionHeaders(userSystemCode, username, password, badge, eori);

				// Assert.

				Assert.That(
					httpHeaders,
					Is.Not.Null.And.Length.EqualTo(5));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("Authorization", "Basic W19NT0NLX1VTRVJOQU1FX106W19NT0NLX1BBU1NXT1JEX10="));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("Accept", "application/vnd.hmrc.2.0+xml"));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("X-Badge-ID", "[_MOCK_BADGE_]"));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("User-Agent", "Vendor=WiseTech Global, Application=eHub, Version=3.0.0.0, Badge=[_MOCK_BADGE_], ClientID=[_MOCK_USER_SYSTEM_CODE_]"));

				Assert.That(
					httpHeaders,
					TestHelpers.Has.HttpHeader("X-Submitter-Identifier", "[_MOCK_EORI_]"));
			}

            [Test]
            public void WhenGettingInvalidInput_ShouldTrowException()
            {
                // Arrange.

                var userSystemCode = "[_MOCK_USER_SYSTEM_CODE_]";
                var username = "[_MOCK_USERNAME_]";
                string password = null;
                var badge = "[_MOCK_BADGE_]";
                var eori = "[_MOCK_EORI_]";

                // Act & Assert

                var ex = Assert.Throws<ArgumentNullException>(delegate { HttpHelpers.CreateRegistrationHeaders(userSystemCode, username, password, badge, eori); });
                Assert.That(ex.Message, Does.Contain("Password was not supplied"));
            }
        }

        [TestFixture]
        public class CreateSubmissionHeadersMethodWithVersion
        {
            [Test]
            public void WhenGettingValidInput_ShouldConstructAllHttpHeadersWithVersionParameter()
            {
                // Arrange.

                var userSystemCode = "[_MOCK_USER_SYSTEM_CODE_]";
                var username = "[_MOCK_USERNAME_]";
                var password = "[_MOCK_PASSWORD_]";
                var badge = "[_MOCK_BADGE_]";
                var eori = "[_MOCK_EORI_]";
                var version = "[_MOCK_VERSION_]";

                // Act.

                var httpHeaders = HttpHelpers.CreateSubmissionHeaders(userSystemCode, username, password, badge, eori, version);

                // Assert.

                Assert.That(
                    httpHeaders,
                    Is.Not.Null.And.Length.EqualTo(5));

                Assert.That(
                    httpHeaders,
                    TestHelpers.Has.HttpHeader("Authorization", "Basic W19NT0NLX1VTRVJOQU1FX106W19NT0NLX1BBU1NXT1JEX10="));

                Assert.That(
                    httpHeaders,
                    TestHelpers.Has.HttpHeader("Accept", "application/vnd.hmrc.[_MOCK_VERSION_]+xml"));

                Assert.That(
                    httpHeaders,
                    TestHelpers.Has.HttpHeader("X-Badge-ID", "[_MOCK_BADGE_]"));

                Assert.That(
                    httpHeaders,
                    TestHelpers.Has.HttpHeader("User-Agent", "Vendor=WiseTech Global, Application=eHub, Version=3.0.0.0, Badge=[_MOCK_BADGE_], ClientID=[_MOCK_USER_SYSTEM_CODE_]"));

                Assert.That(
                    httpHeaders,
                    TestHelpers.Has.HttpHeader("X-Submitter-Identifier", "[_MOCK_EORI_]"));
            }

            [TestFixture]
            public class CreateSubmissionHeadersMethodWithAccountAndUri
            {
                [Test]
                public void WhenGettingValidInput_ShouldConstructAllHttpHeadersWithAccountAndUri()
                {
                    // Arrange.

                    var userSystemCode = "[_MOCK_USER_SYSTEM_CODE_]";
                    var username = "[_MOCK_USERNAME_]";
                    var password = "[_MOCK_PASSWORD_]";
                    var badge = "[_MOCK_BADGE_]";
                    var eori = "[_MOCK_EORI_]";
                    var version = "[_MOCK_VERSION_]";
                    var account = "[_MOCK_ACCOUNT_]";
					var uri = "[_MOCK_URI_]";

					// Act.

					var httpHeaders = HttpHelpers.CreateSubmissionHeaders(userSystemCode, username, password, badge, eori, version, account, uri);

                    // Assert.

                    Assert.That(
                        httpHeaders,
                        Is.Not.Null.And.Length.EqualTo(8));

                    Assert.That(
                        httpHeaders,
                        TestHelpers.Has.HttpHeader("Authorization", "Basic W19NT0NLX1VTRVJOQU1FX106W19NT0NLX1BBU1NXT1JEX10="));

                    Assert.That(
                        httpHeaders,
                        TestHelpers.Has.HttpHeader("Accept", "application/vnd.hmrc.[_MOCK_VERSION_]+xml"));

                    Assert.That(
                        httpHeaders,
                        TestHelpers.Has.HttpHeader("X-Badge-ID", "[_MOCK_BADGE_]"));

                    Assert.That(
                        httpHeaders,
                        TestHelpers.Has.HttpHeader("User-Agent", "Vendor=WiseTech Global, Application=eHub, Version=3.0.0.0, Badge=[_MOCK_BADGE_], ClientID=[_MOCK_USER_SYSTEM_CODE_]"));

                    Assert.That(
                        httpHeaders,
                        TestHelpers.Has.HttpHeader("X-Submitter-Identifier", "[_MOCK_EORI_]"));

                    Assert.That(
                        httpHeaders,
                        TestHelpers.Has.HttpHeader("AccountName", "[_MOCK_ACCOUNT_]"));

                    Assert.That(
                        httpHeaders,
                        TestHelpers.Has.HttpHeader("SendToURI", "[_MOCK_URI_]"));

					Assert.That(
						httpHeaders,
						TestHelpers.Has.HttpHeader("OutboundAuthorization", "CspOutboundValidation"));
				}
			}
        }

        [Test]
        public void WhenGettingInvalidInput_ShouldTrowException()
        {
            // Arrange.

            var userSystemCode = "[_MOCK_USER_SYSTEM_CODE_]";
            var username = "[_MOCK_USERNAME_]";
            var password = "[_MOCK_PASSWORD_]";
            string badge = null;
            var eori = "[_MOCK_EORI_]";

            // Act & Assert

            var ex = Assert.Throws<ArgumentNullException>(delegate { HttpHelpers.CreateRegistrationHeaders(userSystemCode, username, password, badge, eori); });
            Assert.That(ex.Message, Does.Contain("Badge was not supplied"));
        }

        [Test]
        public void TestCreateOverridingConfig()
        {
            var request = new HttpRequest();
            HttpHelpers.CreateOverridingConfig(request);
            Assert.AreEqual(3, request.OverridingExceptionConfigs.Length);
            Assert.AreEqual("Microsoft.XLANGs.Core.XlangSoapException", request.OverridingExceptionConfigs[0].ExceptionType);
            Assert.AreEqual("(System.Net.Http.HttpRequestException) The underlying connection was closed: An unexpected error occurred on a receive", request.OverridingExceptionConfigs[0].ExceptionMessage);
            Assert.IsTrue(request.OverridingExceptionConfigs[0].ShouldRetry);
            Assert.AreEqual("Microsoft.XLANGs.Core.XlangSoapException", request.OverridingExceptionConfigs[1].ExceptionType);
            Assert.AreEqual("(System.Net.Http.HttpRequestException) Unable to connect to the remote server", request.OverridingExceptionConfigs[1].ExceptionMessage);
            Assert.IsTrue(request.OverridingExceptionConfigs[1].ShouldRetry);
            Assert.AreEqual("Microsoft.XLANGs.Core.XlangSoapException", request.OverridingExceptionConfigs[2].ExceptionType);
            Assert.AreEqual("(System.Threading.Tasks.TaskCanceledException) A task was canceled", request.OverridingExceptionConfigs[2].ExceptionMessage);
            Assert.IsTrue(request.OverridingExceptionConfigs[2].ShouldRetry);
        }
    }

    [TestFixture]
	public class CreateRegistrationPayloadMethod
	{
		[Test]
		public void WhenGettingValidCallbackRegistration_ShouldReturnConsumerXml()
		{
			// Arrange.

			var callbackRegistration = new CallbackRegistration
			{
				Uri = new Uri("http://www.mock-callback.com/"),
				Authorization = "[_MOCK_AUTH_]"
			};

			// Act.

			var xml = HttpHelpers.CreateRegistrationPayload(callbackRegistration);

			// Assert.

			Assert.That(
				xml,
				Is.EqualTo("<consumer endpointUrl=\"http://www.mock-callback.com/\" authorization=\"[_MOCK_AUTH_]\"></consumer>"));
		}
	}
}
