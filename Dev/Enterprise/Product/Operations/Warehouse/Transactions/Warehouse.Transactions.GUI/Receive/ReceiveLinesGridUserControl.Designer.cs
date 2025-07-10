using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class ReceiveDocketLinesGridUserControl
	{
		internal ZDocAddressControl ConsigneeDocAddressControl;
		internal SerialNumberUserControl SerialNumberControl;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZMultiControlColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZDateTimeOffsetEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateTimeOffsetEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			this.ConsigneeDocAddressControl = new ZDocAddressControl();
			this.SerialNumberControl = new SerialNumberUserControl();
			((ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			this.SerialNumberControl.SuspendLayout();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsigneeDocAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// LinesGrid
			//
			zGuidFindBoxColumnStyleInfo1.AutoCompleteDisabled = true;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveDocketLinesGridUserControl|ec29a44f-de60-4dae-a48d-ad47b149beb4", "Location");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WE_WL";
			zGuidFindBoxColumnStyleInfo1.ToolTip = "Enter the location here";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "PutawayLocationAreaType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveDocketLinesGridUserControl|2ddcf44b-9f3c-4a2b-a976-ee4ae4ec31ef", "Putaway Transfer");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "PutawayTransferID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "PutawayLocationAreaName";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "WE_ReceiveCrossDockOrderNo";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "WE_PalletID";
			zTextBoxColumnStyleInfo5.ToolTip = "Enter Pallet ID.";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "ConsigneeNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ConsigneeFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "HasEDocsOrNotesAttached";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryGridUserControl|d5ac7ac9-8b71-434b-adbd-78a1db71ea1b", "Reserved Qty");
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "ReservedQuantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "SplitQuantity";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "Lookups.InventoryHeldCodeCollection";
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "WE_WHC_NKOriginalInventoryHeldCode";
			zDropEditColumnStyleInfo1.ToolTip = "Enter or select the Inventory Hold Code.";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "WE_OriginalInventoryStatus";
			zDropEditColumnStyleInfo2.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveDocketLinesGridUserControl|3022dcbb-9eef-47b5-9006-2cceb34f4c9d", "Status");
			zDropEditColumnStyleInfo2.ToolTip = "Enter or select the inventory status.";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "OriginalInventoryStatusDescription";
			zTextBoxColumnStyleInfo6.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("ReceiveDocketLinesGridUserControl|3022dcbb-9eef-47b5-9006-2cceb34f4c9d", "Status");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "WE_RequiredByDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "DestLocation";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo8.ColumnName = "OriginalHoldReason";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "WE_AllocationKey";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.LinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LinesGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsReceive);
			// 
			// ConsigneeDocAddressControl
			// 
			this.ConsigneeDocAddressControl.AllowDrop = true;
			this.ConsigneeDocAddressControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ConsigneeDocAddressControl, "Lines.ConsigneeDocAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((MasterFiles.Business.JobDocAddress)(((WhsReceiveLine)(((System.Collections.IList)(((WhsReceive)(null)).Lines)).SyncRoot)).ConsigneeDocAddress)));
			this.ConsigneeDocAddressControl.BindToOrganisations = "Lookups.Consignees";
			this.ConsigneeDocAddressControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryGridUserControl|eec0d0fd-329e-42f5-9188-40e44248d17a", "Consignee");
			this.ConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(444, 0, true);
			this.ConsigneeDocAddressControl.Name = "ConsigneeDocAddressControl";
			this.ConsigneeDocAddressControl.ReadOnly = false;
			this.ConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 182, true);
			this.ConsigneeDocAddressControl.TabIndex = 2;
			this.ConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// SerialNumberControl
			// 
			this.SerialNumberControl.AllowDrop = true;
			this.SerialNumberControl.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SerialNumberControl, "Lines.SerialNumbers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Warehouse.Transactions.Business.WhsSerialNumberPivotCollection)(((WhsReceiveLine)(((System.Collections.IList)(((WhsReceive)(null)).Lines)).SyncRoot)).SerialNumbers)));
			this.SerialNumberControl.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryGridUserControl|SerialNumbers", "Serial Numbers");
			this.SerialNumberControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(495, 0, true);
			this.SerialNumberControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.SerialNumberControl.Name = "SerialNumberControl";
			this.SerialNumberControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 500, true);
			this.SerialNumberControl.TabIndex = 3;
			this.SerialNumberControl.Visible = false;
			// 
			// ReceiveDocketLinesGridUserControl
			// 
			this.Controls.Add(this.ConsigneeDocAddressControl);
			this.Controls.Add(this.SerialNumberControl);
			this.Name = "ReceiveDocketLinesGridUserControl";
			this.Controls.SetChildIndex(this.ConsigneeDocAddressControl, 0);
			this.Controls.SetChildIndex(this.SerialNumberControl, 0);
			this.Controls.SetChildIndex(this.LinesGrid, 0);
			((ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsigneeDocAddressControl.ResumeLayout(true);
			this.ConsigneeDocAddressControl.PerformLayout();
			this.SerialNumberControl.ResumeLayout(true);
			this.SerialNumberControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
