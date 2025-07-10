using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ExportDeclarationMessageSender : NLMessageSender
{
	public ExportDeclarationMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override void PostSendProcess(NLEDIMessage newMessage)
	{
		base.PostSendProcess(newMessage);

		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._515;
	}

	protected override ZString MessageSubType => ExportSendMessageTypes.Codes.DEC;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.ExportDeclaration;
}
