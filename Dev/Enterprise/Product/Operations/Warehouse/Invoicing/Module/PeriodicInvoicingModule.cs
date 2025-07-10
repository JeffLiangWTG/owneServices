using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.Warehouse.Invoicing.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Invoicing.Module
{
	public abstract partial class PeriodicInvoicingModule : ZFilterGridModule
	{
		public PeriodicInvoicingModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
			InvoicingModuleInitialExtension();
		}

		partial void InvoicingModuleInitialExtension();

		#region Standard Module Overrides

		protected override IFilterControl GetNewFilterControl()
		{
			return new PeriodicInvoicingFilterControl(GridCollection, (PeriodicInvoicingFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return CreatePeriodicInvoicingCollection();
		}

		protected abstract PeriodicInvoicingCollection CreatePeriodicInvoicingCollection();

		public override bool AllowDelete => true;

		#endregion

		#region Invoice All Action

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menuItemCollection = new List<MenuItem>(base.GetNewActionMenuItems());
			menuItemCollection.Insert(menuItemCollection.Count - 1, new ZMenuItem("-"));
			menuItemCollection.Insert(menuItemCollection.Count - 1, new ZMenuItem(AutoCreateMenuItemText, delegate
			{
				var invoicePeriodicMultiClientInvoice = CreatePeriodicInvoicingMultiClientInvoice();
				invoicePeriodicMultiClientInvoice.LoadOrCreateInvoices();
				LastMultiClientForm = new PeriodicInvoicingMultiClientForm(invoicePeriodicMultiClientInvoice);
				LastMultiClientForm.Show();
			}));
			return menuItemCollection.ToArray();
		}

		protected abstract PeriodicInvoicingMultiClientInvoice CreatePeriodicInvoicingMultiClientInvoice();

		PeriodicInvoicingMultiClientForm LastMultiClientForm;

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
			get { return ResString.GetMultilingualString("3f37e0cc-53ae-463a-bcc2-e15101fca0ec", "Invoice All"); }
		}

		#endregion

		protected override IZForm ShowNewForm()
		{
			var form = (PeriodicInvoicingForm)base.ShowNewForm();
			if (form != null) // form will be null if security denied
			{
				PeriodicInvoicing invoice = (PeriodicInvoicing)form.BusinessEntity;
				var fBO = (PeriodicInvoicingFilterBusinessObject)FilterBusinessObject;
				invoice.SetDefaultsForNew(fBO.ET_OH_Client, fBO.ET_WW);
			}
			return form;
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Invoicing.Module
{
	public partial class PeriodicInvoicingModule : ZFilterGridModule
	{
		public IZForm ShowNewFormForTest() => ShowNewForm();

		public PeriodicInvoicingMultiClientForm GetLastMultiClientForm() => LastMultiClientForm;
	}
}

#endif
#endregion
