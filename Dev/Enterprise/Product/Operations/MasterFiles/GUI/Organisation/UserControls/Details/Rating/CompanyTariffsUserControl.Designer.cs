using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class CompanyTariffsUserControl
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
		private void InitializeComponent()
		{
			this.CompanyTariffsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DistanceCalculationProviderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DistanceCalcMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DistanceCalcVersionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DistanceCalcProviderDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OrgRateTariffLevelControl = new Enterprise.MasterFiles.GUI.OrgRateTariffLevelControl();
			this.ARRatingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.OB_RateSecurityGroupBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefaultTariffLevel = new Enterprise.ZArchitecture.ZLabel();
			this.OM_ARAutoUpdateRatesBoundCheckEdit = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CompanyTariffsPanel.SuspendLayout();
			this.DistanceCalculationProviderGroupBox.SuspendLayout();
			this.DistanceCalcMethodDropEdit.SuspendLayout();
			this.DistanceCalcVersionDropEdit.SuspendLayout();
			this.DistanceCalcProviderDropEdit.SuspendLayout();
			this.OrgRateTariffLevelControl.SuspendLayout();
			this.ARRatingGroupBox.SuspendLayout();
			this.OB_RateSecurityGroupBoundDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// CompanyTariffsPanel
			// 
			this.CompanyTariffsPanel.Controls.Add(this.DistanceCalculationProviderGroupBox);
			this.CompanyTariffsPanel.Controls.Add(this.OrgRateTariffLevelControl);
			this.CompanyTariffsPanel.Controls.Add(this.ARRatingGroupBox);
			this.CompanyTariffsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CompanyTariffsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CompanyTariffsPanel.Name = "CompanyTariffsPanel";
			this.CompanyTariffsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 503, true);
			this.CompanyTariffsPanel.TabIndex = 15;
			// 
			// DistanceCalculationProviderGroupBox
			// 
			this.DistanceCalculationProviderGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
				  | System.Windows.Forms.AnchorStyles.Right)));
			this.DistanceCalculationProviderGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompanyTariffsUserControl|aff09fec-98a6-4b49-8a05-f15d3999a618", "Distance Calculation Configuration");
			this.DistanceCalculationProviderGroupBox.Controls.Add(this.DistanceCalcMethodDropEdit);
			this.DistanceCalculationProviderGroupBox.Controls.Add(this.DistanceCalcVersionDropEdit);
			this.DistanceCalculationProviderGroupBox.Controls.Add(this.DistanceCalcProviderDropEdit);
			this.DistanceCalculationProviderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 408, true);
			this.DistanceCalculationProviderGroupBox.Name = "DistanceCalculationProviderGroupBox";
			this.DistanceCalculationProviderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 89, true);
			this.DistanceCalculationProviderGroupBox.TabIndex = 12;
			this.DistanceCalculationProviderGroupBox.TabStop = false;
			// 
			// DistanceCalcMethodDropEdit
			// 
			this.DistanceCalcMethodDropEdit.AllowDrop = true;
			this.DistanceCalcMethodDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				  | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DistanceCalcMethodDropEdit, "MiscServ+OM_CMDistanceCalculationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMDistanceCalculationMethod)));
			this.DistanceCalcMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(318, 56, true);
			this.DistanceCalcMethodDropEdit.Name = "DistanceCalcMethodDropEdit";
			this.DistanceCalcMethodDropEdit.PreBoundMaxLength = 3;
			this.DistanceCalcMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(253, 20, true);
			this.DistanceCalcMethodDropEdit.TabIndex = 2;
			// 
			// DistanceCalcVersionDropEdit
			// 
			this.DistanceCalcVersionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DistanceCalcVersionDropEdit, "MiscServ+OM_CMDistanceCalculationVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMDistanceCalculationVersion)));
			this.DistanceCalcVersionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 56, true);
			this.DistanceCalcVersionDropEdit.Name = "DistanceCalcVersionDropEdit";
			this.DistanceCalcVersionDropEdit.PreBoundMaxLength = 3;
			this.DistanceCalcVersionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.DistanceCalcVersionDropEdit.TabIndex = 1;
			// 
			// DistanceCalcProviderDropEdit
			// 
			this.DistanceCalcProviderDropEdit.AllowDrop = true;
			this.DistanceCalcProviderDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				  | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.DistanceCalcProviderDropEdit, "MiscServ+OM_CMDistanceCalculationProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_CMDistanceCalculationProvider)));
			this.DistanceCalcProviderDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 19, true);
			this.DistanceCalcProviderDropEdit.Name = "DistanceCalcProviderDropEdit";
			this.DistanceCalcProviderDropEdit.PreBoundMaxLength = 3;
			this.DistanceCalcProviderDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 20, true);
			this.DistanceCalcProviderDropEdit.TabIndex = 0;
			// 
			// OrgRateTariffLevelControl
			// 
			this.OrgRateTariffLevelControl.AllowDrop = true;
			this.OrgRateTariffLevelControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				  | System.Windows.Forms.AnchorStyles.Left)
				  | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgRateTariffLevelControl, ".");
			this.OrgRateTariffLevelControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 54, true);
			this.OrgRateTariffLevelControl.Name = "OrgRateTariffLevelControl";
			this.OrgRateTariffLevelControl.ReadOnly = false;
			this.OrgRateTariffLevelControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 348, true);
			this.OrgRateTariffLevelControl.TabIndex = 11;
			// 
			// ARRatingGroupBox
			// 
			this.ARRatingGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
				  | System.Windows.Forms.AnchorStyles.Right)));
			this.ARRatingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompanyTariffsUserControl|82ca7405-271e-457f-8b01-e020bb13c579", "Auto-Rating");
			this.ARRatingGroupBox.Controls.Add(this.OB_RateSecurityGroupBoundDropEdit);
			this.ARRatingGroupBox.Controls.Add(this.DefaultTariffLevel);
			this.ARRatingGroupBox.Controls.Add(this.OM_ARAutoUpdateRatesBoundCheckEdit);
			this.ARRatingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ARRatingGroupBox.Name = "ARRatingGroupBox";
			this.ARRatingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(577, 48, true);
			this.ARRatingGroupBox.TabIndex = 1;
			this.ARRatingGroupBox.TabStop = false;
			// 
			// OB_RateSecurityGroupBoundDropEdit
			// 
			this.OB_RateSecurityGroupBoundDropEdit.AllowDrop = true;
			this.OB_RateSecurityGroupBoundDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OB_RateSecurityGroupBoundDropEdit, "CompanyData.OB_RateSecurityGroup");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).CompanyData.OB_RateSecurityGroup)));
			this.OB_RateSecurityGroupBoundDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("809d7bc1-b087-46f3-a4dc-49fa5346689a", "Security", "Rates\' Security", "Rates\' Security can be configured in Registry -> Organizations -> Code Lists -> Rates\' Security.");
			this.OB_RateSecurityGroupBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(339, 14, true);
			this.OB_RateSecurityGroupBoundDropEdit.Name = "OB_RateSecurityGroupBoundDropEdit";
			this.OB_RateSecurityGroupBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.OB_RateSecurityGroupBoundDropEdit.TabIndex = 5;
			// 
			// DefaultTariffLevel
			// 
			this.DefaultTariffLevel.AutoSize = true;
			this.DefaultTariffLevel.Font = Enterprise.ZArchitecture.Core.OFont.GetFont();
			this.DefaultTariffLevel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(213, 18, true);
			this.DefaultTariffLevel.Name = "DefaultTariffLevel";
			this.DefaultTariffLevel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.DefaultTariffLevel.TabIndex = 6;
			// 
			// OM_ARAutoUpdateRatesBoundCheckEdit
			// 
			this.OM_ARAutoUpdateRatesBoundCheckEdit.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OM_ARAutoUpdateRatesBoundCheckEdit, "MiscServ.OM_ARAutoUpdateRates");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_ARAutoUpdateRates)));
			this.OM_ARAutoUpdateRatesBoundCheckEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("CompanyTariffsUserControl|a8af56d8-4eae-490c-afab-20a4a4647ec5", "Include in Automatic Rate Updates");
			this.OM_ARAutoUpdateRatesBoundCheckEdit.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OM_ARAutoUpdateRatesBoundCheckEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.OM_ARAutoUpdateRatesBoundCheckEdit.Name = "OM_ARAutoUpdateRatesBoundCheckEdit";
			this.OM_ARAutoUpdateRatesBoundCheckEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 17, true);
			this.OM_ARAutoUpdateRatesBoundCheckEdit.TabIndex = 4;
			// 
			// CompanyTariffsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CompanyTariffsPanel);
			this.Name = "CompanyTariffsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(583, 503, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CompanyTariffsPanel.ResumeLayout(false);
			this.CompanyTariffsPanel.PerformLayout();
			this.DistanceCalculationProviderGroupBox.ResumeLayout(false);
			this.DistanceCalculationProviderGroupBox.PerformLayout();
			this.DistanceCalcMethodDropEdit.ResumeLayout(true);
			this.DistanceCalcMethodDropEdit.PerformLayout();
			this.DistanceCalcVersionDropEdit.ResumeLayout(true);
			this.DistanceCalcVersionDropEdit.PerformLayout();
			this.DistanceCalcProviderDropEdit.ResumeLayout(true);
			this.DistanceCalcProviderDropEdit.PerformLayout();
			this.OrgRateTariffLevelControl.ResumeLayout(true);
			this.OrgRateTariffLevelControl.PerformLayout();
			this.ARRatingGroupBox.ResumeLayout(false);
			this.ARRatingGroupBox.PerformLayout();
			this.OB_RateSecurityGroupBoundDropEdit.ResumeLayout(true);
			this.OB_RateSecurityGroupBoundDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel CompanyTariffsPanel;
		private OrgRateTariffLevelControl OrgRateTariffLevelControl;
		private ZGroupBox ARRatingGroupBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox OM_ARAutoUpdateRatesBoundCheckEdit;
		private ZGroupBox DistanceCalculationProviderGroupBox;
		private ZDropEdit DistanceCalcProviderDropEdit;
		private ZDropEdit DistanceCalcVersionDropEdit;
		private ZDropEdit DistanceCalcMethodDropEdit;
		private ZDropEdit OB_RateSecurityGroupBoundDropEdit;
		internal Enterprise.ZArchitecture.ZLabel DefaultTariffLevel;
	}
}
