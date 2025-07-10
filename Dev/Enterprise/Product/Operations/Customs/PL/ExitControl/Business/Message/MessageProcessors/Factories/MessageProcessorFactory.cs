using Enterprise.BatchProcessor;
using Enterprise.Customs.PL.Business;
using Enterprise.Messaging.MessageProcessors;
using AESMessages = Enterprise.Customs.PL.Business.AESMessageCodes.Descriptions;
using EDIMessages = Enterprise.Customs.PL.Business.Constants.EDIMessageSubType;
using Messages = Enterprise.Customs.PL.ExitControl.Business.ExitControlMessageCodes.Descriptions;

namespace Enterprise.Customs.PL.ExitControl.Business;

class MessageProcessorFactory : MessageProcessorFactoryBase
{
	protected override ApplicationTypeMessageProcessor CreateProcessorCore(EnterpriseEDIMessage message, LoggingInformation logger) =>
		(string)message.EM_MessageSubType switch
		{
			EDIMessages.UniversalRejection => new FaultMessageProcessor(logger),
			EDIMessages.Fault => new FaultMessageProcessor(logger),
			AESMessages.UPP => new UPPMessageProcessor(logger),
			AESMessages.NUP => new NUPMessageProcessor(logger),
			AESMessages.NPP => new NPPMessageProcessor(logger),
			AESMessages.UPO => new UPOMessageProcessor(logger),
			Messages.CC521 => new CC521CMessageProcessor(logger),
			Messages.CC522 => new CC522CMessageProcessor(logger),
			Messages.CC525 => new CC525CMessageProcessor(logger),
			Messages.CC557 => new CC557CMessageProcessor(logger),
			Messages.CC561 => new CC561CMessageProcessor(logger),
			_ => null,
		};
}
