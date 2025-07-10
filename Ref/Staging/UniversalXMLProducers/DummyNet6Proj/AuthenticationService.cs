using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.DummyNet6Proj
{
	static class AuthenticationService
	{
		public static string GetServerTimeStamp(string token)
		{
			using (var client = new HttpClient())
			{
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
				var relativeUrl = "RefStlScript/GetServerTimestamp";
				var result = client.GetStringAsync(new Uri(new Uri(ApplicationConfig.DeliveryServiceBaseUrl), relativeUrl)).Result;
				return result;
			}
		}
	}
}
