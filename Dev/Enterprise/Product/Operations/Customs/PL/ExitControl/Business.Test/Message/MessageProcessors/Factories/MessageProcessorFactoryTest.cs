using System;
using System.Collections.Generic;
using Enterprise.Customs.PL.Business;
using Enterprise.Customs.PL.Business.Testing;
using NUnit.Framework;
using AESMessages = Enterprise.Customs.PL.Business.AESMessageCodes.Descriptions;
using EDIMessages = Enterprise.Customs.PL.Business.Constants.EDIMessageSubType;
using Messages = Enterprise.Customs.PL.ExitControl.Business.ExitControlMessageCodes.Descriptions;
using MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes;

namespace Enterprise.Customs.PL.ExitControl.Business.Testing;

[TestedType(typeof(MessageProcessorFactory))]
class MessageProcessorFactoryTest : MessageProcessorFactoryTestBase<EDIMessage>
{
	protected override IReadOnlyCollection<(string MessageType, string MessageName, Type ExpectedProcessorType)> ExpectedProcessorTypes =>
	[
		(MessageType.MiscellaneousCustoms, EDIMessages.UniversalRejection, typeof(FaultMessageProcessor)),
		(MessageType.MiscellaneousCustoms, EDIMessages.Fault, typeof(FaultMessageProcessor)),
		(MessageType.MiscellaneousCustoms, AESMessages.UPP, typeof(UPPMessageProcessor)),
		(MessageType.MiscellaneousCustoms, AESMessages.NUP, typeof(NUPMessageProcessor)),
		(MessageType.MiscellaneousCustoms, AESMessages.NPP, typeof(NPPMessageProcessor)),
		(MessageType.MiscellaneousCustoms, AESMessages.UPO, typeof(UPOMessageProcessor)),
		(MessageType.MiscellaneousCustoms, Messages.CC521, typeof(CC521CMessageProcessor)),
		(MessageType.MiscellaneousCustoms, Messages.CC522, typeof(CC522CMessageProcessor)),
		(MessageType.MiscellaneousCustoms, Messages.CC525, typeof(CC525CMessageProcessor)),
		(MessageType.MiscellaneousCustoms, Messages.CC557, typeof(CC557CMessageProcessor)),
		(MessageType.MiscellaneousCustoms, Messages.CC561, typeof(CC561CMessageProcessor)),
	];

	protected override MessageProcessorFactoryBase CreateMessageProcessorFactory()
		=> new MessageProcessorFactory();
}
