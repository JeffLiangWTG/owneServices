using System.Data.SqlClient;
using System.Net;
using CargoWise.eHub.Portal.IntegrationTests.Attributes;
using CargoWise.eHub.Selenium.IntegrationTests.Core;
using CargoWise.eServices.TestHelpers.Database.Common;
using NUnit.Framework;
using OpenQA.Selenium;

namespace CargoWise.eHub.Portal.IntegrationTests.Views
{
    [TestFixture]
	[WithPortalService]
	public class eHubPortalHttpHeadersTest
	{
		[TestCase("X-AspNet-Version")]
		[TestCase("X-Powered-By")]
		[TestCase("X-AspNetMvc-Version")]
		public void TestHttpHeadersNoVersionInfo(string versionInfo)
		{
			var endPoint = WithPortalServiceAttribute.Current.GetHttpEndPoint("");
			var request = WebRequest.Create(endPoint);

			request.Credentials = CredentialCache.DefaultCredentials;

			using (var response = (HttpWebResponse)request.GetResponse())
			{
				Assert.IsNull(response.Headers[versionInfo]);
			}
		}

		[TestCase("GET")]
		[TestCase("HEAD")]
		public void TestWtgStatusHealthCheckIsSuccessful(string httpRequestType)
		{
			var endPoint = WithPortalServiceAttribute.Current.GetHttpEndPoint("wtg/status");
			var request = WebRequest.Create(endPoint);

			request.Method = httpRequestType;
			request.Credentials = CredentialCache.DefaultCredentials;

			using (var response = (HttpWebResponse)request.GetResponse())
			{
				Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
			}
		}
	}
}
