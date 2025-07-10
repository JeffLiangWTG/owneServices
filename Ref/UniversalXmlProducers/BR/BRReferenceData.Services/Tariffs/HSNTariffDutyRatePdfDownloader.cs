using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net.Http;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class HSNTariffDutyRatePdfDownloader : BaseDownloaderWithHrefSearch
	{
		public new List<byte[]> Download(HttpClient client)
		{
			using (var tariffRate = client.GetAsync(new Uri(ConfigurationProvider.Configuration.GetSection("URL_CURRENT_LISTS_TARIFF_RATES").Value)))
			{
				var tariffRateResponse = tariffRate?.Result;
				Contract.Assume(tariffRateResponse != null);

				var splitInnerHtml = InnerHtml.Split(';');
				var bXlsxList = new List<byte[]>();

				foreach (var innerHtml in splitInnerHtml)
				{
					var href = GetHrefValue(tariffRateResponse.Content.ReadAsStringAsync()?.Result, innerHtml);
					var bXml = DownloadFile(href, client);
					Contract.Assume(bXml != null);
					bXlsxList.Add(bXml);
				}

				return bXlsxList;
			}
		}

		protected override string Url => ConfigurationProvider.Configuration.GetSection("URL_CURRENT_LISTS_TARIFF_RATES").Value;

		protected override string InnerHtml => "Anexo VII;Anexo VI;Anexo V;Anexo IV;Anexo II";
	}
}
