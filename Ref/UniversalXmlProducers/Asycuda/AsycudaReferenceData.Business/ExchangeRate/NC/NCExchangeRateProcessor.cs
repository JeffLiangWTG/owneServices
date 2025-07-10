using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.AsycudaReferenceData.Services;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using Microsoft.IdentityModel.Tokens;

namespace CargoWise.RefDbRepo.AsycudaReferenceData.Business;

public class NCExchangeRateProcessor
{
	public NCExchangeRateProcessor(string outputPath)
	{
		OutputPath = outputPath;
	}
	string OutputPath { get; }

	public async Task Run()
	{
		var tempDirPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		Directory.CreateDirectory(tempDirPath);
		try
		{
			using var httpClient = new HttpClient();
			var downloader = new NCPdfDownloader(httpClient);
			var pdfPath = await downloader.DownloadLatestPdf(GetExchangeRatesDownloadUrl(), tempDirPath);
			if (pdfPath != null && NCExchangeRatePDFParser.ParsePDF(pdfPath) is { } exchangeRates && !exchangeRates.ExchangeRateDetails.IsNullOrEmpty())
			{
				var writerConfiguration = XmlWriterHelper.GetExchangeRateWriterConfiguration(exchangeRates.StartDate, exchangeRates.EndDate, Constants.NewCaledonia);
				var writer = new XmlWriter(writerConfiguration);
				writer.SetDataSource(DataSource);
				writer.SetPublicationTime(DateTime.Now);
				writer.SetUpdateType(UpdateType.Full);

				foreach (var exchangeRate in exchangeRates.ExchangeRateDetails)
				{
					var code = exchangeRate.Code;
					var newRate = new RefExchangeRateZZ()
					{
						ZZN_RX_NKExCurrency = code,
						ZZN_Rate = exchangeRate.Rate
					};
					writer.PopulateData(newRate);
				}
				writer.SaveXml(Path.Combine(OutputPath, "RefExchangeRateZZ_NC.xml"));
			}
		}
		finally
		{
			Directory.Delete(tempDirPath, true);
		}
	}

	public static Uri GetExchangeRatesDownloadUrl(TimeProvider timeProvider = null)
	{
		timeProvider ??= TimeProvider.System;

		var twoMonthsAgo = timeProvider.GetUtcNow().DateTime.AddMonths(-2);

#pragma warning disable CA1308 // Normalize strings to uppercase
		var formattedDate = twoMonthsAgo.ToString("MMMM-yyyy", new CultureInfo("fr-FR")).ToLower(CultureInfo.InvariantCulture);
#pragma warning restore CA1308 // Normalize strings to uppercase

		formattedDate = formattedDate.Replace('é', 'e').Replace('û', 'u');
		return new Uri(BaseUrl.Replace(Placeholder, formattedDate));
	}

	const string BaseUrl = "https://douane.gouv.nc/taux-de-change-{placeholder}";
	const string Placeholder = "{placeholder}";
	const string DataSource = "NC Exchange Rate";
}
