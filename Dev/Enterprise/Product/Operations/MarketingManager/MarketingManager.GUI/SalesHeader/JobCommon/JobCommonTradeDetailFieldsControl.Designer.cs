namespace Enterprise.MarketingManager.GUI
{
	partial class JobCommonTradeDetailFieldsControl
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
			this.isPeriodOfActivityOverriddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.isIndustryVerticalOverriddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.customFieldsControl = new Enterprise.MarketingManager.GUI.SalesProductCustomFieldsControl();
			this.OverallPeriodOfActivityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PA_OH_ServiceProviderGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PA_OH_ControllingAgentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PA_OH_CompetitorGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OverallIndustryVerticalDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ViewProspectPeriodsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.customFieldsControl.SuspendLayout();
			this.OverallPeriodOfActivityDropEdit.SuspendLayout();
			this.PA_OH_ServiceProviderGuidFindBox.SuspendLayout();
			this.PA_OH_ControllingAgentGuidFindBox.SuspendLayout();
			this.PA_OH_CompetitorGuidFindBox.SuspendLayout();
			this.OverallIndustryVerticalDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgTradeDetail);
			// 
			// isPeriodOfActivityOverriddenCheckBox
			// 
			this.isPeriodOfActivityOverriddenCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isPeriodOfActivityOverriddenCheckBox, "ProspectDetail.IsPeriodOfActivityOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgTradeDetail)(null)).ProspectDetail.IsPeriodOfActivityOverridden)));
			this.isPeriodOfActivityOverriddenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isPeriodOfActivityOverriddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 24, true);
			this.isPeriodOfActivityOverriddenCheckBox.Name = "isPeriodOfActivityOverriddenCheckBox";
			this.isPeriodOfActivityOverriddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.isPeriodOfActivityOverriddenCheckBox.TabIndex = 3;
			this.isPeriodOfActivityOverriddenCheckBox.UseVisualStyleBackColor = true;
			// 
			// isIndustryVerticalOverriddenCheckBox
			// 
			this.isIndustryVerticalOverriddenCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isIndustryVerticalOverriddenCheckBox, "ProspectDetail.IsIndustryVerticalOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgTradeDetail)(null)).ProspectDetail.IsIndustryVerticalOverridden)));
			this.isIndustryVerticalOverriddenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isIndustryVerticalOverriddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(349, 5, true);
			this.isIndustryVerticalOverriddenCheckBox.Name = "isIndustryVerticalOverriddenCheckBox";
			this.isIndustryVerticalOverriddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.isIndustryVerticalOverriddenCheckBox.TabIndex = 1;
			this.isIndustryVerticalOverriddenCheckBox.UseVisualStyleBackColor = true;
			// 
			// customFieldsControl
			// 
			this.customFieldsControl.AllowDrop = true;
			this.customFieldsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.customFieldsControl, "CustomProductFieldColumns");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.EntityFramework.IDynamicBusinessObject)(((Enterprise.MasterFiles.Business.OrgTradeDetail)(null)).CustomProductFieldColumns)));
			this.customFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(421, 25, true);
			this.customFieldsControl.Name = "customFieldsControl";
			this.customFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 85, true);
			this.customFieldsControl.TabIndex = 8;
			// 
			// OverallPeriodOfActivityDropEdit
			// 
			this.OverallPeriodOfActivityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverallPeriodOfActivityDropEdit, "ProspectDetail.OverallPeriodOfActivity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgTradeDetail)(null)).ProspectDetail.OverallPeriodOfActivity)));
			this.OverallPeriodOfActivityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 24, true);
			this.OverallPeriodOfActivityDropEdit.Name = "OverallPeriodOfActivityDropEdit";
			this.OverallPeriodOfActivityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OverallPeriodOfActivityDropEdit.TabIndex = 2;
			// 
			// PA_OH_ServiceProviderGuidFindBox
			// 
			this.PA_OH_ServiceProviderGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PA_OH_ServiceProviderGuidFindBox, "ProspectDetail.PAP_OH_ServiceProvider");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgTradeDetail)(null)).ProspectDetail.PAP_OH_ServiceProvider)));
			this.PA_OH_ServiceProviderGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.PA_OH_ServiceProviderGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 90, true);
			this.PA_OH_ServiceProviderGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.PA_OH_ServiceProviderGuidFindBox.Name = "PA_OH_ServiceProviderGuidFindBox";
			this.PA_OH_ServiceProviderGuidFindBox.ShouldResize = true;
			this.PA_OH_ServiceProviderGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PA_OH_ServiceProviderGuidFindBox.TabIndex = 6;
			// 
			// PA_OH_ControllingAgentGuidFindBox
			// 
			this.PA_OH_ControllingAgentGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PA_OH_ControllingAgentGuidFindBox, "ProspectDetail.PAP_OH_ControllingAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgTradeDetail)(null)).ProspectDetail.PAP_OH_ControllingAgent)));
			this.PA_OH_ControllingAgentGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.PA_OH_ControllingAgentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 68, true);
			this.PA_OH_ControllingAgentGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.PA_OH_ControllingAgentGuidFindBox.Name = "PA_OH_ControllingAgentGuidFindBox";
			this.PA_OH_ControllingAgentGuidFindBox.ShouldResize = true;
			this.PA_OH_ControllingAgentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PA_OH_ControllingAgentGuidFindBox.TabIndex = 5;
			// 
			// PA_OH_CompetitorGuidFindBox
			// 
			this.PA_OH_CompetitorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PA_OH_CompetitorGuidFindBox, "ProspectDetail.PAP_OH_Competitor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OrgTradeDetail)(null)).ProspectDetail.PAP_OH_Competitor)));
			this.PA_OH_CompetitorGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.PA_OH_CompetitorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 46, true);
			this.PA_OH_CompetitorGuidFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.PA_OH_CompetitorGuidFindBox.Name = "PA_OH_CompetitorGuidFindBox";
			this.PA_OH_CompetitorGuidFindBox.ShouldResize = true;
			this.PA_OH_CompetitorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PA_OH_CompetitorGuidFindBox.TabIndex = 4;
			// 
			// OverallIndustryVerticalDropEdit
			// 
			this.OverallIndustryVerticalDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverallIndustryVerticalDropEdit, "ProspectDetail.OverallIndustryVertical");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgTradeDetail)(null)).ProspectDetail.OverallIndustryVertical)));
			this.OverallIndustryVerticalDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 2, true);
			this.OverallIndustryVerticalDropEdit.Name = "OverallIndustryVerticalDropEdit";
			this.OverallIndustryVerticalDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.OverallIndustryVerticalDropEdit.TabIndex = 0;
			// 
			// ViewProspectPeriodsButton
			// 
			this.ViewProspectPeriodsButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("534189b3-08fc-4493-9d3d-0d753e86f71d", "Estimate Value History");
			this.ViewProspectPeriodsButton.IsCaptionOverridden = false;
			this.ViewProspectPeriodsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 2, true);
			this.ViewProspectPeriodsButton.Name = "ViewProspectPeriodsButton";
			this.ViewProspectPeriodsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ViewProspectPeriodsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(161, 22, true);
			this.ViewProspectPeriodsButton.TabIndex = 7;
			this.ViewProspectPeriodsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ViewProspectPeriodsButton.ToolTipCaption = null;
			this.ViewProspectPeriodsButton.UseVisualStyleBackColor = true;
			this.ViewProspectPeriodsButton.Click += new System.EventHandler(this.ViewProspectPeriodsButton_Click);
			// 
			// JobCommonTradeDetailFieldsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ViewProspectPeriodsButton);
			this.Controls.Add(this.isPeriodOfActivityOverriddenCheckBox);
			this.Controls.Add(this.isIndustryVerticalOverriddenCheckBox);
			this.Controls.Add(this.customFieldsControl);
			this.Controls.Add(this.OverallPeriodOfActivityDropEdit);
			this.Controls.Add(this.PA_OH_ServiceProviderGuidFindBox);
			this.Controls.Add(this.PA_OH_ControllingAgentGuidFindBox);
			this.Controls.Add(this.PA_OH_CompetitorGuidFindBox);
			this.Controls.Add(this.OverallIndustryVerticalDropEdit);
			this.Name = "JobCommonTradeDetailFieldsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(861, 113, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.customFieldsControl.ResumeLayout(true);
			this.customFieldsControl.PerformLayout();
			this.OverallPeriodOfActivityDropEdit.ResumeLayout(true);
			this.OverallPeriodOfActivityDropEdit.PerformLayout();
			this.PA_OH_ServiceProviderGuidFindBox.ResumeLayout(true);
			this.PA_OH_ServiceProviderGuidFindBox.PerformLayout();
			this.PA_OH_ControllingAgentGuidFindBox.ResumeLayout(true);
			this.PA_OH_ControllingAgentGuidFindBox.PerformLayout();
			this.PA_OH_CompetitorGuidFindBox.ResumeLayout(true);
			this.PA_OH_CompetitorGuidFindBox.PerformLayout();
			this.OverallIndustryVerticalDropEdit.ResumeLayout(true);
			this.OverallIndustryVerticalDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZCheckBox isPeriodOfActivityOverriddenCheckBox;
		private ZArchitecture.GUI.ZCheckBox isIndustryVerticalOverriddenCheckBox;
		private SalesProductCustomFieldsControl customFieldsControl;
		private ZArchitecture.GUI.ZDropEdit OverallPeriodOfActivityDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox PA_OH_ServiceProviderGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox PA_OH_ControllingAgentGuidFindBox;
		private ZArchitecture.GUI.ZGuidFindBox PA_OH_CompetitorGuidFindBox;
		private ZArchitecture.GUI.ZDropEdit OverallIndustryVerticalDropEdit;
		private ZArchitecture.GUI.ZButton ViewProspectPeriodsButton;
	}
}
