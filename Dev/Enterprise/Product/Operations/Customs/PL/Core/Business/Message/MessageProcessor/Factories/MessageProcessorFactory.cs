using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Customs.PL.Business.Constants;
using Messages = Enterprise.Customs.PL.Business.AESMessageCodes.Descriptions;
using MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes;

namespace Enterprise.Customs.PL.Business;

class MessageProcessorFactory : MessageProcessorFactoryBase
{
	protected override ApplicationTypeMessageProcessor CreateProcessorCore(EnterpriseEDIMessage message, LoggingInformation logger)
		=> CreateMessageTypeAgnosticProcessor(message, logger)
		?? (string)message.EM_MessageType switch
		{
			MessageType.Export => CreateExportMessageProcessor(message, logger),
			MessageType.Import => null,
			_ => null,
		};

	ApplicationTypeMessageProcessor CreateMessageTypeAgnosticProcessor(EnterpriseEDIMessage message, LoggingInformation logger)
		=> (string)message.EM_MessageSubType switch
		{
			EDIMessageSubType.UniversalRejection => new FaultMessageProcessor(logger),
			EDIMessageSubType.Fault => new FaultMessageProcessor(logger),
			Messages.UPP => new UPPMessageProcessor(logger),
			Messages.NUP => new NUPMessageProcessor(logger),
			Messages.NPP => new NPPMessageProcessor(logger),
			Messages.UPO => new UPOMessageProcessor(logger),
			_ => null,
		};

	ApplicationTypeMessageProcessor CreateExportMessageProcessor(EnterpriseEDIMessage message, LoggingInformation logger)
		=> (string)message.EM_MessageSubType switch
		{
			Messages.CC504 => new CC504CMessageProcessor(logger),
			Messages.CC509 => new CC509CMessageProcessor(logger),
			Messages.CC525 => new CC525CMessageProcessor(logger),
			Messages.CC528 => new CC528CMessageProcessor(logger),
			Messages.CC529 => new CC529CMessageProcessor(logger),
			Messages.CC531 => new CC531CMessageProcessor(logger),
			Messages.CC548 => new CC548CMessageProcessor(logger),
			Messages.CC551 => new CC551CMessageProcessor(logger),
			Messages.CC556 => new CC556CMessageProcessor(logger),
			Messages.CC571 => new CC571CMessageProcessor(logger),
			Messages.CC574 => new CC574CMessageProcessor(logger),
			Messages.CC582 => new CC582CMessageProcessor(logger),
			Messages.CC599 => new CC599CMessageProcessor(logger),
			Messages.CC604 => new CC604CMessageProcessor(logger),
			Messages.CC609 => new CC609CMessageProcessor(logger),
			Messages.CC628 => new CC628CMessageProcessor(logger),
			_ => null,
		};
}
