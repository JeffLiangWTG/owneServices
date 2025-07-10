using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public class SendEntrySummaryQueryOperationalActionRunner : USDeclarationOperationalActionRunner
	{
		public SendEntrySummaryQueryOperationalActionRunner(IOperationalActionSectionLog log)
			: base(log)
		{
		}
		protected override OperationalActionBulkMessageSender<JobDeclaration> GetMessageSenderCore(JobDeclaration declaration)
		{
			return new OperationalActionBulkENSQueryMessageSender(declaration);
		}

		protected override bool IsJobEligibleForSending(JobDeclaration declaration)
		{
			var declarationLink = declaration.GetDeclarationIdLink();

			if (!declaration.IsImport && !declaration.IsReconMessageType)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Entry Summary query cannot be sent for this job.", new object[] { declarationLink });
				return false;
			}

			if (declaration.IsImport && declaration.ActiveEntryHeaders.EntrySummaryEntry == null)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Entry Summary query cannot be sent. Entry summary entry does not exist for this job.", new object[] { declarationLink });
				return false;
			}

			if (declaration.IsReconMessageType && declaration.ReconDeclaration?.ReconEntry == null)
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Entry Summary query cannot be sent. Recon entry does not exists for this job.", new object[] { declarationLink });
				return false;
			}

			return true;
		}
	}
}
