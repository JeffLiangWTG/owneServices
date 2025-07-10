using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class InvoicingModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public InvoicingModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
			InvoicingModuleInitialExtension();
		}

		partial void InvoicingModuleInitialExtension();

		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.WhsInvoicing;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsInvoicing);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new InvoicingFilterControl(GridCollection, (InvoicingFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new InvoicingFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsInvoiceCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsInvoicing;

		public override bool AllowDelete => true;

		#endregion

		#region Invoice All Action

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItemCollection = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItemCollection.Insert(menuItemCollection.Count - 1, new ZMenuItem("-"));
			menuItemCollection.Insert(menuItemCollection.Count - 1, new ZMenuItem(AutoCreateMenuItemText, delegate
			{
				var invoicePeriodicMultiClientInvoice = new WhsInvoicePeriodicMultiClientInvoice();
				invoicePeriodicMultiClientInvoice.LoadOrCreateInvoices();
				LastMultiClientForm = new InvoicingFormMultiClient(invoicePeriodicMultiClientInvoice);
				LastMultiClientForm.Show();
			}));
			return menuItemCollection.ToArray();
		}

		InvoicingFormMultiClient LastMultiClientForm;

		public override ToolBarButton[] ToolBarButtons
		{
			get
			{
				List<ToolBarButton> buttons = new List<ToolBarButton>(base.ToolBarButtons);
				foreach (ZToolBarButton button in buttons)
				{
					if (button.Text == AutoCreateMenuItemText)
					{
						button.ImageIndex = Icons.GetImageIndex(IconTypes.Forklift);
					}
				}

				return buttons.ToArray();
			}
		}

		static MultilingualString AutoCreateMenuItemText
		{
			get { return ResString.GetMultilingualString("7f3b1bca-45b5-4228-a679-e99800177feb", "Invoice All"); }
		}

		public OperationalActionSupporter OperationalActionSupporter => new InvoicingOperationalActionsSupporter();

		#endregion

		protected override IZForm ShowNewForm()
		{
			var form = (InvoicingForm)base.ShowNewForm();
			if (form != null) // form will be null if security denied
			{
				WhsInvoice invoice = (WhsInvoice)form.BusinessEntity;
				InvoicingFilterBusinessObject fBO = (InvoicingFilterBusinessObject)FilterBusinessObject;
				invoice.SetDefaultsForNew(fBO.ET_OH_Client, fBO.ET_WW);
			}
			return form;
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class InvoicingModule : ZFilterGridModule
	{
		public IZForm ShowNewFormForTest() => ShowNewForm();

		public InvoicingFormMultiClient GetLastMultiClientForm() => LastMultiClientForm;
	}
}

#endif
#endregion
