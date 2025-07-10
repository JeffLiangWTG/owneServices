using System;
using System.Collections.Generic;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class NctsBranchMessageProcessorProvider : Integration.Customs.NL.INctsMessageProcessorProvider
{
	public Dictionary<string, Type> NctsMessageProcessors => new()
	{
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC004C, typeof(CC004CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC006C, typeof(CC006CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC009C, typeof(CC009CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC019C, typeof(CC019CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC022C, typeof(CC022CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC025C, typeof(CC025CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC028C, typeof(CC028CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC029C, typeof(CC029CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC043C, typeof(CC043CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC045C, typeof(CC045CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC051C, typeof(CC051CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC055C, typeof(CC055CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC056C, typeof(CC056CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC057C, typeof(CC057CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC060C, typeof(CC060CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC061C, typeof(CC061CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC140C, typeof(CC140CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC182C, typeof(CC182CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC906C, typeof(CC906CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC917C, typeof(CC917CMessageProcessor) },
		{ NLNctsConstants.NctsMessageTypes.Incoming.CC928C, typeof(CC928CMessageProcessor) },
	};
}
