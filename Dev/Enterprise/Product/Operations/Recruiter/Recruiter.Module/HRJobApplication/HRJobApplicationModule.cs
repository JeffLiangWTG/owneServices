using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Recruiter.Module
{
	public class HRJobApplicationModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public HRJobApplicationModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.HRJobApplication; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.HRJobApplication);
		}

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Recruiter; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.HRJobApplication; }
		}

		#endregion

		#region Workflow

		public override bool SupportsWorkflow => true;

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.HRJobApplicationWorkflowDescriptorCode; }
		}

		#endregion

		#region Menu / Toolbar

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			result.Add(new ZMenuItem("-"));
			result.Add(UpdateAccreditationsDocumentsMenuItem);

			return result.ToArray();
		}

		MenuItem UpdateAccreditationsDocumentsMenuItem
		{
			get { return new ZMenuItem(ResString.GetMultilingualString("F9810BB5-EF1A-4617-9C7A-584DA7CB160F", "Create Missing Application Documents"), delegate { UpdateApplicationDocuments(); }); }
		}

		ProgressForm documentsProgressForm;
		ApplicationDocumentsUpdater updater;
		void UpdateApplicationDocuments()
		{
			if (SelectedBusinessObjects.Length == 0)
			{
				Globals.Message.Show(ResString.GetMultilingualString("17032323-7F53-4498-9338-2B347C18DEF5", "Please select Applications to process"));
				return;
			}

			documentsProgressForm?.Hide();
			documentsProgressForm?.Dispose();
			documentsProgressForm = null;
			documentsProgressForm = new ProgressForm();
			documentsProgressForm.ShowCancelButton = true;
			documentsProgressForm.ShowProgressBar = true;
			documentsProgressForm.Cancelled += DocumentsProgressForm_Cancelled;
			documentsProgressForm.ShowModalTo(ParentModalForm);

			using var daxtraResumeParser = new DaxtraResumeParser();
			if (updater == null)
			{
				updater = new ApplicationDocumentsUpdater(daxtraResumeParser);
				updater.Progress += Updater_Progress;
				updater.Finished += Updater_Finished;
			}

			updater.ProcessBatch(SelectedBusinessObjects.Cast<HRJobApplication>().ToArray());
		}

		void Updater_Finished(object sender, ApplicationDocumentsUpdater.UpdateFinishedEventArgs e)
		{
			if (documentsProgressForm != null)
			{
				documentsProgressForm.Dispose();
				documentsProgressForm = null;

				string message = string.Empty;
				if (e.Errors.Any())
				{
					message = Res.GetString("C8AF4711-C451-447C-8C8F-7647E94FD1FD", "Process completed with errors.");
					message += System.Environment.NewLine;
					message += string.Join(System.Environment.NewLine, e.Errors);
				}
				else
				{
					message = Res.GetString("CA334A91-E4E1-4F2D-B295-B695D4940E06", "Process completed.");
				}

				message += System.Environment.NewLine;
				message += Res.GetString("4BBCD51E-3AAB-4758-AB6C-B52A0B44F6A1",
					"Processed applications: {0}\r\nBatches processed: {1}\r\nDocuments saved: {2}",
					e.Stats.ItemsTotal, e.Stats.BatchesProcessed, e.Stats.DocumentsSaved);

				Globals.Message.Show(message);
			}
		}

		void Updater_Progress(object sender, ApplicationDocumentsUpdater.ApplicationDocumentsProgressEventArgs e)
		{
			if (documentsProgressForm != null)
			{
				documentsProgressForm.Status =
					Res.GetString("A05D821F-16CB-42AC-9F13-7DC6F188B5EC", "Processing applications {0} of {1}. Processing batches {2} of {3}. Batches sent {4}.",
					e.Stats.ItemsProcessed, e.Stats.ItemsTotal,
					e.Stats.BatchesProcessed, e.Stats.BatchesTotal,
					e.Stats.BatchesSent);

				var totalProcessed = (decimal)(e.Stats.ItemsProcessed + e.Stats.BatchesProcessed);
				var totalActions = (decimal)(e.Stats.ItemsTotal + e.Stats.BatchesTotal);

				documentsProgressForm.PercentComplete = (int)((totalProcessed / totalActions) * 100m);
			}
		}

		void DocumentsProgressForm_Cancelled(object sender, EventArgs e)
		{
			updater.Cancel();
		}

		#endregion

		protected override IFilterControl GetNewFilterControl()
		{
			return new HRJobApplicationFilterControl(GridCollection, (HRJobApplicationFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new HRJobApplicationCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new HRJobApplicationFilterBusinessObject();
		}

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new HRJobApplicationOperationalActionSupporter();
	}
}
