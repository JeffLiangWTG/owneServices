using CargoWise.RefDbRepo.ZAReferenceData.Services.Common;
using CargoWise.RefDbRepo.ZAReferenceData.Services.Common.Models;
using CargoWise.RefDbRepo.ZAReferenceData.Services.ExchangeRates;

namespace CargoWise.RefDbRepo.ZAReferenceData.Business.ExchangeRates
{
	public class ExchangeRateProcessor
	{
		public ExchangeRateProcessor(IMessageHandler messageHandler, ILogger logger)
		{
			this.messageHandler = messageHandler;
			this.logger = logger;
		}
		readonly IMessageHandler messageHandler;
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

		bool ProcessMessage(SourceDataMessage gesmesMsg, string outputFolder)
		{
			var exchangeRateData = MessageConversionHelper.Convert(gesmesMsg, logger);
			var success = exchangeRateData != null;

			if (success)
			{
				var builder = new ExchangeRateBuilder(exchangeRateData, logger);
				builder.CreateXmlFile(outputFolder, gesmesMsg.CreatedDate);
			}

			return success;
		}
	}
}
