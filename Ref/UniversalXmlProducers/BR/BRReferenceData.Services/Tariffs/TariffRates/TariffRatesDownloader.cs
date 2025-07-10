using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Threading;
using HtmlAgilityPack;
using WTG.LS.CaptchaSolver.Exceptions;

namespace CargoWise.RefDbRepo.BRReferenceData.Services
{
	public class TariffRatesDownloader : BaseAduanaTableDownloader
	{
		public void DownloadRates(HttpClient client, IEnumerable<string> tariffCodes, Action<IEnumerable<TariffDTO>> saveTariffs)
		{
			var tariffs = new List<TariffDTO>();
			foreach (var tariffCode in tariffCodes)
			{
				TariffDTO tariffDTO = null;
				if (tariffCode.RemoveNonNumberValues().Length < 8)
				{
					continue;
				}

				var tries = 0;
				do
				{
					tries++;
					var tariffRatesHtml = GetTariffRatesPage(client, tariffCode);
					if (tariffRatesHtml.Contains("Dados Gerais"))
					{
						tariffDTO = ExtractDataFromHtml(tariffRatesHtml);

						if (tariffDTO?.TariffRates?.Any(r => string.IsNullOrEmpty(r.StartDate) && r.Code != RateCodes.COFINS && r.Code != RateCodes.PIS) ?? false)
						{
							tariffDTO = null;
						}
						else
						{
							break;
						}
					}
					Thread.Sleep(2000);
				} while (tries <= 5);

				if (tariffDTO != null)
				{
					tariffs.Add(tariffDTO);
				}
			}
			saveTariffs(tariffs);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Makes multi-thread downloader slower in 60%")]
		string GetTariffRatesPage(HttpClient client, string tariffCode)
		{
			using var responseLogin = DoLogin(client);
			if (!responseLogin.IsSuccessStatusCode)
			{
				throw new CaptchaSolverException("Unable to solve captcha on the (Tabelas Aduananeiras)");
			}

			return TariffSearch(client, ViewId(responseLogin), tariffCode);
		}

		static TariffDTO ExtractDataFromHtml(string html)
		{
			Contract.Assume(html?.Length > 0);

			var docHtml = new HtmlDocument();
			docHtml.LoadHtml(html);

			var dataArray = docHtml.DocumentNode?.SelectNodes($"//div[@class='unit unitData']");
			Contract.Assume(dataArray?.Count > 0);

			var tariffDTO = new TariffDTO
			{
				Code = GetDataBySequence(0),
				TariffRates =
				[
					GetRateData(RateCodes.PIS,12),
					GetRateData(RateCodes.COFINS,18),
					GetRateData(RateCodes.IPI,9),
					GetRateData(RateCodes.Duty,3)
				]
			};
			return tariffDTO;

			string GetDataBySequence(int sequence)
			{
				if (dataArray?.Count > sequence)
				{
					var data = dataArray[sequence]?.InnerText ?? string.Empty;
					return data.Contains("NT") ? string.Empty : data.RemoveNonDecimalValues();
				}

				return null;
			}

			TariffRateDTO GetRateData(string type, int percentualIndex)
			{
				return new TariffRateDTO
				{
					Code = type,
					Percentual = GetDataBySequence(percentualIndex).Replace(',', '.'),
					StartDate = GetDataBySequence(++percentualIndex),
					EndDate = GetDataBySequence(++percentualIndex),
				};
			}
		}

		static string TariffSearch(HttpClient client, string viewId, string tariff)
		{
			var prePost = new Dictionary<string, string>
			{
				{ "AJAXREQUEST", "_viewRoot" },
				{ "j_id111:subItemNcmMB_tipoConsulta", "0" },
				{ "j_id111:valorConsulta_codigo", tariff },
				{ "j_id111:panelDownloadArquivoOpenedState", "" },
				{ "j_id111", "j_id111" },
				{ "autoScroll", "" },
				{ "javax.faces.ViewState", viewId },
				{ "j_id111:j_id178", "j_id111:j_id178" }
			};

			using var prePostBody = new FormUrlEncodedContent(prePost);
			using var request = new HttpRequestMessage(HttpMethod.Post, URL_LVL6_SUBITEM) { Content = prePostBody };
			using var responseTariff = client.PostSyncEx(prePostBody, URL_LVL6_SUBITEM);

			if (IsConnected(responseTariff))
			{
				using var responseDetails = OpenTariffDetails(client, ViewId(responseTariff), tariff);
				if (IsConnected(responseDetails))
				{
					return ReadResponseAsString(responseDetails);
				}
			}

			return string.Empty;
		}

		static HttpResponseMessage OpenTariffDetails(HttpClient client, string viewId, string tariff)
		{
			var prePost = new Dictionary<string, string>
			{
				{ "j_id111:subItemNcmMB_tipoConsulta", "0" },
				{ "j_id111:valorConsulta_codigo", tariff },
				{ "j_id111:elementList:0:j_id201", "" },
				{ "j_id111:panelDownloadArquivoOpenedState", "" },
				{ "j_id111", "j_id111" },
				{ "autoScroll", "" },
				{ "javax.faces.ViewState", viewId }
			};

			using var prePostBody = new FormUrlEncodedContent(prePost);
			return client.PostSyncEx(prePostBody, URL_LVL6_SUBITEM);
		}

		static class RateCodes
		{
			public const string IPI = "1038";
			public const string PIS = "5602";
			public const string COFINS = "5629";
			public const string Duty = "0086";
		}
	}
}
