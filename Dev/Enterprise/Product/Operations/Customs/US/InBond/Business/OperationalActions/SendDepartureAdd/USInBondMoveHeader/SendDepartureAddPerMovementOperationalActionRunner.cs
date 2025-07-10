using Enterprise.Customs.US.Business.OperationalAction;
using Enterprise.Services.OperationalActions.Support;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business.OperationalActions
{
	public class SendDepartureAddPerMovementOperationalActionRunner : USOperationalActionRunner<USInBondMoveHeader>
	{
		public SendDepartureAddPerMovementOperationalActionRunner(IOperationalActionSectionLog log)
			: base(log)
		{
		}

		protected override string TypeOfJob => "In-Bond Movement";

		protected override LogControllerLink GetLogControllerLink(USInBondMoveHeader movementHeader)
		{
			return movementHeader.GetInBondMovementIdLink();
		}

		protected override OperationalActionBulkMessageSender<USInBondMoveHeader> GetMessageSender(USInBondMoveHeader movementHeader)
		{
			return new OperationalActionBulkDepartureAddPerMovementMessageSender(movementHeader);
		}

		protected override bool IsJobEligibleForSending(USInBondMoveHeader movementHeader)
		{
			var result = true;
			var inBondMovementLink = movementHeader.GetInBondMovementIdLink();

			if (movementHeader.Header is CusInBondHeader header)
			{
				if (header.IsDocumentOnly)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "In-Bond Movement {0}: Departure Add message can not be sent in Document Only mode.", inBondMovementLink);
					result = false;
				}
				else if (header.IsPostDepartureMessageOnly)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "In-Bond Movement {0}: Departure Add message can not be sent when 'Post Departure Messages Only' is ticked.", inBondMovementLink);
					result = false;
				}
				else if (header.IsSendCustomsMessageMutexLocked)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "In-Bond Movement {0}: {1}", inBondMovementLink, header.CannotSendCustomsMessageWhenMutexIsLocked());
					result = false;
				}
			}

			return result;
		}

		protected override void UnlockMergeMutexIfNecessary(USInBondMoveHeader job)
		{
		}
	}
}
