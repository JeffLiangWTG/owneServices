using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.GUI;
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
	public class OrderModule : ZFilterGridModule, IOperationalActionSupportable, IImportCollectionInfoProvider
	{
		public OrderModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorCSVImportMenuItem(CommonDataTransferCaptions.FromCsvMenuText, new EventHandler(ImportFromCsvEventHandler));
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(ImportFromXmlEventHandler));
			//AddExportDataMenuItem("to Cartage", new EventHandler(ExportToCartage));
			AddImportDataMenuItem(Res.GetString("3a1ce7e7-0032-41a3-b249-8a4abfad8992", "From &IFS"), ImportIFSEventHandler);
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());
			ObjectFactory.Get<IDeniedPartyScreeningActionsProvider>(nameof(IDeniedPartyScreeningActionsProvider), this, result).AddJobsMenuItem();
			new DeniedPartyScreeningPresentationManager().CreateModuleMenusForJob(this, result);
			return result.ToArray();
		}

		#region Import

		protected void ImportFromCsvEventHandler(object sender, EventArgs e)
		{
			using (DataImporterForm form = DataImporterForm.Create(BillingInterfaceName.WarehouseOrderCsvImport)) // Interface name for billing purposes
			{
				form.Importer = new WhsOrderFlatFileDataImporter();
				ZFormModaliser.ShowDialogWithoutDispose(form);
			}
		}

		protected void ImportFromXmlEventHandler(object sender, EventArgs e)
		{
			DataTransferDirector.PromptUserAndImport(BillingInterfaceName.WarehouseOrderLegacyXmlImport); // Interface name for billing purposes
		}

		#region IFSImport

		protected void ImportIFSEventHandler(object sender, EventArgs e)
		{
			DataTransferDirectorIFS.PromptUserAndImport(BillingInterfaceName.WarehouseOrderIFSImport); // Interface name for billing purposes
		}

		XmlDataTransferDirector transferDirectorIFS;

#if DEBUG
		public
#endif
		 XmlDataTransferDirector DataTransferDirectorIFS
		{
			get
			{
				if (transferDirectorIFS == null)
				{
					transferDirectorIFS = new XmlDataTransferDirector(new WhsOrderCartageValueObjectDataAdapterIFS(), true);
					transferDirectorIFS.Serializer = new XmlValueObjectSerializerIFS();
				}
				return transferDirectorIFS;
			}
#if DEBUG
			set { transferDirectorIFS = value; }
#endif
		}

		#endregion

		#endregion

		//#region Export

		//void ExportToCartage(object sender, EventArgs e)
		//{
		//    WhsOrderXmlExportToFileDirectorIFS director = new WhsOrderXmlExportToFileDirectorIFS(new WhsOrderConfirmationValueObjectDataAdapterIFS());
		//    WhsOrder[] selectedOrders = (WhsOrder[])GetSelectedBusinessObjects();

		//    if (director.CanExportOrders(selectedOrders))
		//    {
		//        foreach (WhsOrder selectedOrder in selectedOrders)
		//        {
		//            director.RunExport(selectedOrder);
		//        }
		//    }
		//}

		//bool CanExportOrders(WhsOrder[] orders)
		//{
		//    return true;
		//}

		//#endregion

		#region DataTransferDirector

#if DEBUG
		public
#endif
		XmlDataTransferDirector DataTransferDirector
		{
			get { return dataTransferDirector ?? (dataTransferDirector = new XmlDataTransferDirector(new WhsOrderValueObjectDataAdapter(), true)); }
#if DEBUG
			set { dataTransferDirector = value; }
#endif
		}
		XmlDataTransferDirector dataTransferDirector;

		#endregion

		public override ModuleIdentifier ID => ModuleIDs.WhsOrder;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.WhsOrderWorkflowDescriptorCode;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.WhsOrder };

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsOrder);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrderFilterControl(GridCollection, (OrderFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrderFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsOrderCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsOrder;

		public override bool AllowDelete => false;

		protected override IZForm ShowNewForm()
		{
			var form = base.ShowNewForm() as OrderEntryForm;

			if (form != null) // form will be null if security denied
			{
				WhsOrder docket = (WhsOrder)form.BusinessEntity;
				OrderFilterBusinessObject fBO = (OrderFilterBusinessObject)FilterBusinessObject;

				if (fBO.WD_OH_Client.IsValid)
				{
					docket.WD_OH_Client = fBO.WD_OH_Client;
				}

				if (fBO.WD_WW_Whs.IsValid)
				{
					docket.WD_WW_Whs = fBO.WD_WW_Whs;
				}

				if (fBO.ConsigneePK.IsValid)
				{
					docket.ConsigneePK = fBO.ConsigneePK;
				}
			}
			return form;
		}

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => new OrderOperationalActionSupporter();

		#endregion

		#region IImportCollectionInfoProvider Members

		protected override DataTransferProcessor GetDataImportWizardProcessor(IImportCollectionInfo collectionInfo)
		{
			return new WhsOrderFlattenedDataTransferProcessor((ImportCollectionInfoImplForWhsDocketFlattened)collectionInfo, new WhsOrderCollection(collectionInfo.Collection.Factory, new AdhocCollectionRelationship(typeof(WhsOrder))));
		}

		string IImportCollectionInfoProvider.ContextKey => "WhsOrderModuleImportWizard";

		IImportCollectionInfo IImportCollectionInfoProvider.ImportCollectionInfo => new ImportCollectionInfoImplForWhsDocketFlattened(new WhsDocketFlattenedCollection());

		#endregion
	}
}
