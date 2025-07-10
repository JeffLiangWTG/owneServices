namespace Enterprise.Customs.TW.GUI
{
	partial class OrganisationDetailPlugInUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.HideEXPBuyerZHTAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideEXPExporterZHTAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideIMPImporterZHTAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideIMPImporterAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideIMPSellerZHTAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DefaultExamModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefaultIMPPaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefaultEXPPaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.HideEXPExporterAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.HideEXPBuyerAddrCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ExportDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ExportDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ImportDocumentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ImportDocumentGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DefaultExamModeDropEdit.SuspendLayout();
			this.DefaultIMPPaymentMethodDropEdit.SuspendLayout();
			this.DefaultEXPPaymentMethodDropEdit.SuspendLayout();
			this.ExportDocumentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExportDocumentGrid)).BeginInit();
			this.ExportDocumentGrid.SuspendLayout();
			this.ImportDocumentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ImportDocumentGrid)).BeginInit();
			this.ImportDocumentGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.OrgHeaderWrapper);
			// 
			// HideEXPBuyerZHTAddrCheckBox
			// 
			this.HideEXPBuyerZHTAddrCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HideEXPBuyerZHTAddrCheckBox, "AddInfo.ZO_TWHideEXPBuyerZHTAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWHideEXPBuyerZHTAddr)));
			this.HideEXPBuyerZHTAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 16, true);
			this.HideEXPBuyerZHTAddrCheckBox.Name = "HideEXPBuyerZHTAddrCheckBox";
			this.HideEXPBuyerZHTAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 17, true);
			this.HideEXPBuyerZHTAddrCheckBox.TabIndex = 1;
			this.HideEXPBuyerZHTAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideEXPExporterZHTAddrCheckBox
			// 
			this.HideEXPExporterZHTAddrCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HideEXPExporterZHTAddrCheckBox, "AddInfo.ZO_TWHideEXPExporterZHTAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWHideEXPExporterZHTAddr)));
			this.HideEXPExporterZHTAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 62, true);
			this.HideEXPExporterZHTAddrCheckBox.Name = "HideEXPExporterZHTAddrCheckBox";
			this.HideEXPExporterZHTAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 17, true);
			this.HideEXPExporterZHTAddrCheckBox.TabIndex = 3;
			this.HideEXPExporterZHTAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideIMPImporterZHTAddrCheckBox
			// 
			this.HideIMPImporterZHTAddrCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HideIMPImporterZHTAddrCheckBox, "AddInfo.ZO_TWHideIMPImporterZHTAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWHideIMPImporterZHTAddr)));
			this.HideIMPImporterZHTAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 108, true);
			this.HideIMPImporterZHTAddrCheckBox.Name = "HideIMPImporterZHTAddrCheckBox";
			this.HideIMPImporterZHTAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 17, true);
			this.HideIMPImporterZHTAddrCheckBox.TabIndex = 5;
			this.HideIMPImporterZHTAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideIMPImporterAddrCheckBox
			// 
			this.HideIMPImporterAddrCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HideIMPImporterAddrCheckBox, "AddInfo.ZO_TWHideIMPImporterAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWHideIMPImporterAddr)));
			this.HideIMPImporterAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 131, true);
			this.HideIMPImporterAddrCheckBox.Name = "HideIMPImporterAddrCheckBox";
			this.HideIMPImporterAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(174, 17, true);
			this.HideIMPImporterAddrCheckBox.TabIndex = 6;
			this.HideIMPImporterAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideIMPSellerZHTAddrCheckBox
			// 
			this.HideIMPSellerZHTAddrCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HideIMPSellerZHTAddrCheckBox, "AddInfo.ZO_TWHideIMPSellerZHTAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWHideIMPSellerZHTAddr)));
			this.HideIMPSellerZHTAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 154, true);
			this.HideIMPSellerZHTAddrCheckBox.Name = "HideIMPSellerZHTAddrCheckBox";
			this.HideIMPSellerZHTAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(196, 17, true);
			this.HideIMPSellerZHTAddrCheckBox.TabIndex = 7;
			this.HideIMPSellerZHTAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// DefaultExamModeDropEdit
			// 
			this.DefaultExamModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultExamModeDropEdit, "AddInfo.ZO_TWDefaultExamMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWDefaultExamMode)));
			this.DefaultExamModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 178, true);
			this.DefaultExamModeDropEdit.Name = "DefaultExamModeDropEdit";
			this.DefaultExamModeDropEdit.PreBoundMaxLength = 3;
			this.DefaultExamModeDropEdit.ShouldResizeByMaxLength = false;
			this.DefaultExamModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DefaultExamModeDropEdit.TabIndex = 8;
			// 
			// DefaultIMPPaymentMethodDropEdit
			// 
			this.DefaultIMPPaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultIMPPaymentMethodDropEdit, "AddInfo.ZO_TWDefaultIMPPaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWDefaultIMPPaymentMethod)));
			this.DefaultIMPPaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 204, true);
			this.DefaultIMPPaymentMethodDropEdit.Name = "DefaultIMPPaymentMethodDropEdit";
			this.DefaultIMPPaymentMethodDropEdit.PreBoundMaxLength = 3;
			this.DefaultIMPPaymentMethodDropEdit.ShouldResizeByMaxLength = false;
			this.DefaultIMPPaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DefaultIMPPaymentMethodDropEdit.TabIndex = 9;
			// 
			// DefaultEXPPaymentMethodDropEdit
			// 
			this.DefaultEXPPaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultEXPPaymentMethodDropEdit, "AddInfo.ZO_TWDefaultEXPPaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWDefaultEXPPaymentMethod)));
			this.DefaultEXPPaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(191, 230, true);
			this.DefaultEXPPaymentMethodDropEdit.Name = "DefaultEXPPaymentMethodDropEdit";
			this.DefaultEXPPaymentMethodDropEdit.PreBoundMaxLength = 3;
			this.DefaultEXPPaymentMethodDropEdit.ShouldResizeByMaxLength = false;
			this.DefaultEXPPaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.DefaultEXPPaymentMethodDropEdit.TabIndex = 10;
			// 
			// HideEXPExporterAddrCheckBox
			// 
			this.HideEXPExporterAddrCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HideEXPExporterAddrCheckBox, "AddInfo.ZO_TWHideEXPExporterAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWHideEXPExporterAddr)));
			this.HideEXPExporterAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 85, true);
			this.HideEXPExporterAddrCheckBox.Name = "HideEXPExporterAddrCheckBox";
			this.HideEXPExporterAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(179, 17, true);
			this.HideEXPExporterAddrCheckBox.TabIndex = 4;
			this.HideEXPExporterAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// HideEXPBuyerAddrCheckBox
			// 
			this.HideEXPBuyerAddrCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HideEXPBuyerAddrCheckBox, "AddInfo.ZO_TWHideEXPBuyerAddr");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).AddInfo.ZO_TWHideEXPBuyerAddr)));
			this.HideEXPBuyerAddrCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 39, true);
			this.HideEXPBuyerAddrCheckBox.Name = "HideEXPBuyerAddrCheckBox";
			this.HideEXPBuyerAddrCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 17, true);
			this.HideEXPBuyerAddrCheckBox.TabIndex = 2;
			this.HideEXPBuyerAddrCheckBox.UseVisualStyleBackColor = true;
			// 
			// ExportDocumentGroupBox
			// 
			this.ExportDocumentGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("1a0858fd-b562-4a1f-9a8a-dbceb6ecb8e2", "Override Export Declaration Document Box 33");
			this.ExportDocumentGroupBox.Controls.Add(this.ExportDocumentGrid);
			this.ExportDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 273, true);
			this.ExportDocumentGroupBox.Name = "ExportDocumentGroupBox";
			this.ExportDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 250, true);
			this.ExportDocumentGroupBox.TabIndex = 11;
			this.ExportDocumentGroupBox.TabStop = false;
			// 
			// ExportDocumentGrid
			// 
			this.ExportDocumentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExportDocumentGrid, "ExportCustomDocumentLabels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).ExportCustomDocumentLabels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).ExportCustomDocumentLabels)).SyncRoot)).OT_Position)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).ExportCustomDocumentLabels)).SyncRoot)).OT_Caption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).ExportCustomDocumentLabels)).SyncRoot)).OT_FieldName)));
			this.ExportDocumentGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "OT_Position";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo1.ColumnName = "OT_Caption";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDropEditColumnStyleInfo1.ColumnName = "OT_FieldName";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ExportDocumentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ExportDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ExportDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ExportDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportDocumentGrid.GridId = "f84aff7d-cd0c-4fe6-847f-eb1dba3afa65";
			this.ExportDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExportDocumentGrid.LayoutKey = "ExportDocumentGrid";
			this.ExportDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ExportDocumentGrid.Name = "ExportDocumentGrid";
			this.ExportDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 231, true);
			this.ExportDocumentGrid.TabIndex = 0;
			// 
			// ImportDocumentGroupBox
			// 
			this.ImportDocumentGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("f3893c84-b2a2-4847-883e-5477643cb9f5", "Override Import Declaration Document Box 35");
			this.ImportDocumentGroupBox.Controls.Add(this.ImportDocumentGrid);
			this.ImportDocumentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 273, true);
			this.ImportDocumentGroupBox.Name = "ImportDocumentGroupBox";
			this.ImportDocumentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 250, true);
			this.ImportDocumentGroupBox.TabIndex = 12;
			this.ImportDocumentGroupBox.TabStop = false;
			// 
			// ImportDocumentGrid
			// 
			this.ImportDocumentGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ImportDocumentGrid, "ImportCustomDocumentLabels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).ImportCustomDocumentLabels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).ImportCustomDocumentLabels)).SyncRoot)).OT_Position)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).ImportCustomDocumentLabels)).SyncRoot)).OT_Caption)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.OrgCustomLabels)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.OrgHeaderWrapper)(null)).ImportCustomDocumentLabels)).SyncRoot)).OT_FieldName)));
			this.ImportDocumentGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "OT_Position";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo2.ColumnName = "OT_Caption";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zDropEditColumnStyleInfo2.ColumnName = "OT_FieldName";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.ImportDocumentGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.ImportDocumentGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ImportDocumentGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ImportDocumentGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportDocumentGrid.GridId = "f84aff7d-cd0c-4fe6-847f-eb1dba3afa65";
			this.ImportDocumentGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ImportDocumentGrid.LayoutKey = "ImportDocumentGrid";
			this.ImportDocumentGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ImportDocumentGrid.Name = "ImportDocumentGrid";
			this.ImportDocumentGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(394, 231, true);
			this.ImportDocumentGrid.TabIndex = 0;
			// 
			// OrganisationDetailPlugInUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ImportDocumentGroupBox);
			this.Controls.Add(this.ExportDocumentGroupBox);
			this.Controls.Add(this.HideEXPExporterAddrCheckBox);
			this.Controls.Add(this.HideEXPBuyerAddrCheckBox);
			this.Controls.Add(this.DefaultEXPPaymentMethodDropEdit);
			this.Controls.Add(this.DefaultIMPPaymentMethodDropEdit);
			this.Controls.Add(this.DefaultExamModeDropEdit);
			this.Controls.Add(this.HideIMPSellerZHTAddrCheckBox);
			this.Controls.Add(this.HideIMPImporterAddrCheckBox);
			this.Controls.Add(this.HideIMPImporterZHTAddrCheckBox);
			this.Controls.Add(this.HideEXPExporterZHTAddrCheckBox);
			this.Controls.Add(this.HideEXPBuyerZHTAddrCheckBox);
			this.Name = "OrganisationDetailPlugInUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(912, 550, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DefaultExamModeDropEdit.ResumeLayout(true);
			this.DefaultExamModeDropEdit.PerformLayout();
			this.DefaultIMPPaymentMethodDropEdit.ResumeLayout(true);
			this.DefaultIMPPaymentMethodDropEdit.PerformLayout();
			this.DefaultEXPPaymentMethodDropEdit.ResumeLayout(true);
			this.DefaultEXPPaymentMethodDropEdit.PerformLayout();
			this.ExportDocumentGroupBox.ResumeLayout(false);
			this.ExportDocumentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExportDocumentGrid)).EndInit();
			this.ExportDocumentGrid.ResumeLayout(false);
			this.ExportDocumentGrid.PerformLayout();
			this.ImportDocumentGroupBox.ResumeLayout(false);
			this.ImportDocumentGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ImportDocumentGrid)).EndInit();
			this.ImportDocumentGrid.ResumeLayout(false);
			this.ImportDocumentGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox HideEXPBuyerZHTAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideEXPExporterZHTAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideIMPImporterZHTAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideIMPImporterAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideIMPSellerZHTAddrCheckBox;
		private ZArchitecture.GUI.ZDropEdit DefaultExamModeDropEdit;
		private ZArchitecture.GUI.ZDropEdit DefaultIMPPaymentMethodDropEdit;
		private ZArchitecture.GUI.ZDropEdit DefaultEXPPaymentMethodDropEdit;
		private ZArchitecture.GUI.ZCheckBox HideEXPExporterAddrCheckBox;
		private ZArchitecture.GUI.ZCheckBox HideEXPBuyerAddrCheckBox;
		private ZArchitecture.GUI.ZGroupBox ExportDocumentGroupBox;
		private ZArchitecture.ZGrid ExportDocumentGrid;
		private ZArchitecture.GUI.ZGroupBox ImportDocumentGroupBox;
		private ZArchitecture.ZGrid ImportDocumentGrid;
	}
}
