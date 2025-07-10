using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ExportExitInformationMessageSender : NLMessageSender
{
	public ExportExitInformationMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override void PostSendProcess(NLEDIMessage newMessage)
	{
		base.PostSendProcess(newMessage);
		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._583;
	}

	protected override ZString MessageSubType => ExportSendMessageTypes.Codes.EXT;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.InformationOnNonExitedExport;
}
