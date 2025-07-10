using System;
using System.Net.Http;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class TariffTecDownloader
	{
		public static byte[] Download(HttpClient client)
		{
			var response = (client.GetAsync(new Uri(ConfigurationProvider.Configuration.GetSection("URL_TEC").Value))?.Result);

			var responseBody = response.Content.ReadAsStreamAsync()?.Result;

			var html = new HtmlDocument();
			html.Load(responseBody);

			var anchors = html.DocumentNode.SelectNodes("//a");
			string xlsLink = null;
			foreach (var anchor in anchors)
			{
				if (anchor.HasAttributes && anchor.GetAttributeValue("href", "").StartsWith("/images/Excel/Listas/TEC", StringComparison.Ordinal))
				{
					xlsLink = anchor.GetAttributeValue("href", "");
					break;
				}
			}

			if (xlsLink == null)
			{
				return null;
			}

			response = client.GetAsync(new Uri(ConfigurationProvider.Configuration.GetSection("BASE_URL_TEC").Value + xlsLink))?.Result;
			return response != null && response.IsSuccessStatusCode ? response.Content.ReadAsByteArrayAsync()?.Result : null;
		}
	}
}
