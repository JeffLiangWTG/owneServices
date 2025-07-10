using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Configuration;

namespace CargoWise.RefDbRepo.ZAReferenceData.CmdLine
{
	public class TariffRunner : Runner
	{
		public TariffRunner(ILogger logger) : base(logger)
		{
		}

		protected override void RunCore()
		{
			using (var messageHandler = ObjectFactoryHelper.GetMessageHandler(Logger, SupportedMessageTypes.Prodat))
			{
				var countryCodeLoader = ObjectFactoryHelper.GetCountryCodeLoader(Logger);
				var tariffHelper = ObjectFactoryHelper.GetTariffHelper(Logger);

				var tariffProcessor = new Business.Tariff.TariffProcessor(messageHandler, tariffHelper, countryCodeLoader, Logger);

				tariffProcessor.Run(ConfigurationProvider.OutputFolder);
			}
		}
	}
}
