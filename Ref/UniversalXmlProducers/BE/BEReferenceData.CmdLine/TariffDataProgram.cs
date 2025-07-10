using System;
using System.Text;
using CargoWise.RefDbRepo.BEReferenceData.Business;
using CargoWise.RefDbRepo.BEReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.BEReferenceData.Services;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff;
using CargoWise.RefDbRepo.SharedReferenceData.Business.Tariff.Processors;
using CargoWise.RefDbRepo.SharedReferenceData.Services.Tariff.Loaders;

namespace CargoWise.RefDbRepo.BEReferenceData.CmdLine
{
	sealed class TariffDataProgram
	{
		public static void Run()
		{
			var dateTimeProvider = new SharedReferenceData.Services.Common.DateTimeProvider(ApplicationConfig.TariffHistoricalPeriod);
			var errorCollector = new StringBuilder();
			var processManager = new Business.ProcessManager(new IProcessor[]
			{
				new GoodsNomenclatureProcessor(dateTimeProvider, Array.Empty<IRefXmlBuilder>()),
				new AdditionalCodeProcessor(dateTimeProvider, new IRefXmlBuilder[] { new AdditionalCodeBuilder(dateTimeProvider, errorCollector) }),
				new GeographicalAreaProcessor(dateTimeProvider, new IRefXmlBuilder[] { new TradeGroupBuilder(dateTimeProvider, errorCollector) }),
				new BEMeasureProcessor(dateTimeProvider, new MeasureMappingProvider(), new IRefXmlBuilder[] { new TariffBuilder(dateTimeProvider, errorCollector) }),
			}, new ILoader[]
			{
				new BaseRegulationLoader(),
				new ModificationRegulationLoader(),
				new MeasureTypeLoader(),
				new MeasureConditionCodeLoader()
			});

			processManager.RunProcess(ApplicationConfig.OutputDirectory, errorCollector);

			if (errorCollector.Length > 0)
			{
				var errorMessage = $"The following processing notifications occurred:\r\n{errorCollector}";
				Console.Error.WriteLine(errorMessage);
			}
		}
	}
}
