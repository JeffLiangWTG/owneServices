
namespace Enterprise.Freight.Forwarding.GUI
{
	partial class PackLineInspectionTypeBulkUpdateForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.InspectionBulkSetDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.statusBulkSetDescription = new Enterprise.ZArchitecture.ZLabel();
			this.PackLineGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InspectionBulkSetDropEdit.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackLineGrid)).BeginInit();
			this.PackLineGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 487, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource);
			// 
			// InspectionBulkSetDropEdit
			// 
			this.InspectionBulkSetDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InspectionBulkSetDropEdit, "OuterPackLinesInspectionTypeBulkSetter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLinesInspectionTypeBulkSetter)));
			this.InspectionBulkSetDropEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1b8e40f2-04e5-4042-a910-cd9a8d353759", "Set Inspection status to:", "The Inspection Status to set for all available packlines.");
			this.InspectionBulkSetDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.InspectionBulkSetDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 42, true);
			this.InspectionBulkSetDropEdit.Name = "InspectionBulkSetDropEdit";
			this.InspectionBulkSetDropEdit.ShouldResizeByMaxLength = true;
			this.InspectionBulkSetDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.InspectionBulkSetDropEdit.TabIndex = 1;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("b43f6773-4f68-44c1-b598-3570465ff2c7", "Aviation Security Inspection");
			this.zGroupBox1.Controls.Add(this.PostingButtonsUserControl);
			this.zGroupBox1.Controls.Add(this.statusBulkSetDescription);
			this.zGroupBox1.Controls.Add(this.PackLineGrid);
			this.zGroupBox1.Controls.Add(this.InspectionBulkSetDropEdit);
			this.zGroupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 487, true);
			this.zGroupBox1.TabIndex = 0;
			this.zGroupBox1.TabStop = false;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(643, 456, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 6;
			// 
			// statusBulkSetDescription
			// 
			this.statusBulkSetDescription.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("1854234e-614c-4330-b210-90230e5d0e16", "Setting the status will update all of the following selected packlines, which you can manually override as required:");
			this.statusBulkSetDescription.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.statusBulkSetDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 16, true);
			this.statusBulkSetDescription.Name = "statusBulkSetDescription";
			this.statusBulkSetDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(849, 23, true);
			this.statusBulkSetDescription.TabIndex = 5;
			// 
			// PackLineGrid
			// 
			this.PackLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PackLineGrid, "OuterPackLines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).ShipmentId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).PackLineId)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).PackLineInspectionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).ShipmentInspectionType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).PackageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).PackageCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).ActualVolume)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).ActualVolumeUQ)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).ActualWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateRecord)(((System.Collections.IList)(((Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource)(null)).OuterPackLines)).SyncRoot)).ActualWeightUQ)));
			this.PackLineGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("26f09bff-eb03-440d-bb8c-1c9fc8bd42d5", "Shipment ID");
			zTextBoxColumnStyleInfo1.ColumnName = "ShipmentId";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("4cbf4a56-e167-41d7-9464-9a71835009d4", "Packline ID");
			zTextBoxColumnStyleInfo2.ColumnName = "PackLineId";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("77227c2c-fadc-43c5-80d6-87473f5fa09b", "Packline Inspection");
			zDropEditColumnStyleInfo1.ColumnName = "PackLineInspectionType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("184c9c12-1df6-430a-98e5-c732d3382d00", "Shipment Inspection");
			zTextBoxColumnStyleInfo3.ColumnName = "ShipmentInspectionType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("f6380b38-1b37-4da3-9a5a-18a19dd9e2af", "Package Type");
			zTextBoxColumnStyleInfo4.ColumnName = "PackageType";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("72dfc67b-b827-485e-8d59-154bef92dfc3", "Packs");
			zCalcEditColumnStyleInfo1.ColumnName = "PackageCount";
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("d55d4a51-838a-49ba-8061-4c3f6790542c", "Volume");
			zCalcEditColumnStyleInfo2.ColumnName = "ActualVolume";
			zCalcEditColumnStyleInfo2.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("61f0df45-4735-4141-8a99-443403a801de", "UV");
			zTextBoxColumnStyleInfo5.ColumnName = "ActualVolumeUQ";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("a1d4eeaf-ed32-41b8-9f5f-bd48e20cb928", "Weight");
			zCalcEditColumnStyleInfo3.ColumnName = "ActualWeight";
			zCalcEditColumnStyleInfo3.IsReadOnly = true;
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("92ecb97a-cb8e-4354-92f5-5f09fe0fa84e", "UW");
			zTextBoxColumnStyleInfo6.ColumnName = "ActualWeightUQ";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(20);
			this.PackLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PackLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PackLineGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PackLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PackLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PackLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PackLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PackLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PackLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.PackLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PackLineGrid.GridId = "ef3ef6dd-758e-4801-a982-cccbab72e1f8";
			this.PackLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PackLineGrid.LayoutKey = "zGrid1";
			this.PackLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 68, true);
			this.PackLineGrid.Name = "PackLineGrid";
			this.PackLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 336, true);
			this.PackLineGrid.TabIndex = 2;
			// 
			// PackLineInspectionTypeBulkUpdateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("23c57a34-a09a-4e8a-bc8b-38b5ad0e8717", "Set Inspection Status");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 511, true);
			this.Controls.Add(this.zGroupBox1);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.PackLineBulkUpdateDataSource);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 550, true);
			this.Name = "PackLineInspectionTypeBulkUpdateForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InspectionBulkSetDropEdit.ResumeLayout(true);
			this.InspectionBulkSetDropEdit.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PackLineGrid)).EndInit();
			this.PackLineGrid.ResumeLayout(false);
			this.PackLineGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected ZArchitecture.GUI.ZDropEdit InspectionBulkSetDropEdit;
		protected ZArchitecture.GUI.ZGroupBox zGroupBox1;
		protected ZArchitecture.ZGrid PackLineGrid;
		protected ZArchitecture.ZLabel statusBulkSetDescription;
		protected Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
	}
}
