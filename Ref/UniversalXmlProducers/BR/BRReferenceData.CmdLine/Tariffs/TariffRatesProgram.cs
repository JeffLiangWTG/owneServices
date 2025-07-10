using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using CargoWise.RefDbRepo.BRReferenceData.Business;
using CargoWise.RefDbRepo.BRReferenceData.Services;
using Newtonsoft.Json;
using static CargoWise.RefDbRepo.BRReferenceData.Business.Constants.SpecialSituation;

namespace CargoWise.RefDbRepo.BRReferenceData.CmdLine
{
	public class TariffRatesProgram : GenericCheckUpdateProgram<IEnumerable<TariffDTO>>
	{
		public TariffRatesProgram()
		{
		}

		protected override string DataSourceFriendlyName => "BR Customs Tariff Rates file";

		protected override string LogFileSuffix => Constants.TariffRatesLogName;

		protected override void ExportToXMLFile(IEnumerable<TariffDTO> bFile)
		{
			var outputFileName = GetOutputFilePath($"RefCusTariff_BR_HSN_Rates.xml");
			var parser = new HSNTariffRatesParser(DataSourceFriendlyName);
			parser.ExportToXMLFile(bFile, outputFileName, DateTime.Now);
		}

		protected override IEnumerable<TariffDTO> DowloadContent()
		{
			using var handler = new HttpClientHandler() { UseCookies = true };
			using var client = GetHttpClient(handler);
			return TariffRatesManager.GetTariffsMultiThread(client);
		}

		protected virtual TariffRatesManager TariffRatesManager => new();

		protected override void CheckDowloadContent(IEnumerable<TariffDTO> downloadedContent)
		{
			if (!downloadedContent?.Any() ?? false)
			{
				throw new InvalidOperationException($"The application was unable to verify the last version {DataSourceFriendlyName}.");
			}
		}

		protected override bool CompareDownloadedContentWithLog(IEnumerable<TariffDTO> downloadedContent)
		{
			var jsonDownloaded = JsonConvert.SerializeObject(SortDownloadedContent(downloadedContent));
			var jsonLog = File.ReadAllText(LogFilePath);
			return !jsonLog.Equals(jsonDownloaded, StringComparison.Ordinal);
		}

		public override void UpdateLogFile(IEnumerable<TariffDTO> downloadedContent)
		{
			File.WriteAllText(LogFilePath, JsonConvert.SerializeObject(SortDownloadedContent(downloadedContent)));
		}

		static IEnumerable<TariffDTO> SortDownloadedContent(IEnumerable<TariffDTO> downloadedContent)
		{
			var list = downloadedContent.ToList();
			list.Sort(delegate (TariffDTO x, TariffDTO y)
			{
				return string.Compare(x?.Code, y?.Code, StringComparison.Ordinal);
			});
			return list;
		}
	}
}
