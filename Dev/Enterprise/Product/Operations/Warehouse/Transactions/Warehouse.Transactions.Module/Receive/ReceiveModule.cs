using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class ReceiveModule : ZFilterGridModule, IOperationalActionSupportable, IImportCollectionInfoProvider
	{
		public ReceiveModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorCSVImportMenuItem(Res.GetString("163b6824-4945-4535-b851-c2f1d4095c10", "From &Excel (.csv)"), new EventHandler(ImportFromCSV));
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(ImportFromXmlEventHandler));
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			ObjectFactory.Get<IDeniedPartyScreeningActionsProvider>(nameof(IDeniedPartyScreeningActionsProvider), this, result).AddJobsMenuItem();
			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForJob(this, result);
			return result.ToArray();
		}

		#region Import from XML

		protected void ImportFromXmlEventHandler(object sender, EventArgs e)
		{
			DataTransferDirector.PromptUserAndImport(BillingInterfaceName.WarehouseReceiveXMLImport); // Interface name for billing purposes
		}

		XmlDataTransferDirector DataTransferDirector => dataTransferDirector ?? (dataTransferDirector = new XmlDataTransferDirector(new WhsReceiveValueObjectDataAdapter(), true));
		XmlDataTransferDirector dataTransferDirector;

		#endregion

		#region Import from CSV

		public void ImportFromCSV(object sender, EventArgs e)
		{
			using (var form = new ImportReceiveFromCSVForm())
			{
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		#endregion

		#region ID

		public override ModuleIdentifier ID => ModuleIDs.WhsReceive;

		#endregion

		#region SupportsWorkflow

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.WhsReceiveWorkflowDescriptorCode;

		#endregion

		#region LicenceCheckPoint

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		#endregion

		#region SecurityCheckpoint

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsReceive;

		#endregion

		#region Controller, FilterControl, Grid, etc.

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsReceive);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new ReceiveFilterControl(GridCollection, (ReceiveFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new ReceiveFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsReceiveCollection(Factory);
		}

		#endregion

		#region New Form

		protected override IZForm ShowNewForm()
		{
			var form = (ReceiveEntryForm)base.ShowNewForm();
			if (form != null) // form will be null if security denied
			{
				SetFilterBizODefaults((WhsReceive)form.BusinessEntity);
			}

			return form;
		}

		void SetFilterBizODefaults(WhsReceive receive)
		{
			var filterBizObj = (ReceiveFilterBusinessObject)FilterBusinessObject;

			if (filterBizObj.WD_OH_Client.IsValid)
			{
				receive.WD_OH_Client = filterBizObj.WD_OH_Client;
			}

			if (filterBizObj.WD_WW_Whs.IsValid)
			{
				receive.WD_WW_Whs = filterBizObj.WD_WW_Whs;
			}

			if (filterBizObj.TransportCoPK.IsValid)
			{
				receive.TransportCoPK = filterBizObj.TransportCoPK;
			}
		}

		#endregion

		#region AllowDelete

		public override bool AllowDelete => false;

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new ReceiveOperationalActionSupporter();

		#endregion

		#region IImportCollectionInfoProvider Members

		protected override DataTransferProcessor GetDataImportWizardProcessor(IImportCollectionInfo collectionInfo)
		{
			return new WhsReceiveFlattenedDataTransferProcessor((ImportCollectionInfoImplForWhsDocketFlattened)collectionInfo, new WhsReceiveCollection(collectionInfo.Collection.Factory, new AdhocCollectionRelationship(typeof(WhsReceive))));
		}

		string IImportCollectionInfoProvider.ContextKey => "WhsReceiveModuleImportWizard";

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo => new ImportCollectionInfoImplForWhsDocketFlattened(new WhsDocketFlattenedCollection());

		#endregion
	}
}
