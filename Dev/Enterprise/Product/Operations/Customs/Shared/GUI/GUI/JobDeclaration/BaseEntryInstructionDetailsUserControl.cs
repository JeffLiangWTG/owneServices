using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class BaseEntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
	{
		public BaseEntryInstructionDetailsUserControl()
		{
			InitializeComponent();

			var synchronizeWithBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("7C1A6385-5E9A-4F53-A2D0-D884B3A5A7DB", "&Synchronize with Inventory"));
			synchronizeWithBondedWarehouseMenuItem.Click += SynchronizeWithBondedWarehouseMenuItem_Click;

			var inventoriesSelectionFromBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("033B3659-B316-43D8-BD95-FF100FB571DA", "S&elect Inventory"));
			inventoriesSelectionFromBondedWarehouseMenuItem.Click += InventoriesSelectionFromBondedWarehouseMenuItem_Click;

			bondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("803CA142-8907-4771-9762-0B3E0D8A92A9", "Inventory Management"), new MenuItem[] { synchronizeWithBondedWarehouseMenuItem, inventoriesSelectionFromBondedWarehouseMenuItem });
			EntryInstructionsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			EntryInstructionsGrid.ContextMenu.MenuItems.Add(bondedWarehouseMenuItem);
			EntryInstructionsGrid.ContextMenu.Popup += ContextMenu_Popup;

			NewOwnerOrganisationControl.AllowOverlap(BondHolderOrganisationControl);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			DetailsUserControl.UserControlType = GetDetailsUserControlType();
			DetailsGroupBox.Visible = DetailsUserControl.UserControlType == null;
			DetailsUserControl.Visible = DetailsUserControl.UserControlType != null;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing && EntryInstructionsGrid.ContextMenu != null)
			{
				EntryInstructionsGrid.ContextMenu.Popup -= ContextMenu_Popup;
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			bondedWarehouseMenuItem.Visible = CurrentEntryInstruction is CusEntryInstruction entryInstruction && entryInstruction.IsInventorySelectionEnabled && (entryInstruction.JobDeclaration?.SupportMultipleWarehouseEntry ?? false);
		}

		readonly ZMenuItem bondedWarehouseMenuItem;

		void InventoriesSelectionFromBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			var currentEntryInstruction = CurrentEntryInstruction;
			if (currentEntryInstruction != null)
			{
				using (var form = new InventorySelectionForm(new DeclarationEntryInstructionInventorySelectionHeader(currentEntryInstruction)))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						var parentForm = GetParentForm();
						if (parentForm is ShipmentForm shipmentForm)
						{
							shipmentForm.PlugIns.SelectPlugInTabPage(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
							var plugin = (BaseBrokeragePlugIn)shipmentForm.PlugIns.GetPlugIn(ZArchitecture.Modules.ControllerIDs.Customs.JobDeclaration);
							plugin.OnGUIShown();
							var userControl = (BaseCustomsBrokerageUserControl)plugin.UserControl;
							userControl.MainTabControl.SelectedTab = userControl.InvoiceLinesTabPage;
						}
						else
						{
							if (parentForm is BaseJobDeclarationForm declarationForm)
							{
								var customsBrokerageUserControl = declarationForm.CustomsBrokerageUserControl;
								customsBrokerageUserControl.MainTabControl.SelectedTab = customsBrokerageUserControl.InvoiceLinesTabPage;
							}
						}
					}
				}
			}
		}

		Form GetParentForm()
		{
			Control control = this;
			while (control != null)
			{
				control = control.Parent;
				if (control is Form form)
				{
					return form;
				}
			}
			return null;
		}

		void SynchronizeWithBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			var currentEntryInstruction = CurrentEntryInstruction;
			if (currentEntryInstruction != null)
			{
				if (!currentEntryInstruction.InvoiceLines.Any())
				{
					Globals.Message.ShowInformation(Res.GetString("A3B23C79-0D67-408B-AFA7-D5AD71DFB39D", "At least one invoice line is required for this entry instruction in order for Synchronization to work."));
				}
				else
				{
					var errorMessage = currentEntryInstruction.UpdateOutwardLinesWithInventoryDetails();
					if (!errorMessage.IsEmpty)
					{
						Globals.Message.ShowError(errorMessage);
					}
				}
			}
		}

		public new BaseJobDeclaration CurrentDataItem => (BaseJobDeclaration)base.CurrentDataItem;

		public CusEntryInstruction CurrentEntryInstruction => (CusEntryInstruction)EntryInstructionsGrid.ListManager?.GetCurrent();

		protected virtual Type GetDetailsUserControlType() => null;
	}
}
