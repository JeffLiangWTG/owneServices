namespace CargoWise.eServices.Authentication.IntegrationTests
{
	using System.Configuration;
	using System.IO;
	using System.Net;
	using NUnit.Framework;

	[TestFixture]
	public class AuthenticationServiceHealthCheckTest : AuthenticationTest
	{
		[TestCase("GET", "INFO(AuthenticationWebService): Service is alive.")]
		[TestCase("HEAD", "")]
		public void TestAuthenticationServiceHealthCheck(string method, string bodyString)
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

			var endPoint = ConfigurationManager.AppSettings["CargoWise.eServices.Authentication.HealthCheck.Http.Endpoint"];
			var request = HttpWebRequest.Create(endPoint);
			request.Method = method;
			request.Credentials = CredentialCache.DefaultCredentials;

			try
			{
				using (var response = (HttpWebResponse)request.GetResponse())
				{
					Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
					Assert.IsFalse(response.IsFromCache);
					Assert.That(response.ContentType, Is.EqualTo("text/plain; charset=utf-8"));

					using (var responseStream = response.GetResponseStream())
					{
						using (var reader = new StreamReader(responseStream))
						{
							var result = reader.ReadToEnd();
							Assert.That(result, Is.EqualTo(bodyString));
						}
					}
				}
			}
			catch (WebException ex)
			{
				using (var responseStream = ex.Response.GetResponseStream())
				{
					using (var reader = new StreamReader(responseStream))
					{
						var result = reader.ReadToEnd();
						Assert.Fail("WebException occured with response: " + result);		 
					}
				}
			}
		}
	}
}