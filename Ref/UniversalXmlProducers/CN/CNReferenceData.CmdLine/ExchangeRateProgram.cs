using System;
using System.Globalization;
using System.IO;
using CargoWise.RefDbRepo.CNReferenceData.Business;
using CargoWise.RefDbRepo.CNReferenceData.Services;

namespace CargoWise.RefDbRepo.CNReferenceData.CmdLine
{
	public static class ExchangeRateProgram
	{
		public static void Run(DateTime? today = null, IHttpHandler httpHandler = null)
		{
			today = (today ?? DateTime.UtcNow.ToChinaStandardTime()).Date;

			CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;

			var sourceUrl = GlobalOption.Instance.Setting.ExchangeRateSourceURL;
			var outputFolder = GlobalOption.Instance.Setting.OutputFileFolderPath;

			var collectionTime = CollectionDateHelper.DefineCollectionDate(today.Value);
			if (collectionTime.Date == today)
			{
				var collector = new CNExchangeRateParser(sourceUrl, today);
				var filePath = Path.Combine(outputFolder, $"RefExchangeRateZZ_CN_{collectionTime.ToString("yyyyMMdd", CultureInfo.InvariantCulture)}.xml");
				collector.ExportToXMLFile(filePath, httpHandler ?? new HttpClientHandler());
				Console.Write("Finish");
			}
			else
			{
				Console.Write($"Today {today:yyyy-MM-dd} is not collection date ({collectionTime.Date:yyyy-MM-dd}).");
			}
		}
	}
}
