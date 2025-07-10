using System;
using System.IO;
using System.Text;
using CargoWise.RefDbRepo.CAReferenceData.Business;
using CargoWise.RefDbRepo.CAReferenceData.Business.CAExchangeRate;

namespace CargoWise.RefDbRepo.CAReferenceData.CmdLine
{
	public static class CAExchangeRate
	{
		public static void Run()
		{
			var errorBuilder = new StringBuilder();
			var exportFilePath = Path.Combine(ApplicationConfig.OutputPath, ApplicationConfig.CBSAExchangeRatesFileName);
			var webServiceCaller = new WebServiceCaller();
			var action = new Action(() =>
			{
				new CAExchangeRateProducer(webServiceCaller, errorBuilder, new CAExchangeRateParser()).QueryDataAndParseToXMLFile(exportFilePath);
				if (errorBuilder.Length > 0)
				{
					Program.PrintErrorMessage(errorBuilder.ToString());
				}
			});
			action.Invoke();
		}
	}
}
