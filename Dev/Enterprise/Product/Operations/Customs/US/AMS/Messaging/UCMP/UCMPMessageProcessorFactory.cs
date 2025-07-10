using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	class UCMPMessageProcessorFactory : CommonUCMPMessageProcessorFactory
	{
		public static IKeysForBlockingParallelProcessingProvider GetMessageProcessor(ZString messageType, LoggingInformation logger)
		{
			var fixedType = messageType.ToString().ToUpperInvariant();
			switch (fixedType)
			{
				case AMSApplicationIdentifierCodeList.Codes.ManifestCreateResponse:
				case AMSApplicationIdentifierCodeList.Codes.ManifestCreateTransmissionResponse:
				case AMSApplicationIdentifierCodeList.Codes.ManifestAmendmentResponse:
				case AMSApplicationIdentifierCodeList.Codes.ManifestEditResponse:
					return new ManifestProcessor();
				case AMSApplicationIdentifierCodeList.Codes.StatusNotification:
					return new StatusNotificationProcessor();
				case EDIInterchange.ApplicationCodes.StowPlan:
					return new CusresMessageProcessor(logger);
				default:
					return new UnsupportedTypeMessageProcessor(logger);
			}
		}
	}
}
