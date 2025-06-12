using CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService.Controllers;
using Common.Logging;
using NUnit.Framework;
using Rhino.Mocks;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace CargoWise.eHub.Products.GBCustoms.CSP.OutboundWebService.Tests
{
	public class CSPOutboundControllerTests
	{
		public HttpRequestMessage testRequest;
		public HttpResponseMessage testResponse;
		public string account;

		[SetUp]
		public void Setup()
		{
			testRequest = new HttpRequestMessage();
			account = null;
		}

		[Test]
		public void ValidateAccount_NoHeader()
		{
			var cSPOutboundControllerForTest = new CSPOutboundControllerForTest(testRequest);
			var isValid = cSPOutboundControllerForTest.GetAndRemoveHeader("AccountName", out account, out testResponse);

			Assert.IsTrue(!isValid);
			Assert.AreEqual(HttpStatusCode.InternalServerError, testResponse.StatusCode);
			Assert.AreEqual("No AccountName header found", testResponse.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void ValidateAccount_EmptyHeader()
		{
			testRequest.Headers.Add("AccountName", "");
			var cSPOutboundControllerForTest = new CSPOutboundControllerForTest(testRequest);
			var isValid = cSPOutboundControllerForTest.GetAndRemoveHeader("AccountName", out account, out testResponse);

			Assert.IsTrue(!isValid);
			Assert.AreEqual(HttpStatusCode.InternalServerError, testResponse.StatusCode);
			Assert.AreEqual("Invalid AccountName header", testResponse.Content.ReadAsStringAsync().Result);
		}

		[Test]
		public void ValidateAccount_ValidHeader()
		{
			testRequest.Headers.Add("AccountName", "TestAccountName");
			var cSPOutboundControllerForTest = new CSPOutboundControllerForTest(testRequest);
			var isValid = cSPOutboundControllerForTest.GetAndRemoveHeader("AccountName", out account, out testResponse);

			Assert.IsTrue(isValid);
			Assert.IsNull(testResponse);

			IEnumerable<string> accounts;
			var headerExist = cSPOutboundControllerForTest.Request.Headers.TryGetValues("AccountName", out accounts);
			Assert.IsTrue(!headerExist);
		}

		[Test]
		public void ValidateAuth_ValidHeader()
		{
			testRequest.Headers.Add("OutboundAuthorization", "CspOutboundValidation");
			var cSPOutboundControllerForTest = new CSPOutboundControllerForTest(testRequest);
			Assert.IsTrue(cSPOutboundControllerForTest.ValidateAuthHeader());

			IEnumerable<string> accounts;
			Assert.IsTrue(!cSPOutboundControllerForTest.Request.Headers.TryGetValues("OutboundAuthorization", out accounts));
		}

		[Test]
		public void ValidateAuth_InvalidHeader()
		{
			testRequest.Headers.Add("OutboundAuthorization", "InvalidValue");
			var cSPOutboundControllerForTest = new CSPOutboundControllerForTest(testRequest);
			Assert.IsFalse(cSPOutboundControllerForTest.ValidateAuthHeader());
		}

		[Test]
		public void ValidateAuth_NoHeader()
		{
			testRequest.Headers.Add("SomeOtherHeader", "SomeValue");
			var cSPOutboundControllerForTest = new CSPOutboundControllerForTest(testRequest);
			Assert.IsFalse(cSPOutboundControllerForTest.ValidateAuthHeader());
		}
	}

	#region Implementation
	public class CSPOutboundControllerForTest : CSPOutboundController
	{
		public CSPOutboundControllerForTest(HttpRequestMessage httpRequestMessage)
		{
			InternalLog = () => MockRepository.GenerateMock<ILog>();
			Request = httpRequestMessage;
		}
	}
	#endregion

}
