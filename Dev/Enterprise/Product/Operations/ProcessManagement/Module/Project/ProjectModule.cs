using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.GUI;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.ProcessManagement.Module
{
	public class ProjectModule : ZFilterGridModule, IOperationalActionSupportable, IImportCollectionInfoProvider
	{
		public ProjectModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Project; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.Project }; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Project);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ProjectFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ProjectFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ProjectCollection(Factory);
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return JobInvoicingConsumerTypes.Project.Code; }
		}

		#region Actions Menu

		protected override MenuItem[] GetNewActionMenuItems()
		{
			return base.GetNewActionMenuItems().Append(GetImportFromJiraMenuItem()).ToArray();
		}

		MenuItem GetImportFromJiraMenuItem()
		{
			return new ZMenuItem(Res.GetString("6ac79304-89d8-41a6-8792-12e89402c565", "Import from Jira"), (sender, args) => ShowJiraImportForm());
		}

		protected virtual void ShowJiraImportForm()
		{
			JiraProjectSelectorForm.ShowForm();
		}

		#endregion

		#region IOperationalActionSupportable

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new ProjectActionSupporter(); }
		}

		#endregion

		#region Implementation of IImportCollectionInfoProvider

		ProjectFlattenedCollectionInfo projectCollectionInfo;
		ProjectFlattenedCollection projectCollection;

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo
		{
			get
			{
				if (projectCollectionInfo == null)
				{
					projectCollection = projectCollection ?? new ProjectFlattenedCollection(Factory);
					projectCollectionInfo = new ProjectFlattenedCollectionInfo(projectCollection);
				}
				return projectCollectionInfo;
			}
		}
		public string ContextKey
		{
			get { return "BC30B247-A1EF-4839-B7D2-4B5241B7FD88"; }
		}
		public bool LicenceCheck
		{
			get { return false; }
		}
		#endregion

		#region Implementation of ZFilterGridModule

		protected override void RunImportDataWizard()
		{
			projectCollection = new ProjectFlattenedCollection(Factory);
			var impl = new ProjectFlattenedCollectionInfo(projectCollection);
			var projectFlattenedProcessor = new ProjectFlattenedDataTransferProcessor(impl);
			var projectSaveProcessor = new DataSaveProcessor(Factory,
				inProgressMessage: Res.GetString("EC767505-4E77-4C77-8F2A-A76795D40938", "Saving projects. This may take a long time depending on the amount being imported."),
				completedMessage: Res.GetString("49ACB0C8-30A3-4F3E-A6D3-260E669FD157", "Projects saved."));

			RunImportWizardForm(impl, projectFlattenedProcessor, projectSaveProcessor);
		}

		void RunImportWizardForm(ProjectFlattenedCollectionInfo impl, ProjectFlattenedDataTransferProcessor projectFlattenedProcessor, DataSaveProcessor projectSaveProcessor)
		{
			bool isCancelled = false;
			var form = new MultistepDataImportWizardForm(impl, ContextKey, new DataTransferProcessor[] { projectFlattenedProcessor, projectSaveProcessor }, this.ID.Description.ToString());
			form.Cancelled += (s, e) => { projectFlattenedProcessor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayImportDataWizardResult(projectFlattenedProcessor, isCancelled); };
			form.Show();
		}
		protected static void DisplayImportDataWizardResult(ProjectFlattenedDataTransferProcessor projectFlattenedProcessor, bool isCancelled)
		{
			string messages;
			string caption;

			if (isCancelled)
			{
				Globals.Message.ShowInformation(Res.GetString("DAD5E22F-AAD1-45C2-B8D0-BD1F8A475237", "No project was created."), Res.GetString("D78E4BF7-212F-41D7-A205-73284796413B", "Import Canceled"));
			}
			else
			{
				messages = Res.GetString("404BBDE2-72E2-45AC-90F9-5C15DF622614", "Projects to import = {0}", projectFlattenedProcessor.ItemsToImport) + "\r\n";
				messages += string.Join(System.Environment.NewLine, projectFlattenedProcessor.Logs);
				messages += "\r\n" + Res.GetString("CF911032-E9BE-4E06-BD5D-4472F57169D8", "TOTAL: Projects created = {0}, projects excluded = {1}", projectFlattenedProcessor.NewCount, projectFlattenedProcessor.ErrorCount) + "\r\n";
				caption = Res.GetString("1688982D-F7B2-491E-9DBB-17ABD3A54FB5", "Import Completed");

				using (ZMessageBox notification = new ZMessageBox(messages, caption, MessageBoxButtons.OK, MessageBoxIcon.Information))
				{
					ZFormModaliser.ShowDialogAndDispose(notification);
				}
			}
		}

		#endregion

		#region Licence & Security

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.ProductivityTools; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Project; }
		}

		#endregion
	}
}
