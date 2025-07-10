using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.MessageManagers;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	class CustomsDeliveryNotificationProcessor(LoggingInformation logger) : TWCApplicationTypeMessageProcessor(logger)
	{
		protected override bool ProcessMessageMain(TWMessage message)
		{
			var successful = false;
			if (message.EM_LinkedObject is CusEntryHeader entryHeader)
			{
				message.EM_MessageInterpretation = TWMessageHelper.NewIncomingHelper(message)?.ToHtml() ?? ZString.Empty;
				UpdateEntryHeaderStatus(entryHeader, message);
				successful = true;
			}
			message.EM_Status = successful ? TWMessage.Status.ProcessedOK : TWMessage.Status.Discarded;
			return successful;
		}

		void UpdateEntryHeaderStatus(CusEntryHeader entry, TWMessage message)
		{
			var statusCalculator = new EDITWCStatusCalculator(message);
			var eventType = message.EventType;
			var status = ZString.Empty;
			if (!eventType.IsEmpty)
			{
				var warning = ZString.Empty;
				var interchangeNumber = message.InterchangeNumber;
				var lastSentOutgoingInterchangeNumber = entry.GetLastSentOutgoingInterchangeNumberByMessageType(message.EM_MessageType);
				if (interchangeNumber == lastSentOutgoingInterchangeNumber && statusCalculator.IsAwaitingReply(entry.CH_Status))
				{
					switch (eventType)
					{
						case EventTypeCodeList.Codes.Sent:
							status = statusCalculator.GetMessageAcknowledgedStatus();
							break;
						case EventTypeCodeList.Codes.Error:
							status = statusCalculator.GetMessageRejectedStatus();
							break;
						default:
							warning = Res.GetString("113B8DAA-4FBC-4C62-ADFE-C0324127BDCB", "Unknown Event Type({0}) for Message Number: {1}, Entry Number: {2}, Entry Type: {3}", eventType, message.EM_MessageNum, message.EntryNumber, message.EntryType);
							break;
					}
					if (!status.IsEmpty)
					{
						entry.CH_Status = status;
					}
				}
				else if (interchangeNumber != lastSentOutgoingInterchangeNumber)
				{
					warning = Res.GetString("BFE6F8DE-D8D2-4D9E-A08C-6C1BF8445517", "the previous Interchange number({0}) not is the latest/newest outbound Interchange number({1}) for Message Number: {2}, Entry Number: {3}, Entry Type: {4}", interchangeNumber, lastSentOutgoingInterchangeNumber, message.EM_MessageNum, message.EntryNumber, message.EntryType);
				}
				else
				{
					warning = Res.GetString("7C01B75F-797D-4627-94EA-05DF7787B15C", "the Entry Header is not Awaiting for Message Number: {0}, Entry Number: {1}, Entry Type: {2}", message.EM_MessageNum, message.EntryNumber, message.EntryType);
				}
				if (!warning.IsEmpty)
				{
					Logger.LogWarning(warning);
				}
			}
		}

		public override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message)
		{
			MultilingualString discardReason = (NoResString)string.Empty;
			var linkedObject = TWMessageHelper.LookForCusEntryHeader(message.Factory, message);
			if (linkedObject == null)
			{
				discardReason = GetUnableToFindTheLinkedJobMessage(message);
				LogNotFindEntryHeaderLogWarning(message);
			}
			return (message.EM_GB, linkedObject, discardReason);
		}
	}
}
