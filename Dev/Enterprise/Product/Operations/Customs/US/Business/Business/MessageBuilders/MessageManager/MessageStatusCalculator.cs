using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business
{
	public enum ABIResponseStatus { Cleared, PartialCleared, Warnings, CensusWarning, Rejected, Undefined, PendingReview }

	public abstract class MessageStatusCalculator
	{
		protected MessageStatusCalculator(IMessageAttachee attachee)
		{
			this.attachee = attachee;
		}
		protected readonly IMessageAttachee attachee;

		public void CalculateStatus(MQEDIMessage lastMessage, ABIResponseStatus status)
		{
			ZString statusCalculated = "";

			if (lastMessage.IsTransmitMessage)
			{
				statusCalculated = GetAwaitingStatus(lastMessage.EM_MessageSubType);
			}
			else
			{
				switch (status)
				{
					case ABIResponseStatus.Rejected:
						statusCalculated = GetRejectedStatus(lastMessage.EM_MessageSubType);
						break;
					case ABIResponseStatus.PartialCleared:
						statusCalculated = GetPartialClearedStatus(lastMessage.EM_MessageSubType);
						break;
					case ABIResponseStatus.Warnings:
						statusCalculated = GetWarningStatus(lastMessage.EM_MessageSubType);
						break;
					case ABIResponseStatus.CensusWarning:
						statusCalculated = GetCensusWarningStatus(lastMessage.EM_MessageSubType);
						break;
					case ABIResponseStatus.Cleared:
						statusCalculated = GetClearedStatus(lastMessage.EM_MessageSubType);
						break;
					case ABIResponseStatus.PendingReview:
						statusCalculated = GetPendingReviewStatus(lastMessage.EM_MessageSubType);
						break;
					default:
						statusCalculated = GetUndefinedStatus(lastMessage);
						break;
				}
			}
			if (statusCalculated != "")
			{
				SetMessageStatus(lastMessage, statusCalculated);
			}
		}

		protected virtual ZString GetUndefinedStatus(MQEDIMessage lastMessage)
		{
			return ZString.Empty;
		}

		protected virtual ZString GetWarningStatus(ZString messageSubType)
		{
			return ZString.Empty;
		}

		protected virtual ZString GetCensusWarningStatus(ZString messageSubType)
		{
			return ZString.Empty;
		}

		protected abstract ZString GetClearedStatus(ZString messageSubType);
		protected abstract ZString GetPartialClearedStatus(ZString messageSubType);
		protected abstract ZString GetRejectedStatus(ZString messageSubType);
		protected abstract ZString GetAwaitingStatus(ZString messageSubType);

		protected virtual ZString GetPendingReviewStatus(ZString messageSubType)
		{
			return ZString.Empty;
		}

		protected virtual void SetMessageStatus(MQEDIMessage lastMessage, string statusCalculated)
		{
			attachee.MessageStatus = statusCalculated;
		}
	}
}
