namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class InventoryAllocationsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.CrossDockedOrderLinesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.crossDockedOrderLineAttachedToInventoryGrid1 = new Enterprise.Warehouse.Transactions.GUI.CrossDockedOrderLineAttachedToInventoryGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CrossDockedOrderLinesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid)).BeginInit();
			this.crossDockedOrderLineAttachedToInventoryGrid1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsDocketLine);
			// 
			// CrossDockedOrderLinesGroupBox
			// 
			this.CrossDockedOrderLinesGroupBox.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("InventoryAllocationsUserControl|47ab59fb-805c-4647-b0f2-659d07cabb04", "Cross Docked Order Lines");
			this.CrossDockedOrderLinesGroupBox.Controls.Add(this.crossDockedOrderLineAttachedToInventoryGrid1);
			this.CrossDockedOrderLinesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CrossDockedOrderLinesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CrossDockedOrderLinesGroupBox.Name = "CrossDockedOrderLinesGroupBox";
			this.CrossDockedOrderLinesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 563, true);
			this.CrossDockedOrderLinesGroupBox.TabIndex = 13;
			this.CrossDockedOrderLinesGroupBox.TabStop = false;
			// 
			// crossDockedOrderLineAttachedToInventoryGrid1
			// 
			this.crossDockedOrderLineAttachedToInventoryGrid1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.crossDockedOrderLineAttachedToInventoryGrid1, "Inventory.ReservedPickLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsInventoryView)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsDocketLine)(null)).Inventory)).SyncRoot)).ReservedPickLines)));
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedOrderLineAttachedToInventoryGrid|8ac1a272-1b61-442f-8449-2de0c1b400dc", "Order No.");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "DocketLine+Docket+WD_ExternalReference";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedOrderLineAttachedToInventoryGrid|0929ab8f-35e0-438e-8a8d-1bd5df556ad8", "Consignee");
			zMultiControlColumnStyleInfo1.ColumnName = "ConsigneeNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "ConsigneeFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "DocketLine+Docket+RequiredDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedOrderLineAttachedToInventoryGrid|aa6ffeca-3dbb-4bf8-987b-17aa34d62424", "Product");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "DocketLine+WE_OP";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedOrderLineAttachedToInventoryGrid|ae65d00c-fbed-4b3b-a886-e4aab2834151", "Description");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "DocketLine+ProductDesc";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "DocketLine+SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo1.ColumnName = "ReservedQuantity";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = "DocketLine+SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo2.ColumnName = "WZ_OriginalReservedQty";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedOrderLineAttachedToInventoryGrid|6712c9b2-a23c-4db5-9737-09bda3ffc655", "Part Attrib. 1");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "DocketLine+WE_PartAttrib1";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedOrderLineAttachedToInventoryGrid|46c2869a-f19e-4347-98bb-02eb640f9154", "Part Attrib. 2");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "DocketLine+WE_PartAttrib2";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedOrderLineAttachedToInventoryGrid|fea10b8d-18a3-4f37-8378-d6f7b1c52edc", "Part Attrib. 3");
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "DocketLine+WE_PartAttrib3";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "DocketLine+WE_SerialNumber";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedOrderLineAttachedToInventoryGrid|9b25bae9-65ad-4814-b619-4120f6b83d0d", "Expiry Date");
			zDateEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo2.ColumnName = "DocketLine+WE_ExpiryDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("CrossDockedOrderLineAttachedToInventoryGrid|b00d8972-bd99-448b-9247-fcc80b3d53be", "Packing Date");
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "DocketLine+WE_PackingDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "DocketLine+WE_PalletID";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.crossDockedOrderLineAttachedToInventoryGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.crossDockedOrderLineAttachedToInventoryGrid1.GridId = "48208a2f-46fb-4c57-8fb2-31132088f6b0";
			// 
			// 
			// 
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.AllowNavigation = false;
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.CaptionVisible = false;
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.GridId = null;
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.LayoutKey = "Grid";
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.Name = "Grid";
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(735, 506, true);
			this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid.TabIndex = 0;
			this.crossDockedOrderLineAttachedToInventoryGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.crossDockedOrderLineAttachedToInventoryGrid1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.WhsOrderLine;
			this.crossDockedOrderLineAttachedToInventoryGrid1.Name = "crossDockedOrderLineAttachedToInventoryGrid1";
			this.crossDockedOrderLineAttachedToInventoryGrid1.ReadOnly = false;
			this.crossDockedOrderLineAttachedToInventoryGrid1.ShowAttachButton = false;
			this.crossDockedOrderLineAttachedToInventoryGrid1.ShowDetachButton = false;
			this.crossDockedOrderLineAttachedToInventoryGrid1.ShowNewButton = false;
			this.crossDockedOrderLineAttachedToInventoryGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(741, 544, true);
			this.crossDockedOrderLineAttachedToInventoryGrid1.TabIndex = 2;
			// 
			// InventoryAllocationsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CrossDockedOrderLinesGroupBox);
			this.Name = "InventoryAllocationsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 563, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CrossDockedOrderLinesGroupBox.ResumeLayout(false);
			this.CrossDockedOrderLinesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.crossDockedOrderLineAttachedToInventoryGrid1.InnerGrid)).EndInit();
			this.crossDockedOrderLineAttachedToInventoryGrid1.ResumeLayout(true);
			this.crossDockedOrderLineAttachedToInventoryGrid1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox CrossDockedOrderLinesGroupBox;
		private CrossDockedOrderLineAttachedToInventoryGrid crossDockedOrderLineAttachedToInventoryGrid1;

	}
}
