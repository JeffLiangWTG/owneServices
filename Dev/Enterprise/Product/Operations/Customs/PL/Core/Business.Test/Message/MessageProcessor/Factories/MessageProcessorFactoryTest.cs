using System;
using System.Collections.Generic;
using NUnit.Framework;
using Messages = Enterprise.Customs.PL.Business.AESMessageCodes.Descriptions;
using MessageType = Enterprise.Customs.Common.EU.EUJobMessageTypeList.Codes;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(MessageProcessorFactory))]
class MessageProcessorFactoryTest : MessageProcessorFactoryTestBase<EDIMessage>
{
	protected override IReadOnlyCollection<(string MessageType, string MessageName, Type ExpectedProcessorType)> ExpectedProcessorTypes => new[] {
		(MessageType.Export, Messages.CC504, typeof(CC504CMessageProcessor)),
		(MessageType.Export, Messages.CC509, typeof(CC509CMessageProcessor)),
		(MessageType.Export, Messages.CC525, typeof(CC525CMessageProcessor)),
		(MessageType.Export, Messages.CC528, typeof(CC528CMessageProcessor)),
		(MessageType.Export, Messages.CC529, typeof(CC529CMessageProcessor)),
		(MessageType.Export, Messages.CC531, typeof(CC531CMessageProcessor)),
		(MessageType.Export, Messages.CC548, typeof(CC548CMessageProcessor)),
		(MessageType.Export, Messages.CC551, typeof(CC551CMessageProcessor)),
		(MessageType.Export, Messages.CC556, typeof(CC556CMessageProcessor)),
		(MessageType.Export, Messages.CC571, typeof(CC571CMessageProcessor)),
		(MessageType.Export, Messages.CC574, typeof(CC574CMessageProcessor)),
		(MessageType.Export, Messages.CC582, typeof(CC582CMessageProcessor)),
		(MessageType.Export, Messages.CC599, typeof(CC599CMessageProcessor)),
		(MessageType.Export, Messages.CC604, typeof(CC604CMessageProcessor)),
		(MessageType.Export, Messages.CC609, typeof(CC609CMessageProcessor)),
		(MessageType.Export, Messages.CC628, typeof(CC628CMessageProcessor)),
		(MessageType.Export, Messages.UPP, typeof(UPPMessageProcessor)),
		(MessageType.Import, Messages.UPP, typeof(UPPMessageProcessor)),
		(MessageType.Export, Messages.NPP, typeof(NPPMessageProcessor)),
		(MessageType.Import, Messages.NPP, typeof(NPPMessageProcessor)),
		(MessageType.Export, Messages.NUP, typeof(NUPMessageProcessor)),
		(MessageType.Import, Messages.NUP, typeof(NUPMessageProcessor)),
		(MessageType.Export, Messages.UPO, typeof(UPOMessageProcessor)),
		(MessageType.Import, Messages.UPO, typeof(UPOMessageProcessor)),
	};

	protected override MessageProcessorFactoryBase CreateMessageProcessorFactory()
		=> new MessageProcessorFactory();
}
