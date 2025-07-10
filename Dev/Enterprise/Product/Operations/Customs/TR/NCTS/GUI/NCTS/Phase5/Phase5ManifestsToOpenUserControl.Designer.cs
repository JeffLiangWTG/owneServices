namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class Phase5ManifestsToOpenUserControl
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
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			this.ManifestsToOpenSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.AdditionalInfoGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ManifestsToOpenGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsPartialCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AtWarehouseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.WarehouseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DeclarationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BillLineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.QuantityCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.IsOtherProcedureCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ManifestsToOpenSplitContainer)).BeginInit();
			this.ManifestsToOpenSplitContainer.Panel1.SuspendLayout();
			this.ManifestsToOpenSplitContainer.Panel2.SuspendLayout();
			this.ManifestsToOpenSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfoGrid)).BeginInit();
			this.AdditionalInfoGrid.SuspendLayout();
			this.ManifestsToOpenGroupBox.SuspendLayout();
			this.WarehouseCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsHeader);
			// 
			// ManifestsToOpenSplitContainer
			// 
			this.ManifestsToOpenSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestsToOpenSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManifestsToOpenSplitContainer.Name = "ManifestsToOpenSplitContainer";
			this.ManifestsToOpenSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ManifestsToOpenSplitContainer.Panel1
			// 
			this.ManifestsToOpenSplitContainer.Panel1.Controls.Add(this.AdditionalInfoGrid);
			// 
			// ManifestsToOpenSplitContainer.Panel2
			// 
			this.ManifestsToOpenSplitContainer.Panel2.Controls.Add(this.ManifestsToOpenGroupBox);
			this.ManifestsToOpenSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 589, true);
			this.ManifestsToOpenSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(206);
			this.ManifestsToOpenSplitContainer.TabIndex = 0;
			// 
			// AdditionalInfoGrid
			// 
			this.AdditionalInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalInfoGrid, "ManifestsToOpenList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).IsPartial)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_Quantity3)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).AtWarehouse)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_CustomsOffice)));
			this.AdditionalInfoGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.ColumnName = "IsPartial";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "CSI_Quantity3";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo2.ColumnName = "AtWarehouse";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_CustomsOffice";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AdditionalInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.AdditionalInfoGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfoGrid.GridId = "142de25f-601d-4237-90d0-660438aa83e6";
			this.AdditionalInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalInfoGrid.LayoutKey = "GoodsItemsGrid";
			this.AdditionalInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfoGrid.Name = "AdditionalInfoGrid";
			this.AdditionalInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 206, true);
			this.AdditionalInfoGrid.TabIndex = 1;
			// 
			// ManifestsToOpenGroupBox
			// 
			this.ManifestsToOpenGroupBox.Controls.Add(this.IsPartialCheckBox);
			this.ManifestsToOpenGroupBox.Controls.Add(this.AtWarehouseCheckBox);
			this.ManifestsToOpenGroupBox.Controls.Add(this.WarehouseCodeDropEdit);
			this.ManifestsToOpenGroupBox.Controls.Add(this.DeclarationNoTextBox);
			this.ManifestsToOpenGroupBox.Controls.Add(this.BillNoTextBox);
			this.ManifestsToOpenGroupBox.Controls.Add(this.BillLineNoCalcEdit);
			this.ManifestsToOpenGroupBox.Controls.Add(this.QuantityCalcEdit);
			this.ManifestsToOpenGroupBox.Controls.Add(this.IsOtherProcedureCheckBox);
			this.ManifestsToOpenGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestsToOpenGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManifestsToOpenGroupBox.Name = "ManifestsToOpenGroupBox";
			this.ManifestsToOpenGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 379, true);
			this.ManifestsToOpenGroupBox.TabIndex = 0;
			this.ManifestsToOpenGroupBox.TabStop = false;
			this.ManifestsToOpenGroupBox.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("3D00EDBC-0BB0-4833-954A-AD78837BD7C6", "Manifests To Open Details");
			// 
			// IsPartialCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsPartialCheckBox, "ManifestsToOpenList.IsPartial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).IsPartial)));
			this.IsPartialCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsPartialCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsPartialCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 28, true);
			this.IsPartialCheckBox.Name = "IsPartialCheckBox";
			this.IsPartialCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 22, true);
			this.IsPartialCheckBox.TabIndex = 1;
			this.IsPartialCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsPartialCheckBox.UseVisualStyleBackColor = true;
			// 
			// AtWarehouseCheckBox
			// 
			this.BindingSource.SetBindingMember(this.AtWarehouseCheckBox, "ManifestsToOpenList.AtWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).AtWarehouse)));
			this.AtWarehouseCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AtWarehouseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AtWarehouseCheckBox.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.AtWarehouseCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.AtWarehouseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 56, true);
			this.AtWarehouseCheckBox.Name = "AtWarehouseCheckBox";
			this.AtWarehouseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 22, true);
			this.AtWarehouseCheckBox.TabIndex = 2;
			this.AtWarehouseCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.AtWarehouseCheckBox.UseVisualStyleBackColor = true;
			// 
			// WarehouseCodeDropEdit
			// 
			this.WarehouseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseCodeDropEdit, "ManifestsToOpenList.CSI_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_CustomsOffice)));
			this.WarehouseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 84, true);
			this.WarehouseCodeDropEdit.Name = "WarehouseCodeDropEdit";
			this.WarehouseCodeDropEdit.PreBoundMaxLength = 1;
			this.WarehouseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.WarehouseCodeDropEdit.TabIndex = 3;
			// 
			// DeclarationNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarationNoTextBox, "ManifestsToOpenList.CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_ReferenceNumber2)));
			this.DeclarationNoTextBox.CaptionResourceString = null;
			this.DeclarationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 112, true);
			this.DeclarationNoTextBox.Name = "DeclarationNoTextBox";
			this.DeclarationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.DeclarationNoTextBox.TabIndex = 4;
			// 
			// BillNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.BillNoTextBox, "ManifestsToOpenList.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_ReferenceNumber)));
			this.BillNoTextBox.CaptionResourceString = null;
			this.BillNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 140, true);
			this.BillNoTextBox.Name = "BillNoTextBox";
			this.BillNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.BillNoTextBox.TabIndex = 5;
			// 
			// BillLineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BillLineNoCalcEdit, "ManifestsToOpenList.CSI_LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_LineNo)));
			this.BillLineNoCalcEdit.CaptionResourceString = null;
			this.BillLineNoCalcEdit.DecimalPlaces = 0;
			this.BillLineNoCalcEdit.Decimals = 0;
			this.BillLineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 168, true);
			this.BillLineNoCalcEdit.Name = "BillLineNoCalcEdit";
			this.BillLineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.BillLineNoCalcEdit.TabIndex = 6;
			this.BillLineNoCalcEdit.Text = "0";
			this.BillLineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// QuantityCalcEdit
			// 
			this.QuantityCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QuantityCalcEdit, "ManifestsToOpenList.CSI_Quantity3");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).CSI_Quantity3)));
			this.QuantityCalcEdit.CaptionResourceString = null;
			this.QuantityCalcEdit.DecimalPlaces = 2;
			this.QuantityCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 196, true);
			this.QuantityCalcEdit.Name = "QuantityCalcEdit";
			this.QuantityCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.QuantityCalcEdit.TabIndex = 7;
			this.QuantityCalcEdit.Text = "0.00";
			this.QuantityCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// IsOtherProcedureCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsOtherProcedureCheckBox, "ManifestsToOpenList.IsOtherProcedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsManifestsToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsHeader)(null)).ManifestsToOpenList)).SyncRoot)).IsOtherProcedure)));
			this.IsOtherProcedureCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsOtherProcedureCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsOtherProcedureCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 224, true);
			this.IsOtherProcedureCheckBox.Name = "IsOtherProcedureCheckBox";
			this.IsOtherProcedureCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 22, true);
			this.IsOtherProcedureCheckBox.TabIndex = 8;
			this.IsOtherProcedureCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.IsOtherProcedureCheckBox.UseVisualStyleBackColor = true;
			// 
			// Phase5ManifestsToOpenUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManifestsToOpenSplitContainer);
			this.Name = "Phase5ManifestsToOpenUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 589, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ManifestsToOpenSplitContainer.Panel1.ResumeLayout(false);
			this.ManifestsToOpenSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ManifestsToOpenSplitContainer)).EndInit();
			this.ManifestsToOpenSplitContainer.ResumeLayout(false);
			this.ManifestsToOpenSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfoGrid)).EndInit();
			this.AdditionalInfoGrid.ResumeLayout(false);
			this.AdditionalInfoGrid.PerformLayout();
			this.ManifestsToOpenGroupBox.ResumeLayout(false);
			this.ManifestsToOpenGroupBox.PerformLayout();
			this.WarehouseCodeDropEdit.ResumeLayout(true);
			this.WarehouseCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private CargoWise.Windows.UI.KSplitContainer ManifestsToOpenSplitContainer;

		#endregion	}

		public ZArchitecture.ZGrid AdditionalInfoGrid;
		protected ZArchitecture.GUI.ZGroupBox ManifestsToOpenGroupBox;
		private ZArchitecture.GUI.ZCheckBox IsPartialCheckBox;
		private ZArchitecture.GUI.ZCheckBox AtWarehouseCheckBox;
		private ZArchitecture.GUI.ZCodeFindBox WarehouseCodeDropEdit;
		private ZArchitecture.ZTextBox DeclarationNoTextBox;
		private ZArchitecture.ZTextBox BillNoTextBox;
		private ZArchitecture.ZCalcEdit BillLineNoCalcEdit;
		private ZArchitecture.ZCalcEdit QuantityCalcEdit;
		private ZArchitecture.GUI.ZCheckBox IsOtherProcedureCheckBox;
	}
}
