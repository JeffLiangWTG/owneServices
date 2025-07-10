using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public class ClientLicenseInformationProvider : IClientLicenseInformationProvider
	{
		public ClientLicenseInformationProvider()
		{
		}

		static string LicenceInformationUri => ConfigurationProvider.Configuration["LicenceInformationUri"];

		public async Task<string[]> GetLicenceInformationInactive()
		{
			using (var httpClient = new HttpClient())
			{
				var response = await httpClient.GetAsync(new Uri(LicenceInformationUri + "GetLicenceInformationInactive/"));
				var con = response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().Result;
				var jObject = JsonConvert.DeserializeObject<JObject>(con);
				return jObject["LicenceInformationCollection"].Select(t => (string)t["DatabaseId"]).ToArray();
			}
		}
	}
}
