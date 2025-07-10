using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public class SendEntrySummaryOperationalActionRunner : USDeclarationOperationalActionRunner
	{
		public SendEntrySummaryOperationalActionRunner(IOperationalActionSectionLog log, ZString contactName, ZString contactPhone)
			: base(log)
		{
			this.contactName = contactName;
			this.contactPhone = contactPhone;
		}

		readonly ZString contactName;
		readonly ZString contactPhone;

		protected override OperationalActionBulkMessageSender<JobDeclaration> GetMessageSenderCore(JobDeclaration declaration)
		{
			return new OperationalActionBulkENSMessageSender(declaration, contactName, contactPhone);
		}

		protected override bool IsJobEligibleForSending(JobDeclaration declaration)
		{
			var result = declaration.IsENSFormalImport && !declaration.US_EntryType.IsEmpty;
			var declarationLink = declaration.GetDeclarationIdLink();
			if (result)
			{
				if (declaration.US_PSC)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Entry Summary not submitted because Post Summary Correction is enabled.", new object[] { declarationLink });
					result = false;
				}

				if (declaration.IsACSCargoCertificationMode)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Entry Summary for ACS is no longer supported by CBP.", new object[] { declarationLink });
					result = false;
				}

				var entry = declaration.ActiveEntryHeaders.EntrySummaryEntry;
				if (entry != null && entry.IsWaitingForResponse)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Entry Summary has already been submitted.", new object[] { declarationLink });
					result = false;
				}
			}
			else
			{
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, @"Job {0}: Entry Summary will only be submitted for Import jobs where Enable 7501 is checked and entry type is entered.", new object[] { declarationLink });
			}
			return result;
		}
	}
}
