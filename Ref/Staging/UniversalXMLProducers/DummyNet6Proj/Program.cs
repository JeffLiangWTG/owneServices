using System;
using System.IO;
using CargoWise.RefDbRepo.Common.Utils;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.DummyNet6Proj
{
	public static class Program
	{
		static void Main(string[] args)
		{
			//TODO : When we finally remove this DUMMY Project, we should also remove the code which copy CargoWise.RefDbRepo.Service.Schema_0_9.dll to UniversalXML folder
			CheckBuildConfiguration();
			GetApplicationConfig();

			GetDecryptedDummyPassword();

			if (!string.IsNullOrEmpty(ApplicationConfig.DummyKeyFile))
			{
				GetDecryptedDummyKeyFile();
			}

			TryGetAccessToken();
		}

		static void CheckBuildConfiguration()
		{
			Console.WriteLine("Build Configuration is EF7");
		}

		static void GetDecryptedDummyPassword()
		{
			Console.WriteLine($"dummy password is {ApplicationConfig.DummyPassword}");
		}

		static void GetDecryptedDummyKeyFile()
		{
			var fileContent = File.ReadAllText(ApplicationConfig.DummyKeyFile);
			Console.WriteLine($"dummy key file content is: {fileContent}");
		}

		static void GetApplicationConfig()
		{
			Console.WriteLine("Get config by IConfiguration: ");
			Console.WriteLine($"DeliveryServiceBaseUrl = {ApplicationConfig.DeliveryServiceBaseUrl}");
			Console.WriteLine($"ConfigFromRepoForTest = {ApplicationConfig.ConfigFromRepoForTest}");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types")]
		static void TryGetAccessToken()
		{
			using (var accessTokenProvider = new AccessTokenProvider(ApplicationConfig.TenantId, ApplicationConfig.ClientId, ApplicationConfig.DeliveryServiceClientId, ApplicationConfig.PrivateKeyFileName, ApplicationConfig.CertificateFileName, ApplicationConfig.RefreshAdvanceInMinutes))
			{
				try
				{
					var accessToken = accessTokenProvider.GetAccessToken();
					if (string.IsNullOrEmpty(accessToken))
					{
						Console.WriteLine("Cannot get access token, please check the authentication information.");
						return;
					}
					Console.WriteLine($"Get access token successfully. it's {accessToken}");
					accessToken = accessTokenProvider.GetAccessToken();
					Console.WriteLine($"Get access token again successfully. it's {accessToken}");

					Console.WriteLine($"Try get timeStamp from the server...");
					var timeStamp = AuthenticationService.GetServerTimeStamp(accessToken);
					Console.WriteLine($"Get timeStamp from the server successfully. It's {timeStamp}");
				}
				catch (Exception ex)
				{
					Console.Error.WriteLine(ex);
				}
			}
		}
	}
}
