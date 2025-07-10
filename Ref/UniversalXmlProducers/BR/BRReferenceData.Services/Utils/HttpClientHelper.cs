using System.Net.Http;
using System.Security.Cryptography.X509Certificates;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class HttpClientHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "<Pending>")]
		public static HttpClient GetHttpClientWithCertificate(HttpClientHandler handler, bool isProduction)
		{
			if (handler.ClientCertificates.Count == 0)
			{
				handler.ClientCertificates.Add(new X509Certificate2(CertificateLoader.LoadCertificate(), ConfigurationProvider.Configuration["PORTAL_UNICO_CERTIFICATE_PASSWORD"], X509KeyStorageFlags.MachineKeySet));
			}
			var client = new HttpClient(handler);
			var tokenDto = PortalUnicoAuthenticator.DoAuthentication(client, isProduction);

			SetupHeaderRequest(tokenDto, client);

			return client;
		}

		static void SetupHeaderRequest(TokenDTO tokenDto, HttpClient client)
		{
			client.DefaultRequestHeaders.Add("Authorization", tokenDto.Authorization);
			client.DefaultRequestHeaders.Add("X-CSRF-Token", tokenDto.XToken);
		}
	}
}
