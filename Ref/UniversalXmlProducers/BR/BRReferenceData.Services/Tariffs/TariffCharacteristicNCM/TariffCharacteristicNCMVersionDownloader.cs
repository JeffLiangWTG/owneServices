using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffCharacteristicNCMVersionDownloader
	{
		public TariffCharacteristicNCMVersionDownloader(bool isProduction)
		{
			this.isProduction = isProduction;
		}
		readonly bool isProduction;

		string NcmAttributesUri => ConfigurationProvider.Configuration.GetSection(isProduction ? "URL_TARIFF_NCM_ATTRIBUTE_VERSION" : "URL_TARIFF_NCM_ATTRIBUTE_VERSION_TEST").Value;

		public string GetLastVersion(HttpClient client)
		{
			using (var response = client.GetAsyncEx(NcmAttributesUri)?.Result)
			{
				Contract.Assume(response != null);
				Contract.Assume(response.IsSuccessStatusCode);

				var htmlContent = response.Content?.ReadAsStringAsync()?.Result;

				var versions = Regex.Match(htmlContent, "id=\"versao[0-9]+\"");
				return Regex.Replace(versions?.Value, "[id=\"]", "");
			}
		}
	}
}
