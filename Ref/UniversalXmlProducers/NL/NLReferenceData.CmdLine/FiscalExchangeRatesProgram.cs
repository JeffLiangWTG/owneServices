using System;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Business;
using CargoWise.RefDbRepo.NLReferenceData.Services;

namespace CargoWise.RefDbRepo.NLReferenceData.CmdLine
{
	class FiscalExchangeRatesProgram
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();

			var processManagerFiscalExchangeRates = new FiscalExchangeRatesWebClientProcessManager(new FiscalExchangeRatesBuilder(errorCollector));
			processManagerFiscalExchangeRates.RunProcess(ApplicationConfig.OutputPath, errorCollector, DateTime.Today);

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred: \r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
