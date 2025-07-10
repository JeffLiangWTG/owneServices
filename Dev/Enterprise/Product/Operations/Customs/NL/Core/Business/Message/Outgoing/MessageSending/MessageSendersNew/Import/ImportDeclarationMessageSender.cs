using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ImportDeclarationMessageSender : NLMessageSender
{
	public ImportDeclarationMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override ZString MessageSubType => ImportSendMessageTypes.Codes.DEC;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.ImportDeclaration;
}

