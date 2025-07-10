using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.DeTariffs
{
	public static class AuthorizationHelper
	{
		public static void SetDefaultBasicAuthorisationHeader(HttpClient client, string clientId, string clientSecret)
		{
			ArgumentNullException.ThrowIfNull(client);
			ArgumentNullException.ThrowIfNull(clientId);
			ArgumentNullException.ThrowIfNull(clientSecret);


			var byteArray = Encoding.ASCII.GetBytes($"{clientId}:{clientSecret}");
			client.DefaultRequestHeaders.Authorization =
				new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
		}
	}
}
