using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ExportPresentationMessageSender : NLMessageSender
{
	public ExportPresentationMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override void PostSendProcess(NLEDIMessage newMessage)
	{
		base.PostSendProcess(newMessage);

		entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._511;
		entryHeader.CH_EntryStatus = NLConstants.EntryStatusNew.PreLodged;
	}

	protected override ZString MessageSubType => ExportSendMessageTypes.Codes.PRE;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.ExportPresentation;
}
