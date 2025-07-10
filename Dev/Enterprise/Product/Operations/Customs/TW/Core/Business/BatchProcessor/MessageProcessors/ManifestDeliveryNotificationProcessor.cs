using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	class ManifestDeliveryNotificationProcessor(LoggingInformation logger) : TWCApplicationTypeMessageProcessor(logger)
	{
		protected override bool ProcessMessageMain(TWMessage message)
		{
			var successful = false;
			var eventType = ZString.Empty;
			if (message.EM_LinkedObject is CusEntryNumber entryNumber)
			{
				var manifestBills = TWMessageHelper.LookForAsycudaManifestBills(message.Factory, entryNumber.CE_EntryNum);
				if (manifestBills.Length != 0)
				{
					eventType = message.EventType;
					manifestBills.ForEach(b => b.ABL_MessageStatus = eventType);
					var headers = manifestBills.Select(b => b.Header).Distinct().ToArray();
					if (headers.Length != 0)
					{
						message.EM_LinkedObject = headers[0];
						message.EM_MessageInterpretation = TWMessageHelper.NewIncomingHelper(message)?.ToHtml() ?? ZString.Empty;
						headers.ForEach(TWMessageHelper.UpdateAsycudaHeaderMessageStatusFromBills);
					}
					successful = true;
				}
			}
			message.EM_Status = successful ? eventType : TWMessage.Status.Discarded;
			return successful;
		}

		public override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message)
		{
			CusEntryNumber linkedObject = null;
			MultilingualString discardReason = (NoResString)string.Empty;
			var eventType = message.EventType;
			var entryType = message.EntryType;
			if (TWMessageStatusCodeList.IsValidCode(eventType) && entryType == MessageTypeList.Codes.FHM)
			{
				var entryNumber = message.EntryNumber;
				linkedObject = TWMessageHelper.LookForCusEntryNumber(message.Factory, entryNumber);
				if (linkedObject == null)
				{
					discardReason = GetUnableToFindTheLinkedJobMessage(message);
					Logger.LogWarning(Res.GetString("E7F0D1C0-F645-4AD1-8C95-FB3071196996", "Can not find the corresponding Manifest Bill for Message Number: {0}, Entry Number: {1}, Entry Type: {2}", message.EM_MessageNum, entryNumber, message.EntryType));
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
