using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class Phase5GoodsItemExportToOpenTabUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.ExportToOpenSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.ExportToOpenGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ExportToOpenGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartialCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeclarationNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DeclarationLineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.WrapperCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConsignorIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ExportToOpenSplitContainer)).BeginInit();
			this.ExportToOpenSplitContainer.Panel1.SuspendLayout();
			this.ExportToOpenSplitContainer.Panel2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExportToOpenGrid)).BeginInit();
			this.ExportToOpenGrid.SuspendLayout();
			this.ExportToOpenSplitContainer.SuspendLayout();
			this.ExportToOpenGroupBox.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc);
			// 
			// ExportToOpenSplitContainer
			// 
			this.ExportToOpenSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportToOpenSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportToOpenSplitContainer.Name = "ExportToOpenSplitContainer";
			this.ExportToOpenSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// ExportToOpenSplitContainer.Panel1
			// 
			this.ExportToOpenSplitContainer.Panel1.Controls.Add(this.ExportToOpenGrid);
			// 
			// ExportToOpenSplitContainer.Panel2
			// 
			this.ExportToOpenSplitContainer.Panel2.Controls.Add(this.ExportToOpenGroupBox);
			this.ExportToOpenSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 589, true);
			this.ExportToOpenSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(206);
			this.ExportToOpenSplitContainer.TabIndex = 0;
			// 
			// ExportToOpenGrid
			// 
			this.ExportToOpenGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ExportToOpenGrid, "ExportToOpenList");
			this.ExportToOpenGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo.ColumnName = "CSI_Procedure";
			zDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ExportToOpenGrid.ColumnStyles.Add(zDropEditColumnStyleInfo);
			zCheckBoxColumnStyleInfo.ColumnName = "IsPartial";
			zCheckBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ExportToOpenGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo);
			zTextBoxColumnStyleInfo.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ExportToOpenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo);
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_ItemNumber";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ExportToOpenGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			zCheckBoxColumnStyleInfo2.ColumnName = "IsWrapper";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ExportToOpenGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ExportToOpenGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ExportToOpenGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportToOpenGrid.GridId = "142de25f-601d-4237-90d0-660438aa83u9";
			this.ExportToOpenGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ExportToOpenGrid.LayoutKey = "ExportToOpenGrid";
			this.ExportToOpenGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportToOpenGrid.Name = "ExportToOpenGrid";
			this.ExportToOpenGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 125, true);
			this.ExportToOpenGrid.TabIndex = 1;
			// 
			// ExportToOpenGroupBox
			// 
			this.ExportToOpenGroupBox.Controls.Add(this.DeclarationNoTextBox);
			this.ExportToOpenGroupBox.Controls.Add(this.PartialCheckBox);
			this.ExportToOpenGroupBox.Controls.Add(this.DeclarationTypeDropEdit);
			this.ExportToOpenGroupBox.Controls.Add(this.DeclarationLineNoCalcEdit);
			this.ExportToOpenGroupBox.Controls.Add(this.WrapperCheckBox);
			this.ExportToOpenGroupBox.Controls.Add(this.ConsignorIdTextBox);
			this.ExportToOpenGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportToOpenGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportToOpenGroupBox.Name = "ExportToOpenGroupBox";
			this.ExportToOpenGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 230, true);
			this.ExportToOpenGroupBox.TabIndex = 0;
			this.ExportToOpenGroupBox.TabStop = false;
			this.ExportToOpenGroupBox.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("3D00EDBC-0BB0-4833-954A-AD78837BD7U9", "Export To Open Details");
			// 
			// DeclarationTypeDropEdit
			// 
			this.DeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationTypeDropEdit, "ExportToOpenList.CSI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsExportToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).ExportToOpenList)).SyncRoot)).CSI_Procedure)));
			this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 28, true);
			this.DeclarationTypeDropEdit.Name = "DeclarationTypeDropEdit";
			this.DeclarationTypeDropEdit.PreBoundMaxLength = 1;
			this.DeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.DeclarationTypeDropEdit.TabIndex = 1;
			// 
			// PartialCheckBox
			// 
			this.BindingSource.SetBindingMember(this.PartialCheckBox, "ExportToOpenList.IsPartial");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsExportToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).ExportToOpenList)).SyncRoot)).IsPartial)));
			this.PartialCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.PartialCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PartialCheckBox.CaptionResourceString = null;
			this.PartialCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 56, true);
			this.PartialCheckBox.Name = "PartialCheckBox";
			this.PartialCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.PartialCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 20, true);
			this.PartialCheckBox.TabIndex = 2;
			this.PartialCheckBox.UseVisualStyleBackColor = true;
			// 
			// DeclarationNoTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarationNoTextBox, "ExportToOpenList.CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsExportToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).ExportToOpenList)).SyncRoot)).CSI_ReferenceNumber)));
			//this.DeclarationNoTextBox.CaptionResourceString = CaptionResourceString = Res.GetData("A2D14BF6-C365-4D9D-83A2-FBC96E7DB008", "Amount");
			this.DeclarationNoTextBox.CaptionResourceString = null;
			this.DeclarationNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 84, true);
			this.DeclarationNoTextBox.Name = "DeclarationNoTextBox";
			this.DeclarationNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.DeclarationNoTextBox.TabIndex = 3;
			// 
			// QuantityCalcEdit
			// 
			this.DeclarationLineNoCalcEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationLineNoCalcEdit, "ExportToOpenList.CSI_ItemNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.TR.NCTS.Business.NctsExportToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).ExportToOpenList)).SyncRoot)).CSI_ItemNumber)));
			this.DeclarationLineNoCalcEdit.CaptionResourceString = null;
			this.DeclarationLineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 112, true);
			this.DeclarationLineNoCalcEdit.Name = "QuantityCalcEdit";
			this.DeclarationLineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 20, true);
			this.DeclarationLineNoCalcEdit.TabIndex = 4;
			this.DeclarationLineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// WrapperCheckBox
			// 
			this.BindingSource.SetBindingMember(this.WrapperCheckBox, "ExportToOpenList.IsWrapper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.NCTS.Business.NctsExportToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).ExportToOpenList)).SyncRoot)).IsWrapper)));
			this.WrapperCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.WrapperCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.WrapperCheckBox.CaptionResourceString = null;
			this.WrapperCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 140, true);
			this.WrapperCheckBox.Name = "WrapperCheckBox";
			this.WrapperCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(127, 22, true);
			this.WrapperCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.WrapperCheckBox.UseVisualStyleBackColor = true;
			this.WrapperCheckBox.TabIndex = 5;
			// 
			// ConsignorIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsignorIdTextBox, "ExportToOpenList.CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.NCTS.Business.NctsExportToOpen)(((System.Collections.IList)(((Enterprise.Customs.TR.NCTS.Business.NctsDepartureCargoDesc)(null)).ExportToOpenList)).SyncRoot)).CSI_ReferenceNumber2)));
			this.ConsignorIdTextBox.CaptionResourceString = null;
			this.ConsignorIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 168, true);
			this.ConsignorIdTextBox.Name = "ConsignorIdTextBox";
			this.ConsignorIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.ConsignorIdTextBox.TabIndex = 6;
			// 
			// Phase5ExportToOpenUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExportToOpenSplitContainer);
			this.Name = "Phase5ExportToOpenUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(909, 589, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExportToOpenSplitContainer.Panel1.ResumeLayout(true);
			this.ExportToOpenSplitContainer.Panel2.ResumeLayout(true);
			((System.ComponentModel.ISupportInitialize)(this.ExportToOpenSplitContainer)).EndInit();
			this.ExportToOpenSplitContainer.ResumeLayout(true);
			this.ExportToOpenSplitContainer.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ExportToOpenGrid)).EndInit();
			this.ExportToOpenGrid.ResumeLayout(true);
			this.ExportToOpenGrid.PerformLayout();
			this.ExportToOpenGroupBox.ResumeLayout(true);
			this.ExportToOpenGroupBox.PerformLayout();
			this.ResumeLayout(true);
			this.PerformLayout();

		}

		public CargoWise.Windows.UI.KSplitContainer ExportToOpenSplitContainer;

		#endregion }

		public ZArchitecture.ZGrid ExportToOpenGrid;
		protected ZArchitecture.GUI.ZGroupBox ExportToOpenGroupBox;
		public ZArchitecture.GUI.ZDropEdit DeclarationTypeDropEdit;
		public ZArchitecture.ZTextBox DeclarationNoTextBox;
		public ZArchitecture.GUI.ZCheckBox PartialCheckBox;
		public ZArchitecture.ZCalcEdit DeclarationLineNoCalcEdit;
		public ZArchitecture.GUI.ZCheckBox WrapperCheckBox;
		public ZArchitecture.ZTextBox ConsignorIdTextBox;
	}
}

