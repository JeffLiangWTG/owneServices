using CargoWise.RefDbRepo.ZAReferenceData.Business.ExchangeRates;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Configuration;

namespace CargoWise.RefDbRepo.ZAReferenceData.CmdLine
{
	public class ExchangeRateRunner : Runner
	{
		public ExchangeRateRunner(ILogger logger) : base(logger)
		{
		}

		protected override void RunCore()
		{
			using (var messageHandler = ObjectFactoryHelper.GetMessageHandler(Logger, SupportedMessageTypes.Gesmes))
			{
				var processor = new ExchangeRateProcessor(messageHandler, Logger);
				processor.Run(ConfigurationProvider.OutputFolder);
			}
		}
	}
}
