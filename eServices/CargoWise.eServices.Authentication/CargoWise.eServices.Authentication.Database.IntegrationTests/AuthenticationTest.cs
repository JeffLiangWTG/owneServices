using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using CargoWise.eServices.TestHelpers.Database.Common;
using CargoWise.eServices.TestHelpers.Database.Deployment;
using NUnit.Framework;

namespace CargoWise.eServices.Authentication.IntegrationTests
{
	[TestFixture]
	[WithAuthenticationService]
	public abstract class AuthenticationTest
	{
		[SetUp]
		public void Init()
		{
			TestContext.WriteLine(Deployment.GetLog());

			RunSqlCommand(@"
				IF (NOT EXISTS(SELECT 1 FROM Authentication WHERE AT_SystemID = 'C')) 
				BEGIN 
					INSERT INTO Authentication
					VALUES(
						'C', 
						'24-6F-63-9D-0C-60-85-F5-AB-79-E6-49-CF-C8-C7-FA-9A-2F-2A-B1-BD-CA-07-3C-AF-96-4C-ED-D1-40-97-98-C8-74-F1-30-DF-66-D5-F4-1B-E8-A0-75-7E-31-49-F5-84-32-F8-0E-33-9E-39-9B-5E-CA-91-DA-E8-E4-32-BC', 
						'DAU', 
						'TST', 
						GETUTCDATE(), 
						GETUTCDATE()) 
				END");

			WaitForSiteToBeReady();
		}

		[TearDown]
		public void Dispose()
		{
			RunSqlCommand("DELETE FROM Authentication WHERE AT_SystemID = 'C'");
		}

		static void WaitForSiteToBeReady()
		{
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
			var endPoint = WithAuthenticationServiceAttribute.Current.HealthCheckUrl;

			for (var count = 0; count < 3;)
			{
				try
				{
					var request = WebRequest.Create(endPoint);
					request.Credentials = CredentialCache.DefaultCredentials;

					using (var response = (HttpWebResponse)request.GetResponse())
					{
						Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

						using (var responseStream = response.GetResponseStream())
						{
							using (var reader = new StreamReader(responseStream))
							{
								var responseText = reader.ReadToEnd();
								Assert.That(responseText, Is.EqualTo("INFO(AuthenticationWebService): Service is alive."), responseText);
							}
						}
					}

					break;
				}
				catch (Exception) when (count < 2)
				{
					Task.Delay(10000).Wait();
					count++;
				}
			}
		}

		static void RunSqlCommand(string sql)
		{
			SqlServerHelper.RunCommand(sql, "AuthenticationWebService");
		}
	}
}
