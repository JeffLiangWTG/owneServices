using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class OrgSupplierPartControl
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
			if (disposing && components != null)
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.CI_CustomsUQDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TariffCodeFindBox = new Universal.GUI.TariffFindBox();
			this.ClassificationFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PercAlcoholCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.ProductCodesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ProductCodesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.DetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DetailTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.AttributesTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).BeginInit();
			this.AttributesLeftSplitContainer.Panel1.SuspendLayout();
			this.AttributesLeftSplitContainer.Panel2.SuspendLayout();
			this.AttributesLeftSplitContainer.SuspendLayout();
			this.Attributes1GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).BeginInit();
			this.Attributes1Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).BeginInit();
			this.AttributesRightSplitContainer.Panel1.SuspendLayout();
			this.AttributesRightSplitContainer.Panel2.SuspendLayout();
			this.AttributesRightSplitContainer.SuspendLayout();
			this.Attributes2GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).BeginInit();
			this.Attributes2Grid.SuspendLayout();
			this.Attributes3GroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).BeginInit();
			this.Attributes3Grid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).BeginInit();
			this.PivotGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CI_CustomsUQDropEdit.SuspendLayout();
			this.TariffCodeFindBox.SuspendLayout();
			this.ClassificationFindBox.SuspendLayout();
			this.ProductCodesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductCodesGrid)).BeginInit();
			this.ProductCodesGrid.SuspendLayout();
			this.DetailsLayoutPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// DetailTabControl
			// 
			this.DetailTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 273, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.DetailsLayoutPanel);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 246, true);
			this.DetailsTabPage.Controls.SetChildIndex(this.CI_UsageCommentTextBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.DetailsLayoutPanel, 0);
			// 
			// CI_UsageCommentTextBox
			// 
			this.CI_UsageCommentTextBox.TabIndex = 0;
			this.CI_UsageCommentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 20, true);
			// 
			// AttributesTabPage
			// 
			this.AttributesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 246, true);
			// 
			// AttributesLeftSplitContainer
			// 
			this.AttributesLeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 240, true);
			this.AttributesLeftSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(354);
			// 
			// Attributes1GroupBox
			// 
			this.Attributes1GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 240, true);
			// 
			// Attributes1Grid
			// 
			this.Attributes1Grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.Attributes1Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 221, true);
			// 
			// AttributesRightSplitContainer
			// 
			this.AttributesRightSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(556, 240, true);
			this.AttributesRightSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(261);
			// 
			// Attributes2GroupBox
			// 
			this.Attributes2GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 240, true);
			// 
			// Attributes2Grid
			// 
			this.Attributes2Grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.Attributes2Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 221, true);
			// 
			// Attributes3GroupBox
			// 
			this.Attributes3GroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 240, true);
			// 
			// Attributes3Grid
			// 
			this.Attributes3Grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.Attributes3Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 221, true);
			// 
			// PivotGrid
			// 
			this.PivotGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.PivotGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 130, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.SG.V4.Business.OrgSupplierPart);
			// 
			// CI_CustomsUQDropEdit
			// 
			this.CI_CustomsUQDropEdit.AllowDrop = true;
			this.CI_CustomsUQDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.CI_CustomsUQDropEdit, "PivotsForBinding.CI_CustomsUQ");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.SG.V4.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CustomsUQ)));
			this.CI_CustomsUQDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 191, true);
			this.CI_CustomsUQDropEdit.Name = "CI_CustomsUQDropEdit";
			this.CI_CustomsUQDropEdit.PreBoundMaxLength = 3;
			this.CI_CustomsUQDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(234, 20, true);
			this.CI_CustomsUQDropEdit.TabIndex = 4;
			// 
			// TariffCodeFindBox
			// 
			this.TariffCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TariffCodeFindBox, "PivotsForBinding.CI_TariffNum");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_TariffNum)));
			this.TariffCodeFindBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("A702DDA5-434D-4BD3-9667-53BE64777254", "Tariff");
			this.TariffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 31, true);
			this.TariffCodeFindBox.ModuleID = ModuleIDs.Customs.Universal.RefCusTariff;
			this.TariffCodeFindBox.Name = "TariffCodeFindBox";
			this.TariffCodeFindBox.PreBoundMaxLength = 10;
			this.TariffCodeFindBox.ShouldResize = true;
			this.TariffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 20, true);
			this.TariffCodeFindBox.TabIndex = 1;
			this.TariffCodeFindBox.TariffType = Universal.Constants.TariffTypes.HarmonizedSystem;
			// 
			// ClassificationFindBox
			// 
			this.ClassificationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClassificationFindBox, "PivotsForBinding.CI_CC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.SG.V4.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_CC)));
			this.ClassificationFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.ClassificationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 5, true);
			this.ClassificationFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.SG.SG4Classification;
			this.ClassificationFindBox.Name = "ClassificationFindBox";
			this.ClassificationFindBox.PreBoundMaxLength = 35;
			this.ClassificationFindBox.ShouldResize = true;
			this.ClassificationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 20, true);
			this.ClassificationFindBox.TabIndex = 0;
			// 
			// PercAlcoholCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PercAlcoholCalcEdit, "PivotsForBinding.CI_PercAlcohol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.SG.V4.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_PercAlcohol)));
			this.PercAlcoholCalcEdit.DecimalPlaces = 3;
			this.PercAlcoholCalcEdit.Decimals = 3;
			this.PercAlcoholCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 57, true);
			this.PercAlcoholCalcEdit.Name = "PercAlcoholCalcEdit";
			this.PercAlcoholCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.PercAlcoholCalcEdit.TabIndex = 2;
			this.PercAlcoholCalcEdit.Text = "0.000";
			this.PercAlcoholCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ProductCodesGroupBox
			// 
			this.ProductCodesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.ProductCodesGroupBox.AutoSize = true;
			this.ProductCodesGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("605f04eb-272e-49de-abaa-3ac1169f6994", "Product Codes");
			this.ProductCodesGroupBox.Controls.Add(this.ProductCodesGrid);
			this.ProductCodesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 83, true);
			this.ProductCodesGroupBox.Name = "ProductCodesGroupBox";
			this.ProductCodesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(287, 102, true);
			this.ProductCodesGroupBox.TabIndex = 3;
			this.ProductCodesGroupBox.TabStop = false;
			// 
			// ProductCodesGrid
			// 
			this.ProductCodesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ProductCodesGrid, "PivotsForBinding.ProductCodes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).ProductCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.ProductCode)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.CusClassPartPivot)(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).ProductCodes)).SyncRoot)).CY_Data)));
			this.ProductCodesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.Caption = "Product Code";
			zDropEditColumnStyleInfo2.ColumnName = "CY_Data";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(132);
			this.ProductCodesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ProductCodesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ProductCodesGrid.GridId = "314bb978-afc9-4fff-9aaa-bf04e841b156";
			this.ProductCodesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ProductCodesGrid.LayoutKey = "ProductCodesGrid";
			this.ProductCodesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ProductCodesGrid.Name = "ProductCodesGrid";
			this.ProductCodesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(281, 83, true);
			this.ProductCodesGrid.TabIndex = 0;
			// 
			// DetailsLayoutPanel
			// 
			this.DetailsLayoutPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.DetailsLayoutPanel.Controls.Add(this.PercAlcoholCalcEdit);
			this.DetailsLayoutPanel.Controls.Add(this.ClassificationFindBox);
			this.DetailsLayoutPanel.Controls.Add(this.TariffCodeFindBox);
			this.DetailsLayoutPanel.Controls.Add(this.CI_CustomsUQDropEdit);
			this.DetailsLayoutPanel.Controls.Add(this.ProductCodesGroupBox);
			this.DetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 29, true);
			this.DetailsLayoutPanel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 300, true);
			this.DetailsLayoutPanel.Name = "DetailsLayoutPanel";
			this.DetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(692, 212, true);
			this.DetailsLayoutPanel.TabIndex = 1;
			// 
			// OrgSupplierPartControl
			// 
			this.Name = "OrgSupplierPartControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(928, 403, true);
			this.DetailTabControl.ResumeLayout(false);
			this.DetailTabControl.PerformLayout();
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.AttributesTabPage.ResumeLayout(false);
			this.AttributesTabPage.PerformLayout();
			this.AttributesLeftSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesLeftSplitContainer)).EndInit();
			this.AttributesLeftSplitContainer.ResumeLayout(false);
			this.AttributesLeftSplitContainer.PerformLayout();
			this.Attributes1GroupBox.ResumeLayout(false);
			this.Attributes1GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes1Grid)).EndInit();
			this.Attributes1Grid.ResumeLayout(false);
			this.Attributes1Grid.PerformLayout();
			this.AttributesRightSplitContainer.Panel1.ResumeLayout(false);
			this.AttributesRightSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.AttributesRightSplitContainer)).EndInit();
			this.AttributesRightSplitContainer.ResumeLayout(false);
			this.AttributesRightSplitContainer.PerformLayout();
			this.Attributes2GroupBox.ResumeLayout(false);
			this.Attributes2GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes2Grid)).EndInit();
			this.Attributes2Grid.ResumeLayout(false);
			this.Attributes2Grid.PerformLayout();
			this.Attributes3GroupBox.ResumeLayout(false);
			this.Attributes3GroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.Attributes3Grid)).EndInit();
			this.Attributes3Grid.ResumeLayout(false);
			this.Attributes3Grid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PivotGrid)).EndInit();
			this.PivotGrid.ResumeLayout(false);
			this.PivotGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CI_CustomsUQDropEdit.ResumeLayout(true);
			this.CI_CustomsUQDropEdit.PerformLayout();
			this.TariffCodeFindBox.ResumeLayout(true);
			this.TariffCodeFindBox.PerformLayout();
			this.ClassificationFindBox.ResumeLayout(true);
			this.ClassificationFindBox.PerformLayout();
			this.ProductCodesGroupBox.ResumeLayout(false);
			this.ProductCodesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ProductCodesGrid)).EndInit();
			this.ProductCodesGrid.ResumeLayout(false);
			this.ProductCodesGrid.PerformLayout();
			this.DetailsLayoutPanel.ResumeLayout(false);
			this.DetailsLayoutPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZDropEdit CI_CustomsUQDropEdit;
		Universal.GUI.TariffFindBox TariffCodeFindBox;
		ZGuidFindBox ClassificationFindBox;
		private ZArchitecture.ZCalcEdit PercAlcoholCalcEdit;
		private ZGroupBox ProductCodesGroupBox;
		private ZArchitecture.ZGrid ProductCodesGrid;
		private ZPanel DetailsLayoutPanel;
	}
}
