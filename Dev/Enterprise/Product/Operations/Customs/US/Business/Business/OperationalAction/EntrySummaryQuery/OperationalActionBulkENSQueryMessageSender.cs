using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public class OperationalActionBulkENSQueryMessageSender : DeclarationOperationalActionBulkMessageSender
	{
		public OperationalActionBulkENSQueryMessageSender(JobDeclaration job)
			: base(job)
		{
		}

		protected override string MessageTypeCore => "Entry Summary Query";

		protected override SaveResult OperationalActionSendMessageCore(bool sendWithMessageErrors, IOperationalActionSectionLog log)
		{
			var result = SaveResult.Fail;
			using (new DisposableAction(() => job.Factory.SuspendValidation(), () => job.Factory.ResumeValidation()))
			{
				CusEntryHeader entryHeader = null;

				if (job.IsImport)
				{
					entryHeader = job.ActiveEntryHeaders.EntrySummaryEntry;
				}
				else if (job.IsReconMessageType)
				{
					entryHeader = job.ReconDeclaration.ReconEntry.GetEntry();
				}

				if (entryHeader != null)
				{
					var builder = new ACEEntrySummaryQueryMessageBuilder(entryHeader);
					var message = builder.PopulateMessage();
					if (message != null)
					{
						try
						{
							job.Factory.Save();
							result = SaveResult.Success;
						}
						catch (ZSaveConcurrencyException)
						{
							result = SaveResult.FailWithConcurrencyError;
						}
						catch (ZSaveException ex)
						{
							result = SaveResult.Fail;
							log.NotifyFormat(OperationalActionLogErrorLevel.Warning, "Job {0}: Cannot save job. An unexpected error occurred - {1}", new object[] { JobLink, ex.FriendlyMessage });
						}
					}
				}
			}

			return result;
		}
	}
}
