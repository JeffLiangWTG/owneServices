using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GUI
{
	public partial class InventorySelectionForm : ZChildForm
	{
		public InventorySelectionForm() { }

		public InventorySelectionForm(InventorySelectionHeader header) : base(header)
		{
			module = (ZFilterModule)ZModuleFactory.Instance.Create(ModuleIDs.WhsInventory);
			var filterBusinessObject = module.FilterBusinessObject;
			((IFilterStripBusinessObjectInternals)filterBusinessObject).LayoutContext = Env.CurrentCompany.Country.Code + "InventorySelectionForm";

			SetDefaultValuesSuspendingValidations(filterBusinessObject);
			SetupMainPanel(module);
			SetDataGridGridID();
			ChangeDataLayout();
			BusinessEntity.OnGroupByChanged += UpdateInventoryDetails;
			SetGroupByVisibilities(header);

			if (header.IsAutoSelectionSupported)
			{
				FillAllDrawQtyButton.CaptionResourceString = Res.GetData("7572DA21-3A1B-48B4-BDA6-13358E53072A", "Auto Select");
			}

			DialogResult = DialogResult.None;
		}
		readonly ZFilterModule module;

		void SetDefaultValuesSuspendingValidations(FilterStripBusinessObject filterBo)
		{
			var productFilter = filterBo.ModuleFilters["Product"].Cast<ModuleFilter>();
			productFilter.ForEach(filter => filter.SuspendValidation());

			filterBo.SetExternalDefaults(BusinessEntity.GetFilterDefaults());

			productFilter.ForEach(filter => filter.ResumeValidation());
		}

		void SetGroupByVisibilities(InventorySelectionHeader header)
		{
			var groupByCartonEnabled = header.IsGroupByCartonSupported;
			var groupByProductEnabled = header.IsGroupByProductSupported;
			CartonRadioButton.Visible = groupByCartonEnabled;
			ProductRadioButton.Visible = groupByProductEnabled;
			GroupByGroupBox.Visible = groupByCartonEnabled || groupByProductEnabled;
		}

		public new InventorySelectionHeader BusinessEntity
		{
			get { return (InventorySelectionHeader)base.BusinessEntity; }
		}

		void UpdateInventoryDetails(object sender, EventArgs e)
		{
			if (embeddedControl != null && embeddedControl.GridCollection.Count > 0)
			{
				BusinessEntity.UpdateSelectionLinesDetails(embeddedControl.GridCollection.OfType<Warehouse.Integration.IWhsInventoryView>());
				if (BusinessEntity.AutoFillOutDrawQuantities)
				{
					BusinessEntity.FillOutDrawQuantities();
				}
			}
			ChangeDataLayout();
		}

		ZFilterStripControl embeddedControl;
		void SetupMainPanel(ZEmbeddedModule module)
		{
			embeddedControl = (ZFilterStripControl)module.EmbeddedControl;
			((System.ComponentModel.ISupportInitialize)(embeddedControl.BindingSource)).BeginInit();
			embeddedControl.SuspendLayout();
			embeddedControl.FilteredGrid.Visible = false;
			embeddedControl.PerformSearch += UpdateInventoryDetails;
			var labels = embeddedControl.Controls.Find("ToolStripRecordsFoundLabel", false);
			if (labels.Length == 0)
			{
				ErrorReporter.ReportOnce("ToolStripRecordsFoundLabel doesn't exist");
			}
			else if (labels.Length > 1)
			{
				ErrorReporter.ReportOnce("ToolStripRecordsFoundLabel exists multiple time");
			}
			else
			{
				labels[0].Visible = false;
			}
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			Controls.Remove(MainPanel);
			MainPanel.Dock = embeddedControl.FilteredGrid.Dock;
			MainPanel.Anchor = embeddedControl.FilteredGrid.Anchor;
			MainPanel.Location = embeddedControl.FilteredGrid.Location;
			MainPanel.Size = embeddedControl.FilteredGrid.Size;
			MainPanel.AllowOverlap(embeddedControl.Controls.Find("ToolStrip", true).Single());
			MainPanel.AllowOverlap(embeddedControl.Controls.Find("ToolStripHelp", true).Single());
			embeddedControl.Controls.Add(MainPanel);
			embeddedControl.Controls.SetChildIndex(MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(embeddedControl.BindingSource)).EndInit();
			embeddedControl.ResumeLayout(false);
			embeddedControl.PerformLayout();
			embeddedControl.FilteredGrid.Layout += (object sender, LayoutEventArgs e) =>
			{
				if (MainPanel.Location != embeddedControl.FilteredGrid.Location)
				{
					MainPanel.Location = embeddedControl.FilteredGrid.Location;
				}
				if (MainPanel.Height != embeddedControl.FilteredGrid.Height)
				{
					ControlDpiScalingHelper.SetHeight(ref MainPanel, embeddedControl.FilteredGrid.Height, false);
				}
				if (MainPanel.Width != embeddedControl.FilteredGrid.Width)
				{
					ControlDpiScalingHelper.SetWidth(ref MainPanel, embeddedControl.FilteredGrid.Width, false);
				}
			};
			embeddedControl.TabIndex = 0;
			Controls.Add(embeddedControl);
			Controls.SetChildIndex(this.embeddedControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			ResumeLayout(false);
			PerformLayout();
		}

		void ChangeDataLayout()
		{
			DataGrid.SaveUserLayoutSettings();
			using (DataGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				SetDataGridGridID();
				if (BusinessEntity.IsGroupByCarton)
				{
					DataGrid.SetAvailability(false, [InventorySelectionLine.Schema.US_Attribute1, InventorySelectionLine.Schema.US_Attribute2, InventorySelectionLine.Schema.US_Attribute3, InventorySelectionLine.Schema.US_DeclarantsReference]);
					DataGrid.SetAvailability(true, [InventorySelectionLine.Schema.US_ArrivalDate, InventorySelectionLine.Schema.US_CustomsEntryKey, InventorySelectionLine.Schema.US_CartonQtyOnHand, InventorySelectionLine.Schema.US_CartonQtytoDraw, InventorySelectionLine.Schema.US_ProductQtyPerCarton, InventorySelectionLine.Schema.US_GroupingID, InventorySelectionLine.Schema.US_SerialNumber, InventorySelectionLine.Schema.US_CustomsDeadline]);
					DataGrid.SetColumnVisible(true, [InventorySelectionLine.Schema.US_ArrivalDate, InventorySelectionLine.Schema.US_CustomsEntryKey, InventorySelectionLine.Schema.US_CartonQtyOnHand, InventorySelectionLine.Schema.US_CartonQtytoDraw, InventorySelectionLine.Schema.US_GroupingID, InventorySelectionLine.Schema.US_SerialNumber, InventorySelectionLine.Schema.US_CustomsDeadline]);
					DataGrid.SetAvailability(true, InventorySelectionLine.Schema.US_OriginalPackType);
					DataGrid.SetColumnVisible(true, InventorySelectionLine.Schema.US_OriginalPackType);
					DataGrid.ReOrderColumns(
						[
							InventorySelectionLine.Schema.US_Product,
							InventorySelectionLine.Schema.US_Description,
							InventorySelectionLine.Schema.US_OriginalBondedQty,
							InventorySelectionLine.Schema.US_CartonQtyOnHand,
							InventorySelectionLine.Schema.US_CartonQtytoDraw,
							InventorySelectionLine.Schema.US_ProductQtyToDraw,
							InventorySelectionLine.Schema.US_ProductQtyOnHand,
							InventorySelectionLine.Schema.US_CustomsEntryKey,
							InventorySelectionLine.Schema.US_ArrivalDate,
							InventorySelectionLine.Schema.US_ProductQtyPerCarton,
							InventorySelectionLine.Schema.US_OriginalPackType,
							InventorySelectionLine.Schema.US_GroupingID,
							InventorySelectionLine.Schema.US_Warehouse
						]);
				}
				else if (BusinessEntity.IsGroupByProduct)
				{
					DataGrid.SetAvailability(false, [InventorySelectionLine.Schema.US_ArrivalDate, InventorySelectionLine.Schema.US_CartonQtyOnHand, InventorySelectionLine.Schema.US_CartonQtytoDraw, InventorySelectionLine.Schema.US_CustomsEntryKey, InventorySelectionLine.Schema.US_ProductQtyPerCarton, InventorySelectionLine.Schema.US_GroupingID, InventorySelectionLine.Schema.US_DeclarantsReference]);
					DataGrid.SetAvailability(true, [InventorySelectionLine.Schema.US_OriginalPackType, InventorySelectionLine.Schema.US_Attribute1, InventorySelectionLine.Schema.US_Attribute2, InventorySelectionLine.Schema.US_Attribute3, InventorySelectionLine.Schema.US_SerialNumber, InventorySelectionLine.Schema.US_CustomsDeadline]);
					DataGrid.SetColumnVisible(true, [InventorySelectionLine.Schema.US_OriginalPackType, InventorySelectionLine.Schema.US_Attribute1, InventorySelectionLine.Schema.US_SerialNumber, InventorySelectionLine.Schema.US_CustomsDeadline]);
					DataGrid.ReOrderColumns(
						[
							InventorySelectionLine.Schema.US_Product,
							InventorySelectionLine.Schema.US_Description,
							InventorySelectionLine.Schema.US_OriginalBondedQty,
							InventorySelectionLine.Schema.US_ProductQtyToDraw,
							InventorySelectionLine.Schema.US_ProductQtyOnHand,
							InventorySelectionLine.Schema.US_OriginalPackType,
							InventorySelectionLine.Schema.US_Warehouse
						]);
				}
				else
				{
					DataGrid.SetAvailability(true, [InventorySelectionLine.Schema.US_ArrivalDate, InventorySelectionLine.Schema.US_CustomsEntryKey, InventorySelectionLine.Schema.US_GroupingID, InventorySelectionLine.Schema.US_SerialNumber, InventorySelectionLine.Schema.US_CustomsDeadline]);
					DataGrid.SetColumnVisible(true, [InventorySelectionLine.Schema.US_ArrivalDate, InventorySelectionLine.Schema.US_CustomsEntryKey, InventorySelectionLine.Schema.US_GroupingID, InventorySelectionLine.Schema.US_SerialNumber, InventorySelectionLine.Schema.US_CustomsDeadline]);
					DataGrid.SetAvailability(false, [InventorySelectionLine.Schema.US_CartonQtyOnHand, InventorySelectionLine.Schema.US_CartonQtytoDraw]);
					DataGrid.SetAvailability(true, [InventorySelectionLine.Schema.US_OriginalPackType, InventorySelectionLine.Schema.US_Attribute1, InventorySelectionLine.Schema.US_Attribute2, InventorySelectionLine.Schema.US_Attribute3, InventorySelectionLine.Schema.US_DeclarantsReference]);
					DataGrid.SetColumnVisible(true, InventorySelectionLine.Schema.US_OriginalPackType);
					DataGrid.ReOrderColumns(
						[
							InventorySelectionLine.Schema.US_Product,
							InventorySelectionLine.Schema.US_Description,
							InventorySelectionLine.Schema.US_OriginalBondedQty,
							InventorySelectionLine.Schema.US_ProductQtyToDraw,
							InventorySelectionLine.Schema.US_ProductQtyOnHand,
							InventorySelectionLine.Schema.US_CustomsEntryKey,
							InventorySelectionLine.Schema.US_ArrivalDate,
							InventorySelectionLine.Schema.US_ProductQtyPerCarton,
							InventorySelectionLine.Schema.US_OriginalPackType,
							InventorySelectionLine.Schema.US_GroupingID,
							InventorySelectionLine.Schema.US_Warehouse
						]);
				}
			}
			DataGrid.LoadUserLayoutSettings();
			SelectButton.Enabled = (embeddedControl != null && embeddedControl.GridCollection.Count > 0);
		}

		void SetDataGridGridID()
		{
			var prefix = "P";
			var businessEntity = BusinessEntity;
			if (businessEntity != null)
			{
				if (businessEntity.IsGroupByCarton)
				{
					prefix = "C";
				}
				else if (businessEntity.IsGroupByInventory)
				{
					prefix = "I";
				}
			}
			DataGrid.GridId = prefix + dataGridGridID;
			DataGrid.CurrentColumnLayout = null;
		}
		const string dataGridGridID = "79a5b2d5-2899-484e-8374-9ce6d3d80f7e";

		void SelectButton_Click(object sender, EventArgs e)
		{
			if (BusinessEntity.SelectionLines.HasALeastOneLineWithDrawQty)
			{
				if (BusinessEntity.SelectionLines.WillDrawLineFromDifferentWarehouses)
				{
					Globals.Message.ShowError(Res.GetString("455C2140-9E3B-466F-AF89-6A4E744740CB", "Inventories belonging to different warehouses have been selected; please only select inventories from same warehouse."), Res.GetString("E9124498-5276-445C-9CD9-0C8F872298CC", "MULTIPLE WAREHOUSE SELECTED"));
				}
				else
				{
					BusinessEntity.RunPreSaveValidation();
					if (BusinessEntity.HasErrors)
					{
						using (var form = new ZErrorMessageBox(BusinessEntity))
						{
							ZFormModaliser.ShowMessageBoxWithoutDispose(form);
						}
					}
					else
					{
						BusinessEntity.ImportInventories();
						if (!BusinessEntity.ImportInventoriesResult.IsEmpty)
						{
							Globals.Message.Show(BusinessEntity.ImportInventoriesResult);
						}

						DialogResult = System.Windows.Forms.DialogResult.OK;
						Close();
					}
				}
			}
			else
			{
				Globals.Message.ShowError(Res.GetString("241b9640-be21-42e6-b129-58a0a379c3cc", "No inventory has been selected to draw from warehouse; please enter the draw qty on the inventory you want to draw from warehouse."), Res.GetString("7396fb56-133b-4513-86a7-a648c56af098", "NO INVENTORY SELECTED"));
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (embeddedControl != null)
				{
					embeddedControl.PerformSearch -= UpdateInventoryDetails;
				}

				var entity = BusinessEntity;
				if (entity != null)
				{
					entity.OnGroupByChanged -= UpdateInventoryDetails;
				}

				if (module != null)
				{
					module.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void ClearDrawQtyButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.ClearDrawQuantities();
		}

		void FillAllDrawQtyButton_Click(object sender, EventArgs e)
		{
			BusinessEntity.FillOutDrawQuantities();
		}
	}
}
