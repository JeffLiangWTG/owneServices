
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Security.Authentication;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public abstract class BaseJsonDownloader<T>
	{
		static T JsonToObject(string json) => JsonConvert.DeserializeObject<T>(json);

		protected T DownloadJson(HttpClient client, string url)
		{
			using (var clientResponse = client.GetAsyncEx(url))
			{
				var response = clientResponse?.Result;

				Contract.Assume(response != null);
				if (response.IsSuccessStatusCode)
				{
					var dto = JsonToObject(response.Content.ReadAsStringAsync().Result);

					Contract.Assume(dto != null);
					return dto;
				}
				else
				{
					throw new AuthenticationException(response.ReasonPhrase);
				}
			}
		}

		protected string GetParameter(string key) => ConfigurationProvider.Configuration.GetSection(key).Value;
	}
}
