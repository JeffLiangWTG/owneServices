using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	class LicensingMessageProcessor(LoggingInformation logger) : TWCApplicationTypeMessageProcessor(logger)
	{
		protected override bool ProcessMessageMain(TWMessage message)
		{
			var successful = false;
			if (message.EM_LinkedObject is CusTWControllingMessageHeader controllingMessageHeader)
			{
				message.EM_MessageInterpretation = TWMessageHelper.NewIncomingHelper(message, controllingMessageHeader)?.ToHtml() ?? ZString.Empty;
				var licensingStatus = message.StatusNameCode;
				if (!licensingStatus.IsEmpty)
				{
					controllingMessageHeader.TW1_EntryStatus = licensingStatus;
				}
				controllingMessageHeader.TW1_MessageStatus = TWMessageStatusCodeList.Codes.Acknowledged;
				successful = true;
			}
			message.EM_Status = successful ? TWMessage.Status.ProcessedOK : TWMessage.Status.Discarded;
			return successful;
		}

		public override (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message)
		{
			MultilingualString discardReason = (NoResString)string.Empty;
			var linkedObject = TWMessageHelper.LookForCusTWControllingMessageHeader(message.Factory, message.FunctionalReferenceID, message.EntryNumber);
			if (linkedObject == null)
			{
				discardReason = GetUnableToFindTheLinkedJobMessage(message);
				Logger.LogWarning(ValidationConstants.MessageProcessor.ControllingMessageHeaderNotFound(message.EM_MessageNum, message.EntryNumber, message.FunctionalReferenceID));
			}
			return (message.EM_GB, linkedObject, discardReason);
		}
	}
}
