using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.US.Business.OperationalAction
{
	public class OperationalActionBulkCargoManifestEntryStatusQueryMessageSender : DeclarationOperationalActionBulkMessageSender
	{
		public OperationalActionBulkCargoManifestEntryStatusQueryMessageSender(JobDeclaration dec, ZString action, ZString outputOption, ZBool updateEntryWithResults, ZBool requestForReleatedBOL)
			: base(dec)
		{
			this.action = action;
			this.outputOption = outputOption;
			this.updateEntryWithResults = updateEntryWithResults;
			this.requestForReleatedBOL = requestForReleatedBOL;
		}

		readonly ZString action;
		readonly ZString outputOption;
		readonly ZBool updateEntryWithResults;
		readonly ZBool requestForReleatedBOL;

		protected override string MessageTypeCore
		{
			get { return "Cargo/Manifest/Entry Status Query"; }
		}

		protected override SaveResult OperationalActionSendMessageCore(bool sendWithMessageErrors, IOperationalActionSectionLog log)
		{
			var result = SaveResult.Fail;
			using (new DisposableAction(() => job.Factory.SuspendValidation(), () => job.Factory.ResumeValidation()))
			{
				var header = new CargoManifestStatusQueryHeaderObject(job);
				var messageCount = header.PopulateAndSendQueryMessages(action, updateEntryWithResults, outputOption, requestForReleatedBOL);

				if (messageCount > 0)
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

			return result;
		}
	}
}
