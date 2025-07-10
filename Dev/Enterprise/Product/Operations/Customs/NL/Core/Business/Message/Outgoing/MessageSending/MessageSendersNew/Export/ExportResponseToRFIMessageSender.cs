using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ExportResponseToRFIMessageSender : NLMessageSender
{
	public ExportResponseToRFIMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override ZString MessageSubType => ExportSendMessageTypes.Codes.CRE;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.ControlFindingsInformationExport;
}
