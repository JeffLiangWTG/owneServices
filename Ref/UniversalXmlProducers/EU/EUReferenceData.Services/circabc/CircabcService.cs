using System.Globalization;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.UniversalXMLProducers.Common;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.EUReferenceData.Services.Circabc
{
	public class CircabcService
	{
		public CircabcService(IHttpClientHelper httpClient, string baseUrl)
		{
			this.baseUrl = baseUrl;
			this.httpClient = httpClient;
		}

		public async Task<CircabcResponse> GetFolderAsync(string id)
		{
			var url = string.Format(CultureInfo.InvariantCulture, baseUrl, id);
			var data = await httpClient.GetWebPageAsync(url);
			return JsonConvert.DeserializeObject<CircabcResponse>(data);
		}

		readonly IHttpClientHelper httpClient;
		readonly string baseUrl;
	}
}
