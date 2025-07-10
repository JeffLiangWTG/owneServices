using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAHarmonizedTariffs;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	public static class CATariffDataRunner
	{
		public static void Run()
		{
			var errorBuilder = new StringBuilder();

			var producers = GetProducers(errorBuilder);
			Parallel.Invoke(producers.Select(x => new Action(() => x.QueryDataAndParseToXMLFile())).ToArray());

			if (errorBuilder.Length > 0)
			{
				Program.PrintErrorMessage(errorBuilder.ToString());
			}
		}

		static IEnumerable<IProducer> GetProducers(StringBuilder errorBuilder)
		{
			// Replace with CARM API until CARM go live
			yield return new CARMRateDataProducer(new WebServiceCaller(errorBuilder), errorBuilder);
			var downloader = new TariffFileDownloader(ApplicationConfig.CBSABaseUrl, ApplicationConfig.CATariffDetailUrl, ApplicationConfig.CAConditionDetailUrl);
			yield return new CARMTariffDataProducer(new WebServiceCaller(errorBuilder), errorBuilder, downloader);
			//yield return new TreatmentCodesProducer();
			//yield return new TradeGroupDataProducer(errorBuilder, downloader);
			//yield return new TariffDataProducer(errorBuilder, downloader);
		}
	}
}
