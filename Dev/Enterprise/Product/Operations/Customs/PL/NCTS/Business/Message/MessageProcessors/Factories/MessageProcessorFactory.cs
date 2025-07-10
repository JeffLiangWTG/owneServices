using Enterprise.BatchProcessor;
using Enterprise.Customs.PL.Business;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Customs.PL.Business.Constants;
using ArrivalMessages = Enterprise.Customs.PL.NCTS.Business.CodeDescriptionPairLists.NctsArrivalMessageCodes.Descriptions;
using DepartureMessages = Enterprise.Customs.PL.NCTS.Business.CodeDescriptionPairLists.NctsDepartureMessageCodes.Descriptions;
using MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes;
using PLCommonMessages = Enterprise.Customs.PL.Business.CodeDescriptionPairLists.PLCommonMessageCodes.Descriptions;

namespace Enterprise.Customs.PL.NCTS.Business;

class MessageProcessorFactory : MessageProcessorFactoryBase
{
	protected override ApplicationTypeMessageProcessor CreateProcessorCore(EnterpriseEDIMessage message, LoggingInformation logger)
		=> CreateMessageTypeAgnosticProcessor(message, logger)
		?? (string)message.EM_MessageType switch
		{
			MessageType.NctsDeparture => CreateDepartureMessageProcessor(message, logger),
			MessageType.NctsArrivalNotification => CreateArrivalMessageProcessor(message, logger),
			_ => null,
		};

	ApplicationTypeMessageProcessor CreateMessageTypeAgnosticProcessor(EnterpriseEDIMessage message, LoggingInformation logger)
		=> (string)message.EM_MessageSubType switch
		{
			EDIMessageSubType.UniversalRejection => new FaultMessageProcessor(logger),
			EDIMessageSubType.Fault => new FaultMessageProcessor(logger),
			PLCommonMessages.UPO => new UPOMessageProcessor(logger),
			PLCommonMessages.UPP => new UPPMessageProcessor(logger),
			PLCommonMessages.NPP => new NPPMessageProcessor(logger),
			_ => null,
		};

	ApplicationTypeMessageProcessor CreateDepartureMessageProcessor(EnterpriseEDIMessage message, LoggingInformation logger)
		=> (string)message.EM_MessageSubType switch
		{
			DepartureMessages.IE004 => new CC004MessageProcessor(logger),
			DepartureMessages.IE009 => new CC009MessageProcessor(logger),
			DepartureMessages.IE022 => new CC022MessageProcessor(logger),
			DepartureMessages.IE028 => new CC028MessageProcessor(logger),
			DepartureMessages.IE029 => new CC029MessageProcessor(logger),
			DepartureMessages.IE029SC => new CC029SCMessageProcessor(logger),
			DepartureMessages.IE035 => new CC035MessageProcessor(logger),
			DepartureMessages.IE045 => new CC045MessageProcessor(logger),
			DepartureMessages.IE051 => new CC051MessageProcessor(logger),
			DepartureMessages.IE055 => new CC055MessageProcessor(logger),
			DepartureMessages.IE056 => new CC056MessageProcessor(logger),
			DepartureMessages.IE060 => new CC060MessageProcessor(logger),
			DepartureMessages.IE140 => new CC140MessageProcessor(logger),
			DepartureMessages.IE182 => new CC182MessageProcessor(logger),
			DepartureMessages.IE906 => new CC906MessageProcessor(logger),
			DepartureMessages.IE917 => new CC917MessageProcessor(logger),
			DepartureMessages.IE928 => new CC928MessageProcessor(logger),
			_ => null,
		};

	ApplicationTypeMessageProcessor CreateArrivalMessageProcessor(EnterpriseEDIMessage message, LoggingInformation logger)
		=> (string)message.EM_MessageSubType switch
		{
			ArrivalMessages.IE019 => new CC019MessageProcessor(logger),
			ArrivalMessages.IE025 => new CC025MessageProcessor(logger),
			ArrivalMessages.IE043 => new CC043MessageProcessor(logger),
			ArrivalMessages.IE057 => new CC057MessageProcessor(logger),
			DepartureMessages.IE917 => new CC917MessageProcessor(logger),
			_ => null,
		};
}
