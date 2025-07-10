using System;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class ExportCancellationMessageSender : NLMessageSender
{
	public ExportCancellationMessageSender(JobDeclarationMessageSendingObject messageSendingObject) : base(messageSendingObject)
	{
	}

	protected override void PostSendProcess(NLEDIMessage newMessage)
	{
		base.PostSendProcess(newMessage);
		if (!messageSendingObject.ReasonForInvalidation.IsEmpty)
		{
			newMessage.CustomsMessageRemarks = $"{NLConstants.StatementTypes.Customs}|{messageSendingObject.ReasonForInvalidation}";
		}
	}

	protected override Type GetEdiMessageType() => typeof(CancelDeclarationEDIMessage);

	protected override ZString MessageSubType => ExportSendMessageTypes.Codes.CAN;

	protected override ZString WcoType => NLConstants.WCoTypeCodes.ExportInvalidationRequest;
}

