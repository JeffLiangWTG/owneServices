using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.GUI.Orders.DataTransfer;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.Freight.Integration;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Orders.Module
{
	public class OrdersModule : ZFilterGridModule, IOrdersModule, IOperationalActionSupportable
	{
		public OrdersModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override void AddInterfaceConnectorMenuItems()
		{
			AddInterfaceConnectorImportMenuItem(CommonDataTransferCaptions.FromXmlMenuText, new EventHandler(OnImportFromXml_Click));
			AddInterfaceConnectorCSVImportMenuItem(CommonDataTransferCaptions.FromCsvMenuText, new EventHandler(OnImportFromCsv_Click));
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewStandardMenuItems());
			result.Add(new ZMenuItem(ResString.GetMultilingualString("MenuItem.BulkUpdate", "Bulk Update"), OnBulkUpdate_Click));
			return result.ToArray();
		}

		public override ModuleIdentifier ID => ModuleIDs.Orders;

		public override bool SupportsWorkflow => true;

		public override string WorkflowType => WorkflowDescriptors.OrderWorkflowDescriptorCode;

		public override BusinessContext[] BusinessContexts => new BusinessContext[] { BusinessContext.Order };

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.OrderManager;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.OrderTracking;

		#region IOrdersModule

		public IZForm ShowFormForSplit(Order orderToSplit, CreateOrderType splitType)
		{
			return ((OrdersController)GetNewController(null)).ShowSplitForm(orderToSplit, splitType);
		}

		#endregion

		#region Implementation

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Orders);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrdersFilterControl((OrderCollection)GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			SetupFactory(Factory);
			return new OrderCollection(Factory);
		}

		protected override BusinessObjectFactory GetNewFactory()
		{
			var factory = base.GetNewFactory();
			SetupFactory(factory);
			return factory;
		}

		void SetupFactory(BusinessObjectFactory factory)
		{
			if (factory != null)
			{
				ChildEditableService.SetState(factory, ChildEditableServiceStates.Order);
			}
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrdersFilterBusinessObject();
		}

		void OnBulkUpdate_Click(object sender, EventArgs e)
		{
			new OrderDetailsBulkUpdateForm(new OrderDetailsBulkUpdateBusinessObject(new BusinessObjectFactory())).Show();
		}

		void OnImportFromXml_Click(object sender, EventArgs e)
		{
			new OrderXMLDataTransferDirector(new OrderValueObjectDataAdapter(), true).PromptUserAndImport(BillingInterfaceName.OrdersXmlImport); // Interface name for billing purposes
		}

		void OnImportFromCsv_Click(object sender, EventArgs e)
		{
			CsvOrderDataImporter importer = new CsvOrderDataImporter();
			using (CsvOrderDataImporterForm orderDataImporterForm = CsvOrderDataImporterForm.Create(BillingInterfaceName.OrdersCsvImport)) // Interface name for billing purposes
			{
				orderDataImporterForm.Importer = importer;
				ZFormModaliser.ShowDialogWithoutDispose(orderDataImporterForm);
			}
		}

		#endregion

		#region IOperationalActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter => new OrderOperationalActionSupporter();

		#endregion
	}
}
