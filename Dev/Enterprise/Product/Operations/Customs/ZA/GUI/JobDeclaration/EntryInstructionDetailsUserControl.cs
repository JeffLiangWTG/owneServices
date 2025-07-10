using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Common.ModuleRegistration;
using Enterprise.Customs.GUI;
using Enterprise.Customs.ZA.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
#if DEBUG
using Enterprise.ZArchitecture.GUI.Testing;
#endif

namespace Enterprise.Customs.ZA.GUI
{
	public partial class EntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();

			synchronizeWithBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("{53041573-64E7-4E74-AEAE-3659DC2FB08F}", "&Synchronize with Inventory"));
			synchronizeWithBondedWarehouseMenuItem.Click += SynchronizeWithBondedWarehouseMenuItem_Click;

			inventoriesSelectionFromBondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("{8FC788DD-70CD-4D6A-9694-BA87D6D544BB}", "S&elect Inventory"));
			inventoriesSelectionFromBondedWarehouseMenuItem.Click += InventoriesSelectionFromBondedWarehouseMenuItem_Click;

			bondedWarehouseMenuItem = new ZMenuItem(ResString.GetMultilingualString("{D0820C67-8835-4EE9-89BF-F066BE27BD31}", "Inventory Management’"), new MenuItem[] { synchronizeWithBondedWarehouseMenuItem, inventoriesSelectionFromBondedWarehouseMenuItem });
			EntryInstructionsGrid.ContextMenu.MenuItems.Add(new ZMenuItem("-"));
			EntryInstructionsGrid.ContextMenu.MenuItems.Add(bondedWarehouseMenuItem);
			EntryInstructionsGrid.ContextMenu.Popup += ContextMenu_Popup;

#if DEBUG
			TypeDescriptor.AddAttributes(PortOfExitDropEdit, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(BankCodeDropEdit, new SuppressFormsLocalizedTestAttribute());
			MissingResourceStringChecker.ExcludeFromTest(this.ProvisionalPaymentTypeDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(this.CreditTermDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(this.RefTypeDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(this.EntityTypeDropEdit);
			MissingResourceStringChecker.ExcludeFromTest(this.ScopeDropEdit);
#endif
		}

		public new JobDeclaration CurrentDataItem
		{
			get { return (JobDeclaration)base.CurrentDataItem; }
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
			bondedWarehouseMenuItem.Visible = CurrentEntryInstruction?.IsInventorySelectionEnabled ?? false;
		}

		readonly ZMenuItem bondedWarehouseMenuItem;
		readonly ZMenuItem synchronizeWithBondedWarehouseMenuItem;
		readonly ZMenuItem inventoriesSelectionFromBondedWarehouseMenuItem;

		void InventoriesSelectionFromBondedWarehouseMenuItem_Click(object sender, EventArgs e)
		{
			var currentEntryInstruction = CurrentEntryInstruction;
			if (currentEntryInstruction != null)
			{
				using (var form = new InventorySelectionForm(new DeclarationInventorySelectionHeader(currentEntryInstruction)))
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(form) == DialogResult.OK)
					{
						var parentForm = GetParentForm();
						if (parentForm is ShipmentForm shipmentForm)
						{
							shipmentForm.PlugIns.SelectPlugInTabPage(CustomsControllerIDs.JobDeclaration);
							var plugin = (BaseBrokeragePlugIn)shipmentForm.PlugIns.GetPlugIn(CustomsControllerIDs.JobDeclaration);
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
					Globals.Message.ShowInformation(Res.GetString("{2EF189AF-3C0D-4AD0-A584-6635AC4BE5D8}", "At least one invoice line is required for this entry instruction in order for Synchronization to work."));
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

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.JE_MessageTypeInfo.ValueChanged -= JE_MessageTypeInfo_ValueChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				CurrentDataItem.JE_MessageTypeInfo.ValueChanged += JE_MessageTypeInfo_ValueChanged;
				CurrentDataItem.JE_OH_ImporterInfo.ValueChanged += JE_OH_ImporterInfo_ValueChanged;
				JE_MessageTypeInfo_ValueChanged(null, EventArgs.Empty);
				JE_OH_ImporterInfo_ValueChanged(null, EventArgs.Empty);
			}
		}

		void JE_OH_ImporterInfo_ValueChanged(object sender, EventArgs e)
		{
			UpdatePermitsGroupBoxLabels();
		}

		void UpdatePermitsGroupBoxLabels()
		{
			var importer = CurrentDataItem.Importer;
			if (importer != null)
			{
				RCCPermitsGroupBox.Text = Res.GetString("B6B70064-A163-4804-973E-65DE05A5D178", "Value Rebate Permits for {0}", importer.OH_Code);
				DutyRebateGroupBox.Text = Res.GetString("6218FEFC-1212-4525-9DB3-3B9299BF8202", "Duty Rebate Permits for {0}", importer.OH_Code);
				RCCImporterRequiredLabel.Visible = false;
				dutyRebateImportRequiredLabel.Visible = false;
				RCCPermitsGrid.Visible = true;
				DutyRebateGrid.Visible = true;
			}
			else
			{
				RCCPermitsGroupBox.Text = Res.GetString("5B39A420-6BF8-4956-B373-6FE736CB2412", "Value Rebate Permits");
				DutyRebateGroupBox.Text = Res.GetString("7BF01EF9-E7A5-4303-A2D8-B5FE03089DD1", "Duty Rebate Permits");
				RCCImporterRequiredLabel.Visible = true;
				dutyRebateImportRequiredLabel.Visible = true;
				RCCPermitsGrid.Visible = false;
				DutyRebateGrid.Visible = false;
			}
		}

		void JE_MessageTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			var declaration = CurrentDataItem;
			if (declaration != null)
			{
				bool isImport = declaration.IsImport;
				bool isExport = declaration.IsExport;

				SetControlVisibility(isImport, RCCPermitsGroupBox);
				SetControlVisibility(isImport, DutyRebateGroupBox);
				SetControlVisibility(isImport || isExport, RemovalDetailsControl);
				SetControlVisibility(declaration.BondedWarehouseEditable, ToWarehouseAddressControl, ToWarehouseGroupBox, FromWarehouseAddressControl, FromWarehouseGroupBox);
				CaseNumberSplitContainer.Panel2Collapsed = !isImport;

				EntryInstructionsGrid.SetAvailability(isExport, [CusEntryInstruction.Schema.CEI_BankCode, CusEntryInstruction.Schema.CEI_UCROrderNumber, CusEntryInstruction.Schema.CEI_CreditTerms, CusEntryInstruction.Schema.CEI_TransactionValue, CusEntryInstruction.Schema.CEI_RX_NKTransactionValueCurrency]);
				EntryInstructionsGrid.SetColumnCaption(CusEntryInstruction.Schema.CEI_PreviousMRN, declaration.IsImportByExternalBroker ? Res.GetString("CDA8CE6F-EA26-49AE-84C1-330F8CE62837", "WHS MRN")
					: Res.GetString("D5EDBBCC-1C20-4A7C-8EAF-D222F55577A7", "Previous MRN"));
				UpdateExchangeRateDateUserControls(isExport);
				UpdateUCRGroupBoxUserControls(!isImport);
			}
		}

		void UpdateUCRGroupBoxUserControls(bool enabled)
		{
			UCROverrideCheckBox.Checked = !enabled;
			OrderNumberZTextBox.Enabled = !CurrentEntryInstruction?.CEI_UCROrderNumberInfo.ReadOnly ?? enabled;
			RefTypeDropEdit.Enabled = !CurrentEntryInstruction?.CEI_RefTypeInfo.ReadOnly ?? enabled;
			EntityTypeDropEdit.Enabled = !CurrentEntryInstruction?.CEI_EntityTypeInfo.ReadOnly ?? enabled;
			ScopeDropEdit.Enabled = !CurrentEntryInstruction?.CEI_ScopeInfo.ReadOnly ?? enabled;
		}

		void UpdateExchangeRateDateUserControls(bool isExport)
		{
			ExchangeRateDateZDateEdit.Visible = isExport;
			EntryInstructionsGrid.SetAvailability(isExport, AutoZACusEntryInstruction.Schema.CEI_ExchangeRateDate);
		}

		static void SetControlVisibility(bool condition, params Control[] controls)
		{
			foreach (var control in controls)
			{
				control.Visible = condition;
			}
		}

		public CusEntryInstruction CurrentEntryInstruction => (CusEntryInstruction)EntryInstructionsGrid.ListManager?.GetCurrent();
	}
}
