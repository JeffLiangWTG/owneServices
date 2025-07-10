namespace Enterprise.MasterFiles.GUI
{
	public partial class RefPacksForm
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
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.RP_ConversionFactorBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RP_OH_SupplierBoundGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RP_CustomsPackBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RP_CommercialPackBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RP_TypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.SaveButton = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.zGuidFindBoxCountry = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RP_OH_SupplierBoundGuidFindBox.SuspendLayout();
			this.RP_CustomsPackBoundDropEdit.SuspendLayout();
			this.RP_CommercialPackBoundDropEdit.SuspendLayout();
			this.RP_TypeBoundDropEdit.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.zStmNoteTabPage1.SuspendLayout();
			this.SaveButton.SuspendLayout();
			this.zGuidFindBoxCountry.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 242, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(295);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(295);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.BaseRefPacks);
			// 
			// RP_ConversionFactorBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RP_ConversionFactorBoundCalcEdit, "RP_ConversionFactor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.BaseRefPacks)(null)).RP_ConversionFactor)));
			this.RP_ConversionFactorBoundCalcEdit.DecimalPlaces = 2;
			this.RP_ConversionFactorBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 108, true);
			this.RP_ConversionFactorBoundCalcEdit.Name = "RP_ConversionFactorBoundCalcEdit";
			this.RP_ConversionFactorBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			this.RP_ConversionFactorBoundCalcEdit.TabIndex = 3;
			this.RP_ConversionFactorBoundCalcEdit.Text = "0.000000000";
			this.RP_ConversionFactorBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RP_OH_SupplierBoundGuidFindBox
			// 
			this.RP_OH_SupplierBoundGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RP_OH_SupplierBoundGuidFindBox, "RP_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.BaseRefPacks)(null)).RP_OH_Supplier)));
			this.RP_OH_SupplierBoundGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 78, true);
			this.RP_OH_SupplierBoundGuidFindBox.Name = "RP_OH_SupplierBoundGuidFindBox";
			this.RP_OH_SupplierBoundGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.RP_OH_SupplierBoundGuidFindBox.TabIndex = 2;
			// 
			// RP_TypeBoundDropEdit
			// 
			this.RP_TypeBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RP_TypeBoundDropEdit, "RP_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.BaseRefPacks)(null)).RP_Type)));
			this.RP_TypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 48, true);
			this.RP_TypeBoundDropEdit.Name = "RP_TypeBoundDropEdit";
			this.RP_TypeBoundDropEdit.PreBoundMaxLength = 3;
			this.RP_TypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.RP_TypeBoundDropEdit.TabIndex = 1;
			// 
			// RP_CustomsPackBoundDropEdit
			// 
			this.RP_CustomsPackBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RP_CustomsPackBoundDropEdit, "RP_CustomsPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.BaseRefPacks)(null)).RP_CustomsPack)));
			this.RP_CustomsPackBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 138, true);
			this.RP_CustomsPackBoundDropEdit.Name = "RP_CustomsPackBoundDropEdit";
			this.RP_CustomsPackBoundDropEdit.ShouldResizeByMaxLength = false;
			this.RP_CustomsPackBoundDropEdit.PreBoundMaxLength = 6;
			this.RP_CustomsPackBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.RP_CustomsPackBoundDropEdit.TabIndex = 4;
			// 
			// RP_CommercialPackBoundDropEdit
			// 
			this.RP_CommercialPackBoundDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RP_CommercialPackBoundDropEdit, "RP_CommercialPack");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.BaseRefPacks)(null)).RP_CommercialPack)));
			this.RP_CommercialPackBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 168, true);
			this.RP_CommercialPackBoundDropEdit.Name = "RP_CommercialPackBoundDropEdit";
			this.RP_CommercialPackBoundDropEdit.ShouldResizeByMaxLength = false;
			this.RP_CommercialPackBoundDropEdit.PreBoundMaxLength = 6;
			this.RP_CommercialPackBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.RP_CommercialPackBoundDropEdit.TabIndex = 5;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.MainTabControl.Controls.Add(this.zLogsTabPage1);
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 237, true);
			this.MainTabControl.TabIndex = 5;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefPacksForm|48c4be23-f064-4e76-a0d8-95aa244d7cb2", "Packs");
			this.MainTabPage.Controls.Add(this.zGuidFindBoxCountry);
			this.MainTabPage.Controls.Add(this.RP_ConversionFactorBoundCalcEdit);
			this.MainTabPage.Controls.Add(this.RP_OH_SupplierBoundGuidFindBox);
			this.MainTabPage.Controls.Add(this.RP_TypeBoundDropEdit);
			this.MainTabPage.Controls.Add(this.RP_CustomsPackBoundDropEdit);
			this.MainTabPage.Controls.Add(this.RP_CommercialPackBoundDropEdit);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(582, 210, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(582, 180, true);
			this.zStmNoteTabPage1.TabIndex = 1;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(582, 180, true);
			this.zLogsTabPage1.TabIndex = 2;
			// 
			// SaveButton
			// 
			this.SaveButton.AllowDrop = true;
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(285, 213, true);
			this.SaveButton.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
			this.SaveButton.TabIndex = 6;
			// 
			// zGuidFindBoxCountry
			// 
			this.zGuidFindBoxCountry.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBoxCountry, "RP_CustomsCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.BaseRefPacks)(null)).RP_CustomsCountry)));
			this.zGuidFindBoxCountry.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 18, true);
			this.zGuidFindBoxCountry.Name = "zGuidFindBoxCountry";
			this.zGuidFindBoxCountry.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.zGuidFindBoxCountry.TabIndex = 0;
			// 
			// RefPacksForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefPacksForm|f4673e4a-577f-4428-bfac-8514dbdf79de", "Packs Conversion");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(590, 296, true);
			this.Controls.Add(this.SaveButton);
			this.Controls.Add(this.MainTabControl);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.BaseRefPacks);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 326, true);
			this.Name = "RefPacksForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainTabControl, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RP_OH_SupplierBoundGuidFindBox.ResumeLayout(true);
			this.RP_OH_SupplierBoundGuidFindBox.PerformLayout();
			this.RP_CustomsPackBoundDropEdit.ResumeLayout(true);
			this.RP_CustomsPackBoundDropEdit.PerformLayout();
			this.RP_CommercialPackBoundDropEdit.ResumeLayout(true);
			this.RP_CommercialPackBoundDropEdit.PerformLayout();
			this.RP_TypeBoundDropEdit.ResumeLayout(true);
			this.RP_TypeBoundDropEdit.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.zStmNoteTabPage1.ResumeLayout(false);
			this.zStmNoteTabPage1.PerformLayout();
			this.SaveButton.ResumeLayout(true);
			this.SaveButton.PerformLayout();
			this.zGuidFindBoxCountry.ResumeLayout(true);
			this.zGuidFindBoxCountry.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

		protected internal Enterprise.ZArchitecture.ZCalcEdit RP_ConversionFactorBoundCalcEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZGuidFindBox RP_OH_SupplierBoundGuidFindBox;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit RP_CustomsPackBoundDropEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit RP_CommercialPackBoundDropEdit;
		protected internal Enterprise.ZArchitecture.GUI.ZDropEdit RP_TypeBoundDropEdit;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl MainTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage MainTabPage;
		private Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl SaveButton;
		protected internal Enterprise.ZArchitecture.GUI.ZCodeFindBox zGuidFindBoxCountry;
	}
}
