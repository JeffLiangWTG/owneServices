using System;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Business.ExchangeRates;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.GBReferenceData.Services.Common;

namespace CargoWise.RefDbRepo.GBReferenceData.CmdLine
{
	class ExchangeRateProgram
	{
		public static void Run()
		{
			var errorCollector = new StringBuilder();
			var dateTimeProvider = new SharedReferenceData.Services.Common.DateTimeProvider(0);
			var webClientWrapper = new WebClientWrapper();

			new ExchangeRateBuilder(dateTimeProvider, errorCollector, webClientWrapper).RunProcess(ConfigurationProvider.OutputDirectory);

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred:\r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
