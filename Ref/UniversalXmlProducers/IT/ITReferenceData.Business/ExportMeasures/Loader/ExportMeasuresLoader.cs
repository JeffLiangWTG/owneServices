using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.ITReferenceData.Business.Shared;
using CargoWise.RefDbRepo.ITReferenceData.Services;
using HtmlAgilityPack;

namespace CargoWise.RefDbRepo.ITReferenceData.Business.ExportMeasures
{
	public sealed class ExportMeasuresLoader : IExportMeasuresLoader
	{
		public ExportMeasuresLoader(ILogger logger, IDateTimeProvider dateTimeProvider, HttpClient httpClient)
		{
			this.logger = Argument.NotNull(logger, nameof(logger));
			this.dateTimeProvider = Argument.NotNull(dateTimeProvider, nameof(dateTimeProvider));
			this.httpClient = Argument.NotNull(httpClient, nameof(httpClient));
		}

		public async Task DoHandshakeAsync()
		{
			var parameters = new HandshakeRequestObject(dateTimeProvider).Build();
			using var formContent = new FormUrlEncodedContent(parameters);
			_ = await httpClient.PostWithRetryAsync(ApplicationConfig.TaricServletUri, formContent);
		}

		public async Task<IReadOnlyCollection<MeasureInformation>> GetMeasureInformationAsync(string tariffCode)
		{
			var parameters = new TariffInformationRequestObject(dateTimeProvider, tariffCode).Build();
			using var formContent = new FormUrlEncodedContent(parameters);
			using var response = await httpClient.PostWithRetryAsync(ApplicationConfig.MisureServletUri, formContent);
			var responseString = await response.Content.ReadAsStringAsync();

			var htmlDocument = new HtmlDocument();
			htmlDocument.LoadHtml(responseString);

			var table = htmlDocument.DocumentNode.SelectSingleNode("//tbody//table[.//td[1][text() = 'Nazionali']]");

			if (table == null)
			{
				return [];
			}

			var rows = table.SelectNodes(".//tr[td[@class='TDOUTPUTSX']]");

			if (rows == null)
			{
				logger.Log($"Tariff {tariffCode} - Failed to parse rows");
				return [];
			}

			var measures = new List<MeasureInformation>(rows.Count);

			foreach (var row in rows)
			{
				var tradeGroupColumn = row.SelectSingleNode(".//td[contains(., 'Monitoraggio Statistico Export')]/a");
				var tradeGroup = tradeGroupColumn?.InnerText?.Trim();

				if (tradeGroup == null)
				{
					continue;
				}

				var additionalCodeColumn = row.SelectSingleNode(".//td[contains(., 'Cadd:')]");
				var additionalCodeLink = additionalCodeColumn?.SelectSingleNode(".//a[preceding-sibling::text()[contains(., 'Cadd:')]]");
				var additionalCode = additionalCodeLink?.InnerText?.Trim();

				if (additionalCode == null)
				{
					logger.Log($"Tariff {tariffCode} - Failed to parse additional code for trade group '{tradeGroup}'");
					continue;
				}

				measures.Add(new MeasureInformation(tradeGroup, additionalCode));
			}

			return measures;
		}

		readonly ILogger logger;
		readonly IDateTimeProvider dateTimeProvider;
		readonly HttpClient httpClient;
	}
}
