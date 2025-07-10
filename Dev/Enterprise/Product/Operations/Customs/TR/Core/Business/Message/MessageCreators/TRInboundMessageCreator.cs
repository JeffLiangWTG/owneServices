using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.TR.Business
{
	public class TRInboundMessageCreator : IInboundMessageCreator
	{
		public void CreateMessagesForInterchange(EDIInterchange interchange)
		{
			var bodyText = interchange.EI_BodyText;
			var headerText = interchange.EI_HeaderText;
			if (bodyText.IsEmpty || headerText.IsEmpty)
			{
				interchange.EI_Status = EDIInterchange.Status.Error;
				interchange.Logs.AddNew(Events.ErrorReport, "NO TR CUSTOMS DATA");
			}
			else
			{
				var newEDIMessage = interchange.ContainedMessages.AddNew();
				newEDIMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
				newEDIMessage.EM_MessageNum = interchange.EI_InterchangeNum.Right(EDIMessage.Schema.EM_MessageNumMaxLength);
				newEDIMessage.EM_Status = EDIMessage.Status.Queued;
				newEDIMessage.EM_MessageType = interchange.EI_InterchangeType;

				if (interchange.EI_InterchangeType == TRMessageTypes.Codes.XER)
				{
					newEDIMessage.EM_MessageText = headerText;
					newEDIMessage.EM_MessageInterpretation = newEDIMessage.EM_MessageText;
				}
				else
				{
					newEDIMessage.EM_MessageText = bodyText;
					newEDIMessage.EM_MessageInterpretation = newEDIMessage.EM_MessageText;
				}

				interchange.ContainedMessages.Add(newEDIMessage);
			}
		}
	}
}
