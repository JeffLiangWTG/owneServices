using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	class TransferApplicationFromEHubNotificationProcessor(LoggingInformation logger) : TWCApplicationTypeMessageProcessor(logger)
	{
		protected override bool ProcessMessageMain(TWMessage message)
		{
			var successful = false;
			if (message.EM_LinkedObject is CusInBondHeader inBondHeader)
			{
				inBondHeader.BH_MessageStatus = message.EventType;
				successful = true;
			}
			message.EM_Status = successful ? TWMessage.Status.ProcessedOK : TWMessage.Status.Discarded;
			return successful;
		}

		public override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message)
		{
			CusInBondHeader linkedObject = null;
			MultilingualString discardReason = (NoResString)string.Empty;
			var entryType = message.EntryType;
			if (TWMessageStatusCodeList.IsValidCode(message.EventType) && entryType == CusEntryNumberTypes.Taiwan.Transhipment)
			{
				var entryNumber = message.EntryNumber;
				linkedObject = TWMessageHelper.LookForCusInBondHeader(message.Factory, entryNumber);
				if (linkedObject == null)
				{
					discardReason = GetUnableToFindTheLinkedJobMessage(message);
					Logger.LogWarning(ValidationConstants.MessageProcessor.TranshipmentEntryHeaderNotFound(message.EM_MessageNum, entryNumber, entryType));
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
