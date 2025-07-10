using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Helpers;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces;

namespace CargoWise.RefDbRepo.ZAReferenceData.Business.Tariff
{
	public class TariffProcessor
	{
		public TariffProcessor(IMessageHandler messageHandler, ITariffHelper tariffHelper, ICountryCodeLoader countryCodeLoader, ILogger logger)
		{
			this.messageHandler = messageHandler;
			this.tariffHelper = tariffHelper;
			this.countryCodeLoader = countryCodeLoader;
			this.logger = logger;
		}
		readonly IMessageHandler messageHandler;
		readonly ITariffHelper tariffHelper;
		readonly ICountryCodeLoader countryCodeLoader;
		readonly ILogger logger;

		public void Run(string outputFolder)
		{
			foreach (var msg in messageHandler.GetMessages())
			{
				logger.LogInfo($"Processing message: {msg.ID} {msg.Filename}");
				var success = ProcessMessage(msg, outputFolder);

				messageHandler.UpdateStatus(msg, success);
			}
		}

		bool ProcessMessage(SourceDataMessage prodatMsg, string outputFolder)
		{
			var header = MessageConversionHelper.Convert(prodatMsg, logger);
			var success = header != null;

			if (success)
			{
				header.ProcessUpdates(countryCodeLoader, tariffHelper, logger);

				var builder = new TariffBuilder(header, logger);
				builder.CreateXmlFile(outputFolder, prodatMsg.CreatedDate);
			}

			return success;
		}
	}
}
