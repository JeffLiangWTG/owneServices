using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class TariffRateIPIDownloader
	{
		public static byte[] Download(HttpClient client)
		{
			var response = client.GetAsyncEx(ConfigurationProvider.Configuration.GetSection("URL_TIPI_HOME").Value)?.Result;

			if (response != null && response.IsSuccessStatusCode)
			{
				var html = new HtmlDocument();
				html.Load(response.Content.ReadAsStreamAsync()?.Result, Encoding.UTF8);

				var anchors = html.DocumentNode.SelectNodes("//a[@class='external-link']");

				var decreesAndActs = new Dictionary<string, string>();
				foreach (var anchor in anchors)
				{
					var href = anchor.GetAttributeValue("href", "");
					var text = anchor.GetDirectInnerText();
					if (string.IsNullOrEmpty(text?.Trim()))
					{
						continue;
					}
					decreesAndActs.Add(text, href);
				}

				var json = JsonConvert.SerializeObject(decreesAndActs);
				byte[] bytes = Encoding.UTF8.GetBytes(json);

				return bytes;
			}

			return null;
		}
	}
}
