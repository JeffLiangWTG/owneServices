using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.BatchProcessor;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.MessageProcessors
{
	public class TWMessageProcessor(LoggingInformation logger) : TWCApplicationTypeMessageProcessor(logger)
	{
		public override (ZGuid BranchPK, CargoWise.EntityFramework.BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message)
		{
			return default;
		}

		protected override bool RequiresPreProcessingCore => false;

		protected override bool ProcessMessageMain(TWMessage message)
		{
			var successful = false;
			var errorInMessage = message.ErrorText;
			if (!errorInMessage.IsEmpty)
			{
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				Logger.LogError(GetFailedToParseTheMessage(message, errorInMessage));
			}
			else
			{
				var messageProcessor = TWCMessageProcessorFactory.GetMessageProcessor(message, Logger);
				if (messageProcessor is TWCApplicationTypeMessageProcessor twcProcessor)
				{
					var linkedObjectResult = twcProcessor.TryFindLinkedObject(message);
					if (linkedObjectResult.LinkedObject is { } linkedObject)
					{
						message.EM_LinkedObject = linkedObject;
						twcProcessor.ProcessMessage(message);
						successful = true;
					}
					else
					{
						message.EM_Status = EDIMessageStatusList.Codes.Discarded;
					}
				}
				else
				{
					message.EM_Status = EDIMessageStatusList.Codes.Failed;
				}
			}
			return successful;
		}
	}
}
