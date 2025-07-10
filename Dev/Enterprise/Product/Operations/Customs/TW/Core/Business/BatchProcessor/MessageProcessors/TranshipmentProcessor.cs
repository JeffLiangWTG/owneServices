using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	class TranshipmentProcessor(LoggingInformation logger) : TWCApplicationTypeMessageProcessor(logger)
	{
		protected override bool ProcessMessageMain(TWMessage message)
		{
			var successful = false;
			if (message.EM_LinkedObject is CusInBondHeader inBondHeader)
			{
				message.EM_MessageInterpretation = TWMessageHelper.NewIncomingHelper(message)?.ToHtml() ?? ZString.Empty;
				UpdateCusInBondHeader(message, inBondHeader);
				successful = true;
			}
			message.EM_Status = successful ? TWMessage.Status.ProcessedOK : TWMessage.Status.Discarded;
			return successful;
		}

		void UpdateCusInBondHeader(TWMessage message, CusInBondHeader cusInBondHeader)
		{
			var releaseStatus = message.ReleaseStatus;
			if (!releaseStatus.IsEmpty)
			{
				cusInBondHeader.BH_ReleaseStatus = releaseStatus;
			}
			cusInBondHeader.BH_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged;
		}

		public override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message)
		{
			MultilingualString discardReason = (NoResString)string.Empty;
			var linkedObject = TWMessageHelper.LookForCusInBondHeader(message.Factory, message.EntryNumber);
			if (linkedObject == null)
			{
				discardReason = GetUnableToFindTheLinkedJobMessage(message);
				Logger.LogWarning(ValidationConstants.MessageProcessor.TranshipmentEntryHeaderNotFound(message.EM_MessageNum, message.EntryNumber, message.EntryType));
			}
			return (message.EM_GB, linkedObject, discardReason);
		}
	}
}
