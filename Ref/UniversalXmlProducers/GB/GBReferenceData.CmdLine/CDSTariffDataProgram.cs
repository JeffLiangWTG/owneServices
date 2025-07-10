using System;
using System.Text;
using CargoWise.RefDbRepo.GBReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.GBReferenceData.Services;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;

namespace CargoWise.RefDbRepo.GBReferenceData.CmdLine
{
	class CDSTariffDataProgram
	{
		public static void Run()
		{
			var dateTimeProvider = new SharedReferenceData.Services.Common.DateTimeProvider(ConfigurationProvider.TariffHistoricalPeriod);
			var errorCollector = new StringBuilder();

			var processManager = new Business.Tariff.ProcessManager(new IProcessor[]
			{
				new GoodsNomenclatureProcessor(dateTimeProvider, new IRefXmlBuilder[] { new GoodsNomenclatureBuilder(dateTimeProvider, errorCollector) }),
				new GBMeasureProcessor(dateTimeProvider, new MeasureMappingProvider(), new IRefXmlBuilder[] { new TariffBuilder(dateTimeProvider, errorCollector), new EUNVATBuilder(dateTimeProvider, errorCollector) }),
				new GeographicalAreaProcessor(dateTimeProvider, new IRefXmlBuilder[] { new TradeGroupBuilder(dateTimeProvider, errorCollector) }),
				new AdditionalCodeProcessor(dateTimeProvider, new IRefXmlBuilder[] { new AdditionalCodeBuilder(dateTimeProvider, errorCollector) })
			}, new ILoader[]
			{
				new BaseRegulationLoader(),
				new ModificationRegulationLoader(),
				new MeasureTypeLoader(),
				new MeasureConditionCodeLoader()
			});

			processManager.RunProcess(ConfigurationProvider.OutputDirectory, errorCollector);

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred:\r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
