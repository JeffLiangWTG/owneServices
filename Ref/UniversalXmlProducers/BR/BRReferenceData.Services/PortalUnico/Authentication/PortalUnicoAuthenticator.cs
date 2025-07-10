using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.Common.Argument;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class PortalUnicoAuthenticator
	{
		public static TokenDTO DoAuthentication(HttpClient client, bool isProduction)
		{
			client.DefaultRequestHeaders.Add("Role-Type", "IMPEXP");
			using (var request = new HttpRequestMessage(HttpMethod.Post, ConfigurationProvider.Configuration.GetSection(isProduction ? "URL_PORTAL_UNICO_AUTHENTICATION" : "URL_PORTAL_UNICO_AUTHENTICATION_TEST").Value))
			{
				var response = client.SendAsync(request).Result;

				Argument.IsTrue(response.IsSuccessStatusCode, "response.IsSuccessStatusCode");

				var headersToken = response.Headers.GetValues("Set-Token")?.ToArray();
				var token = headersToken?.Length > 0 ? headersToken[0] : "";

				var headersXToken = response.Headers.GetValues("X-CSRF-Token")?.ToArray();
				var xToken = headersXToken?.Length > 0 ? headersXToken[0] : "";

				TokenDTO dto = new TokenDTO() { Authorization = token, XToken = xToken };

				return dto;
			}
		}
	}
}
