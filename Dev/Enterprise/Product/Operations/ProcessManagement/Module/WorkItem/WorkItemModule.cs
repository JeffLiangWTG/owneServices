using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business;
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
	public class WorkItemModule : ZFilterGridModule, IOperationalActionSupportable, IImportCollectionInfoProvider
	{
		#region Standard Module Overrides

		public WorkItemModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WorkItem; }
		}

		public override BusinessContext[] BusinessContexts
		{
			get { return new BusinessContext[] { BusinessContext.WorkItem }; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WorkItem);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new WorkItemFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new WorkItemFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WorkItemCollection(Factory);
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return JobInvoicingConsumerTypes.WorkItem.Code; }
		}

		#endregion

		#region IOperationalActionSupportable

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new WorkItemActionSupporter(); }
		}

		#endregion

		#region Implementation of IImportCollectionInfoProvider

		WorkItemFlattenedCollectionInfo workItemCollectionInfo;
		WorkItemFlattenedCollection workItemCollection;

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo
		{
			get
			{
				if (workItemCollectionInfo == null)
				{
					workItemCollection = workItemCollection ?? new WorkItemFlattenedCollection(Factory);
					workItemCollectionInfo = new WorkItemFlattenedCollectionInfo(workItemCollection);
				}
				return workItemCollectionInfo;
			}
		}

		public string ContextKey
		{
			get { return "3B50874D-A243-4D5F-9495-A7619041CAA7"; }
		}

		#endregion

		#region Implementation of ZFilterGridModule

		protected override void RunImportDataWizard()
		{
			workItemCollection = new WorkItemFlattenedCollection(Factory);
			var impl = new WorkItemFlattenedCollectionInfo(workItemCollection);
			var workItemFlattenedProcessor = new WorkItemFlattenedDataTransferProcessor(impl);
			var workItemSaveProcessor = new DataSaveProcessor(Factory,
				inProgressMessage: Res.GetString("AC139B4F-8113-4D71-87A9-DA4579BCD278", "Saving work items. This may take a long time depending on the amount being imported."),
				completedMessage: Res.GetString("0F275D6A-F43D-480E-B32A-40C0742C789C", "Work items saved."));

			RunImportWizardForm(impl, workItemFlattenedProcessor, workItemSaveProcessor);
		}

		void RunImportWizardForm(WorkItemFlattenedCollectionInfo impl, WorkItemFlattenedDataTransferProcessor workItemFlattenedProcessor, DataSaveProcessor workItemSaveProcessor)
		{
			bool isCancelled = false;
			var form = new MultistepDataImportWizardForm(impl, ContextKey, new DataTransferProcessor[] { workItemFlattenedProcessor, workItemSaveProcessor }, this.ID.Description.ToString());
			form.Cancelled += (s, e) => { workItemFlattenedProcessor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayImportDataWizardResult(workItemFlattenedProcessor, isCancelled); };
			form.Show();
		}
		protected static void DisplayImportDataWizardResult(WorkItemFlattenedDataTransferProcessor workItemFlattenedProcessor, bool isCancelled)
		{
			string messages;
			string caption;

			if (isCancelled)
			{
				Globals.Message.ShowInformation(Res.GetString("EC3CC8D0-4601-4C26-B38C-0C252E115D14", "No work item was created."), Res.GetString("076C588E-0628-4D75-98FC-FF7124328388", "Import Canceled"));
			}
			else
			{
				messages = Res.GetString("2480BBDE-57A9-44DB-9F2F-9DD309A26D42", "Work items to import = {0}", workItemFlattenedProcessor.ItemsToImport) + "\r\n";
				messages += string.Join(System.Environment.NewLine, workItemFlattenedProcessor.Logs);
				messages += "\r\n" + Res.GetString("EBB1528B-0A81-4E38-9207-647577168FDA", "TOTAL: Work items created = {0}, work items excluded = {1}", workItemFlattenedProcessor.NewCount, workItemFlattenedProcessor.ErrorCount) + "\r\n";
				caption = Res.GetString("298DB0DA-38C9-4D53-AE1B-FD1C14FC18A7", "Import Completed");

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
			get { return Env.Security.WorkItem; }
		}

		#endregion
	}
}
