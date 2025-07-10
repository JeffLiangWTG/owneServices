using System;
using System.Collections.Generic;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Testing;
using NUnit.Framework;
using ArrivalMessages = Enterprise.Customs.PL.NCTS.Business.CodeDescriptionPairLists.NctsArrivalMessageCodes.Descriptions;
using DepartureMessages = Enterprise.Customs.PL.NCTS.Business.CodeDescriptionPairLists.NctsDepartureMessageCodes.Descriptions;
using MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes;
using PLCommonMessages = Enterprise.Customs.PL.Business.CodeDescriptionPairLists.PLCommonMessageCodes.Descriptions;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

[TestedType(typeof(MessageProcessorFactory))]
class MessageProcessorFactoryTest : MessageProcessorFactoryTestBase<EDIMessage>
{
	protected override IReadOnlyCollection<(string MessageType, string MessageName, Type ExpectedProcessorType)> ExpectedProcessorTypes =>
	[
		(MessageType.NctsDeparture, PLCommonMessages.UPO, typeof(UPOMessageProcessor)),
		(MessageType.NctsArrivalNotification, PLCommonMessages.UPO, typeof(UPOMessageProcessor)),
		(MessageType.NctsDeparture, PLCommonMessages.UPP, typeof(UPPMessageProcessor)),
		(MessageType.NctsArrivalNotification, PLCommonMessages.UPP, typeof(UPPMessageProcessor)),
		(MessageType.NctsDeparture, PLCommonMessages.NPP, typeof(NPPMessageProcessor)),
		(MessageType.NctsArrivalNotification, PLCommonMessages.NPP, typeof(NPPMessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE022, typeof(CC022MessageProcessor)),
		(MessageType.NctsArrivalNotification, ArrivalMessages.IE025, typeof(CC025MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE004, typeof(CC004MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE009, typeof(CC009MessageProcessor)),
		(MessageType.NctsArrivalNotification, ArrivalMessages.IE019, typeof(CC019MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE028, typeof(CC028MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE029, typeof(CC029MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE029SC, typeof(CC029SCMessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE035, typeof(CC035MessageProcessor)),
		(MessageType.NctsArrivalNotification, ArrivalMessages.IE043, typeof(CC043MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE045, typeof(CC045MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE051, typeof(CC051MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE055, typeof(CC055MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE056, typeof(CC056MessageProcessor)),
		(MessageType.NctsArrivalNotification, ArrivalMessages.IE057, typeof(CC057MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE060, typeof(CC060MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE140, typeof(CC140MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE182, typeof(CC182MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE906, typeof(CC906MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE917, typeof(CC917MessageProcessor)),
		(MessageType.NctsArrivalNotification, DepartureMessages.IE917, typeof(CC917MessageProcessor)),
		(MessageType.NctsDeparture, DepartureMessages.IE928, typeof(CC928MessageProcessor)),
	];

	protected override MessageProcessorFactoryBase CreateMessageProcessorFactory()
		=> new MessageProcessorFactory();
}
