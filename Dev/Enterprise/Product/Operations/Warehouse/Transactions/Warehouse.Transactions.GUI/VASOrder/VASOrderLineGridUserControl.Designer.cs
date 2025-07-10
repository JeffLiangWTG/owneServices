namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class VASOrderLineGridUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.VASOrderLineGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.VASOrderLineGrid)).BeginInit();
			this.VASOrderLineGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsVASOrder);
			// 
			// VASOrderLineGrid
			// 
			this.VASOrderLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.VASOrderLineGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).WVL_LineNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).WVL_OP_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).ProductDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).WVL_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).WVL_PackingDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDate)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).WVL_ExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).WVL_PartAttrib1)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).WVL_PartAttrib2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).WVL_PartAttrib3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrderLine)(((System.Collections.IList)(((Enterprise.Warehouse.Transactions.Business.WhsVASOrder)(null)).Lines)).SyncRoot)).WVL_SerialNumber)));
			this.VASOrderLineGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "WVL_LineNumber";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WVL_OP_Product";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "ProductDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "WVL_Quantity";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "WVL_PackingDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.ColumnName = "WVL_ExpiryDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Custom;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "WVL_PartAttrib1";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "WVL_PartAttrib2";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "WVL_PartAttrib3";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "WVL_SerialNumber";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.VASOrderLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.VASOrderLineGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.VASOrderLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.VASOrderLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.VASOrderLineGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.VASOrderLineGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.VASOrderLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.VASOrderLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.VASOrderLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.VASOrderLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.VASOrderLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VASOrderLineGrid.GridId = "c9cff5e7-a810-4fd9-951a-160711bb32f3";
			this.VASOrderLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VASOrderLineGrid.LayoutKey = "VASOrderLineGrid";
			this.VASOrderLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VASOrderLineGrid.Name = "VASOrderLineGrid";
			this.VASOrderLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 316, true);
			this.VASOrderLineGrid.TabIndex = 0;
			// 
			// VASOrderLineGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.VASOrderLineGrid);
			this.Name = "VASOrderLineGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 316, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.VASOrderLineGrid)).EndInit();
			this.VASOrderLineGrid.ResumeLayout(false);
			this.VASOrderLineGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid VASOrderLineGrid;
	}
}
