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
	public class SendDepartureAddOperationalActionRunner : USOperationalActionRunner<CusInBondHeader>
	{
		public SendDepartureAddOperationalActionRunner(IOperationalActionSectionLog log)
			: base(log)
		{
		}

		protected override string TypeOfJob => "In-Bond";

		protected override LogControllerLink GetLogControllerLink(CusInBondHeader job)
		{
			return job.GetInBondHeaderIdLink();
		}

		protected override OperationalActionBulkMessageSender<CusInBondHeader> GetMessageSender(CusInBondHeader header) => new OperationalActionBulkDepartureAddMessageSender(header);

		protected override bool IsJobEligibleForSending(CusInBondHeader header)
		{
			var result = true;
			var inBondLink = header.GetInBondHeaderIdLink();
			if (header.IsDocumentOnly)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Departure Add message can not be sent in Document Only mode.", inBondLink);
				result = false;
			}
			else if (header.IsPostDepartureMessageOnly)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Departure Add message can not be sent when 'Post Departure Messages Only' is ticked.", inBondLink);
				result = false;
			}
			else if (header.IsSendCustomsMessageMutexLocked)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: {1}", inBondLink, header.CannotSendCustomsMessageWhenMutexIsLocked());
				result = false;
			}

			return result;
		}

		protected override void UnlockMergeMutexIfNecessary(CusInBondHeader job)
		{
		}
	}
}
