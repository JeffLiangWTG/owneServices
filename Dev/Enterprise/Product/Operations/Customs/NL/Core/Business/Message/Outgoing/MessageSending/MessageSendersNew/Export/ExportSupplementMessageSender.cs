using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ExportSupplementMessageSender : NLMessageSender
{
	public ExportSupplementMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override ZString MessageSubType => ExportSendMessageTypes.Codes.SUP;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.ExportDeclaration;
}
