using System;
using System.Text;
using CargoWise.RefDbRepo.NLReferenceData.Business;
using CargoWise.RefDbRepo.NLReferenceData.Services;

namespace CargoWise.RefDbRepo.NLReferenceData.CmdLine
{
	class CustomsExchangeRatesProgram
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();
			var exchangeRateBuilder = new CustomsExchangeRatesBuilder(errorCollector);
			var processManagerExchangeRates = new CustomsExchangeRatesProcessManager(exchangeRateBuilder);
			processManagerExchangeRates.RunProcess(ApplicationConfig.OutputPath, errorCollector, DateTime.Today);

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred: \r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
