using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ExportFallbackMessageSender : NLMessageSender
{
	public ExportFallbackMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override void PostSendProcess(NLEDIMessage newMessage)
	{
		base.PostSendProcess(newMessage);
		newMessage.EM_HeldUntilDate = ZDateTime.MaxSmallDateTime;
	}

	protected override void SetMessageTextAndInterpretation(NLEDIMessage message)
	{
		entryHeader.FallbackEntryNumberIssueDate = ZDateTime.Now;
		base.SetMessageTextAndInterpretation(message);
	}

	protected override ZString MessageSubType => ExportSendMessageTypes.Codes.FBK;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.ExportDeclaration;
}

