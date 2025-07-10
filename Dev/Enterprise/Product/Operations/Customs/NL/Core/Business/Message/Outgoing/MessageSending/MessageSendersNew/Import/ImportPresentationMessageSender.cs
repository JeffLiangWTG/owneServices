using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ImportPresentationMessageSender : NLMessageSender
{
	public ImportPresentationMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override ZString MessageSubType => ImportSendMessageTypes.Codes.PRE;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.ImportPresentation;
}
