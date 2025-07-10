using System;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public static class ExchangeRateDownloader
	{
		public static byte[] Download(HttpClient client, DateTime date)
		{
			var response = client.GetAsyncEx($"{URL_EXCHANGE_RATE}&RadOpcao=2&ChkMoeda=61&DATAINI={date:dd}%2F{date:MM}%2F{date:yyyy}&DATAFIM=")?.Result;
			byte[] returnStreamValue = null;
			var content = response.Content.ReadAsStringAsync()?.Result;
			var result = content?.IndexOf("gerarCSVTodasAsMoedas&amp;id=", StringComparison.Ordinal) ?? 0;
			if (result > 0)
			{
				var subStringValue = content.Substring(result);
				if (!string.IsNullOrEmpty(subStringValue))
				{
					var finalPosition = subStringValue.IndexOf('"');
					if (finalPosition > 0)
					{
						var finalURL = subStringValue.Substring(0, finalPosition);

						var downloadId = Regex.Replace(finalURL, "[^0-9]", "");
						if (!string.IsNullOrEmpty(downloadId))
						{
							var downloadURL = URL_CSV_EXCHANGE_RATE + "&id=" + downloadId;
							using (var resultStream = client.GetByteArrayAsyncEx(downloadURL))
							{
								returnStreamValue = resultStream?.Result;
							}
						}
					}
				}
			}

			return returnStreamValue;

		}

		static readonly string URL_EXCHANGE_RATE = ConfigurationProvider.Configuration.GetSection("BRExchangeRateUrl").Value;
		static readonly string URL_CSV_EXCHANGE_RATE = ConfigurationProvider.Configuration.GetSection("BRExchangeRateUrlCsv").Value;
	}
}
