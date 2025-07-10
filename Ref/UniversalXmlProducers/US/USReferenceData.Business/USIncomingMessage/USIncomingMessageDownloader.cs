using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.eHub.Adapter;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.USReferenceData.Services;
using static CargoWise.RefDbRepo.USReferenceData.Business.Constants;

namespace CargoWise.RefDbRepo.USReferenceData.Business.USIncomingMessageProcessor
{
	public class USIncomingMessageDownloader
	{
		public USIncomingMessageDownloader(IeHubAdapter adapter, string outputPath, string csvFilePath)
		{
			Argument.NotNull(adapter, nameof(adapter));
			this.adapter = adapter;
			this.outputPath = outputPath;
			this.csvFilePath = csvFilePath;
		}

		readonly IeHubAdapter adapter;
		readonly string outputPath;
		readonly string csvFilePath;
		IReadOnlyList<ForeignPort> foreignPorts;

		public ActionResult Run()
		{
			try
			{
				if (CanRetrieveMessages() && adapter.Inbox.Count > 0)
				{
					foreach (var message in adapter.Inbox)
					{
						ReadMessageAndParse2XML(message);
					}
					adapter.Inbox.MarkAsRead();
				}
				return ActionResult.Successful;
			}
			catch (EndpointNotFoundException ex)
			{
				Console.Error.WriteLine(ex.Message);
				return ActionResult.Failed;
			}
		}

		bool CanRetrieveMessages()
		{
			var retryTimes = ApplicationConfig.Instance.USIncomingMessageRetryTimes;
			if (retryTimes == 0)
			{
				retryTimes = 1;
			}
			for (var retryCount = 0; retryCount < retryTimes; retryCount++)
			{
				try
				{
					adapter.RetrieveMessages();
					return true;
				}
				catch (Exception)
				{
					Thread.Sleep(ApplicationConfig.Instance.USIncomingMessageMillisecondsBetweenRetries);
					continue;
				}

			}
			return false;
		}

		void ReadMessageAndParse2XML(IeHubMessage message)
		{
			using (var stream = new StreamReader(message.MessageStream))
			{
				var messageText = stream.ReadToEnd();
				var pattern = @"<Body><!\[CDATA\[(.*?)\]\]></Body>";
				var match = Regex.Match(messageText, pattern);
				if (match.Success)
				{
					ProcessMessageText(match.Value, 25, 95);
				}
				else
				{
					ProcessMessageText(messageText, 150, 220);
				}
			}
		}

		void ProcessMessageText(string messageText, int messageTypeIndex, int identifierIndex)
		{
			if (messageText.Length < messageTypeIndex + 2)
			{
				Console.WriteLine("Unable to parse message type, message: {0}.", messageText);
				return;
			}

			var messageTypeCode = messageText.Substring(messageTypeIndex, 2);
			var identifier = messageText.Substring(identifierIndex, 4);
			var processor = GetProcessor(messageTypeCode, identifier);
			if (processor == null)
			{
				Console.WriteLine("Unknow message type: {0} ignored.", messageTypeCode);
				return;
			}
			processor.Process(messageText);
		}

		USIncomingMessageProcessor GetProcessor(string messageTypeCode, string identifier)
		{
			if (ApplicationConfig.Instance.USIncomingMessageResponseMessageType.Equals(messageTypeCode, StringComparison.Ordinal))
			{
				switch (identifier)
				{
					case USIncomingMessageRequest.F104:
						return new ScheduleKPortCodeMessageProcessor(outputPath, GetForeignPorts());
					case USIncomingMessageRequest.F101:
						return new RegionDistrictPortCodeMessageProcessor(outputPath);
					case USIncomingMessageRequest.F211:
						return new FirmsCodeMessageProcessor(outputPath);
				}
			}
			if (ApplicationConfig.Instance.CurrencyExchangeRateMessageType.Equals(messageTypeCode, StringComparison.Ordinal))
			{
				return new CurrencyExchangeRateMessageProcessor(outputPath);
			}
			return null;
		}

		IReadOnlyList<ForeignPort> GetForeignPorts()
		{
			if (foreignPorts == null)
			{
				var csvParser = new CsvParser();
				foreignPorts = csvParser.Parse<ForeignPort>(csvFilePath).AsReadOnly();
			}
			return foreignPorts;
		}
	}
}
