namespace Enterprise.MasterFiles.GUI
{
	partial class OverallStaffCommissionRulePreviewPane
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.typeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GroupPkGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.CompanyPkGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.ruleApplicabilityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.SubModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ServiceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProductDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.newAgreementDefaultsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommissionTriggerTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommissionBasisDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.typeGroupBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.GroupPkGuidDropEdit.SuspendLayout();
			this.CompanyPkGuidFindBox.SuspendLayout();
			this.ruleApplicabilityGroupBox.SuspendLayout();
			this.EndDateEdit.SuspendLayout();
			this.StartDateEdit.SuspendLayout();
			this.SubModuleDropEdit.SuspendLayout();
			this.ServiceDropEdit.SuspendLayout();
			this.ProductDropEdit.SuspendLayout();
			this.newAgreementDefaultsGroupBox.SuspendLayout();
			this.CommissionTriggerTypeDropEdit.SuspendLayout();
			this.CommissionBasisDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OverallStaffCommissionRule);
			// 
			// typeGroupBox
			// 
			this.typeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.typeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("40dea0ee-69f3-42d5-adf0-5e8391c19d58", "Rule Type");
			this.typeGroupBox.Controls.Add(this.StatusDropEdit);
			this.typeGroupBox.Controls.Add(this.GroupPkGuidDropEdit);
			this.typeGroupBox.Controls.Add(this.CompanyPkGuidFindBox);
			this.typeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.typeGroupBox.Name = "typeGroupBox";
			this.typeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 93, true);
			this.typeGroupBox.TabIndex = 0;
			this.typeGroupBox.TabStop = false;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.StatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).Status)));
			this.StatusDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 66, true);
			this.StatusDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.StatusDropEdit.TabIndex = 2;
			// 
			// GroupPkGuidDropEdit
			// 
			this.GroupPkGuidDropEdit.AllowDrop = true;
			this.GroupPkGuidDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GroupPkGuidDropEdit, "GroupPk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).GroupPk)));
			this.GroupPkGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 18, true);
			this.GroupPkGuidDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.GroupPkGuidDropEdit.Name = "GroupPkGuidDropEdit";
			this.GroupPkGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.GroupPkGuidDropEdit.TabIndex = 0;
			// 
			// CompanyPkGuidFindBox
			// 
			this.CompanyPkGuidFindBox.AllowDrop = true;
			this.CompanyPkGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CompanyPkGuidFindBox, "CompanyPk");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).CompanyPk)));
			this.CompanyPkGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 42, true);
			this.CompanyPkGuidFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.CompanyPkGuidFindBox.Name = "CompanyPkGuidFindBox";
			this.CompanyPkGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.CompanyPkGuidFindBox.TabIndex = 1;
			// 
			// ruleApplicabilityGroupBox
			// 
			this.ruleApplicabilityGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ruleApplicabilityGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c7120ae6-8aad-4a46-9132-b91c01dba196", "Rule Applicability");
			this.ruleApplicabilityGroupBox.Controls.Add(this.EndDateEdit);
			this.ruleApplicabilityGroupBox.Controls.Add(this.StartDateEdit);
			this.ruleApplicabilityGroupBox.Controls.Add(this.SubModuleDropEdit);
			this.ruleApplicabilityGroupBox.Controls.Add(this.ServiceDropEdit);
			this.ruleApplicabilityGroupBox.Controls.Add(this.ProductDropEdit);
			this.ruleApplicabilityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 99, true);
			this.ruleApplicabilityGroupBox.Name = "ruleApplicabilityGroupBox";
			this.ruleApplicabilityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 117, true);
			this.ruleApplicabilityGroupBox.TabIndex = 1;
			this.ruleApplicabilityGroupBox.TabStop = false;
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).EndDate)));
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 90, true);
			this.EndDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 4;
			// 
			// StartDateEdit
			// 
			this.StartDateEdit.AllowDrop = true;
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).StartDate)));
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 90, true);
			this.StartDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 3;
			// 
			// SubModuleDropEdit
			// 
			this.SubModuleDropEdit.AllowDrop = true;
			this.SubModuleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SubModuleDropEdit, "SubModule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).SubModule)));
			this.SubModuleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SubModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 66, true);
			this.SubModuleDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.SubModuleDropEdit.Name = "SubModuleDropEdit";
			this.SubModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.SubModuleDropEdit.TabIndex = 2;
			// 
			// ServiceDropEdit
			// 
			this.ServiceDropEdit.AllowDrop = true;
			this.ServiceDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ServiceDropEdit, "Service");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).Service)));
			this.ServiceDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ServiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 42, true);
			this.ServiceDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ServiceDropEdit.Name = "ServiceDropEdit";
			this.ServiceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.ServiceDropEdit.TabIndex = 1;
			// 
			// ProductDropEdit
			// 
			this.ProductDropEdit.AllowDrop = true;
			this.ProductDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ProductDropEdit, "Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).Product)));
			this.ProductDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ProductDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 18, true);
			this.ProductDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ProductDropEdit.Name = "ProductDropEdit";
			this.ProductDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.ProductDropEdit.TabIndex = 0;
			// 
			// newAgreementDefaultsGroupBox
			// 
			this.newAgreementDefaultsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.newAgreementDefaultsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6b556b6f-b661-449f-8eae-aa4fcfe47b7b", "New Agreement Defaults");
			this.newAgreementDefaultsGroupBox.Controls.Add(this.CommissionTriggerTypeDropEdit);
			this.newAgreementDefaultsGroupBox.Controls.Add(this.CommissionBasisDropEdit);
			this.newAgreementDefaultsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 222, true);
			this.newAgreementDefaultsGroupBox.Name = "newAgreementDefaultsGroupBox";
			this.newAgreementDefaultsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 70, true);
			this.newAgreementDefaultsGroupBox.TabIndex = 2;
			this.newAgreementDefaultsGroupBox.TabStop = false;
			// 
			// CommissionTriggerTypeDropEdit
			// 
			this.CommissionTriggerTypeDropEdit.AllowDrop = true;
			this.CommissionTriggerTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CommissionTriggerTypeDropEdit, "CommissionTriggerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).CommissionTriggerType)));
			this.CommissionTriggerTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2c6638ae-9089-4502-9fdd-9c3bffdb68f0", "Trigger", "Effective Trigger", "");
			this.CommissionTriggerTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 42, true);
			this.CommissionTriggerTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.CommissionTriggerTypeDropEdit.Name = "CommissionTriggerTypeDropEdit";
			this.CommissionTriggerTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.CommissionTriggerTypeDropEdit.TabIndex = 1;
			// 
			// CommissionBasisDropEdit
			// 
			this.CommissionBasisDropEdit.AllowDrop = true;
			this.CommissionBasisDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CommissionBasisDropEdit, "CommissionBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OverallStaffCommissionRule)(null)).CommissionBasis)));
			this.CommissionBasisDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a758c0bb-5098-4ea1-ba76-50c5fa7be7d2", "Basis");
			this.CommissionBasisDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 18, true);
			this.CommissionBasisDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.CommissionBasisDropEdit.Name = "CommissionBasisDropEdit";
			this.CommissionBasisDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.CommissionBasisDropEdit.TabIndex = 0;
			// 
			// OverallStaffCommissionRulePreviewPane
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.newAgreementDefaultsGroupBox);
			this.Controls.Add(this.ruleApplicabilityGroupBox);
			this.Controls.Add(this.typeGroupBox);
			this.Name = "OverallStaffCommissionRulePreviewPane";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 292, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.typeGroupBox.ResumeLayout(false);
			this.typeGroupBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.GroupPkGuidDropEdit.ResumeLayout(true);
			this.GroupPkGuidDropEdit.PerformLayout();
			this.CompanyPkGuidFindBox.ResumeLayout(true);
			this.CompanyPkGuidFindBox.PerformLayout();
			this.ruleApplicabilityGroupBox.ResumeLayout(false);
			this.ruleApplicabilityGroupBox.PerformLayout();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.StartDateEdit.ResumeLayout(true);
			this.StartDateEdit.PerformLayout();
			this.SubModuleDropEdit.ResumeLayout(true);
			this.SubModuleDropEdit.PerformLayout();
			this.ServiceDropEdit.ResumeLayout(true);
			this.ServiceDropEdit.PerformLayout();
			this.ProductDropEdit.ResumeLayout(true);
			this.ProductDropEdit.PerformLayout();
			this.newAgreementDefaultsGroupBox.ResumeLayout(false);
			this.newAgreementDefaultsGroupBox.PerformLayout();
			this.CommissionTriggerTypeDropEdit.ResumeLayout(true);
			this.CommissionTriggerTypeDropEdit.PerformLayout();
			this.CommissionBasisDropEdit.ResumeLayout(true);
			this.CommissionBasisDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox typeGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox CompanyPkGuidFindBox;
		private ZArchitecture.GUI.ZGuidDropEdit GroupPkGuidDropEdit;
		private ZArchitecture.GUI.ZDropEdit StatusDropEdit;
		private ZArchitecture.GUI.ZGroupBox ruleApplicabilityGroupBox;
		private ZArchitecture.GUI.ZDateEdit EndDateEdit;
		private ZArchitecture.GUI.ZDateEdit StartDateEdit;
		private ZArchitecture.GUI.ZDropEdit SubModuleDropEdit;
		private ZArchitecture.GUI.ZDropEdit ServiceDropEdit;
		private ZArchitecture.GUI.ZDropEdit ProductDropEdit;
		private ZArchitecture.GUI.ZGroupBox newAgreementDefaultsGroupBox;
		private ZArchitecture.GUI.ZDropEdit CommissionBasisDropEdit;
		private ZArchitecture.GUI.ZDropEdit CommissionTriggerTypeDropEdit;
	}
}
