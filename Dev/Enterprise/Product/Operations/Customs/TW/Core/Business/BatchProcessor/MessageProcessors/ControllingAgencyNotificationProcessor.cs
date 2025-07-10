using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	class ControllingAgencyNotificationProcessor(LoggingInformation logger) : TWCApplicationTypeMessageProcessor(logger)
	{
		protected override bool ProcessMessageMain(TWMessage message)
		{
			var successful = false;
			if (message.EM_LinkedObject is CusTWControllingMessageHeader controllingMessageHeader)
			{
				message.EM_MessageInterpretation = TWMessageHelper.NewIncomingHelper(message, controllingMessageHeader)?.ToHtml() ?? ZString.Empty;
				controllingMessageHeader.TW1_MessageStatus = message.EventType;
				successful = true;
			}
			message.EM_Status = successful ? TWMessage.Status.ProcessedOK : TWMessage.Status.Discarded;
			return successful;
		}

		public override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message)
		{
			MultilingualString discardReason = (NoResString)string.Empty;
			CusTWControllingMessageHeader linkedObject = null;
			var eventType = message.EventType;
			var entryType = message.EntryType;
			if (TWMessageStatusCodeList.IsValidCode(eventType) && entryType == MessageTypeList.Codes.NXM)
			{
				var functionalReferenceID = message.FunctionalReferenceID;
				linkedObject = TWMessageHelper.LookForCusTWControllingMessageHeader(message.Factory, functionalReferenceID);
				if (linkedObject == null)
				{
					discardReason = GetUnableToFindTheLinkedJobMessage(message);
					Logger.LogWarning(ValidationConstants.MessageProcessor.ControllingMessageHeaderWithMessageTyepNotFound(message.EM_MessageNum, message.FunctionalReferenceID, message.EM_MessageType));
				}
			}
			else
			{
				Logger.LogWarning(ValidationConstants.MessageProcessor.EventTypeOrEntryTypeIsNotSupported(message.EM_MessageNum, message.EntryNumber, entryType));
			}
			return (message.EM_GB, linkedObject, discardReason);
		}
	}
}
