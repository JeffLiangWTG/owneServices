namespace Enterprise.MasterFiles.GUI
{
	partial class AccCommissionRulePreviewPane
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ruleDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ACM_EndDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ACM_StartDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ACM_SubModuleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACM_ServiceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACM_ProductDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACM_CommissionTriggerTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ACM_CommissionBasisDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.newAgreementDefaultsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ruleDetailsGroupBox.SuspendLayout();
			this.ACM_EndDateDateEdit.SuspendLayout();
			this.ACM_StartDateDateEdit.SuspendLayout();
			this.ACM_SubModuleDropEdit.SuspendLayout();
			this.ACM_ServiceDropEdit.SuspendLayout();
			this.ACM_ProductDropEdit.SuspendLayout();
			this.ACM_CommissionTriggerTypeDropEdit.SuspendLayout();
			this.ACM_CommissionBasisDropEdit.SuspendLayout();
			this.newAgreementDefaultsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccCommissionRule);
			// 
			// ruleDetailsGroupBox
			// 
			this.ruleDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ruleDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("95d0cb27-ad20-452f-8ed0-a88008e92c65", "Rule Applicability");
			this.ruleDetailsGroupBox.Controls.Add(this.ACM_EndDateDateEdit);
			this.ruleDetailsGroupBox.Controls.Add(this.ACM_StartDateDateEdit);
			this.ruleDetailsGroupBox.Controls.Add(this.ACM_SubModuleDropEdit);
			this.ruleDetailsGroupBox.Controls.Add(this.ACM_ServiceDropEdit);
			this.ruleDetailsGroupBox.Controls.Add(this.ACM_ProductDropEdit);
			this.ruleDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ruleDetailsGroupBox.Name = "ruleDetailsGroupBox";
			this.ruleDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 117, true);
			this.ruleDetailsGroupBox.TabIndex = 0;
			this.ruleDetailsGroupBox.TabStop = false;
			// 
			// ACM_EndDateDateEdit
			// 
			this.ACM_EndDateDateEdit.AllowDrop = true;
			this.ACM_EndDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ACM_EndDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ACM_EndDateDateEdit, "ACM_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_EndDate)));
			this.ACM_EndDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 91, true);
			this.ACM_EndDateDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACM_EndDateDateEdit.Name = "ACM_EndDateDateEdit";
			this.ACM_EndDateDateEdit.TabIndex = 4;
			// 
			// ACM_StartDateDateEdit
			// 
			this.ACM_StartDateDateEdit.AllowDrop = true;
			this.ACM_StartDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ACM_StartDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ACM_StartDateDateEdit, "ACM_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_StartDate)));
			this.ACM_StartDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 91, true);
			this.ACM_StartDateDateEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACM_StartDateDateEdit.Name = "ACM_StartDateDateEdit";
			this.ACM_StartDateDateEdit.TabIndex = 3;
			// 
			// ACM_SubModuleDropEdit
			// 
			this.ACM_SubModuleDropEdit.AllowDrop = true;
			this.ACM_SubModuleDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ACM_SubModuleDropEdit, "ACM_SubModule");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_SubModule)));
			this.ACM_SubModuleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 67, true);
			this.ACM_SubModuleDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACM_SubModuleDropEdit.Name = "ACM_SubModuleDropEdit";
			this.ACM_SubModuleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.ACM_SubModuleDropEdit.TabIndex = 2;
			// 
			// ACM_ServiceDropEdit
			// 
			this.ACM_ServiceDropEdit.AllowDrop = true;
			this.ACM_ServiceDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ACM_ServiceDropEdit, "ACM_Service");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_Service)));
			this.ACM_ServiceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 43, true);
			this.ACM_ServiceDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACM_ServiceDropEdit.Name = "ACM_ServiceDropEdit";
			this.ACM_ServiceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.ACM_ServiceDropEdit.TabIndex = 1;
			// 
			// ACM_ProductDropEdit
			// 
			this.ACM_ProductDropEdit.AllowDrop = true;
			this.ACM_ProductDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ACM_ProductDropEdit, "ACM_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_Product)));
			this.ACM_ProductDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 19, true);
			this.ACM_ProductDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACM_ProductDropEdit.Name = "ACM_ProductDropEdit";
			this.ACM_ProductDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.ACM_ProductDropEdit.TabIndex = 0;
			// 
			// ACM_CommissionTriggerTypeDropEdit
			// 
			this.ACM_CommissionTriggerTypeDropEdit.AllowDrop = true;
			this.ACM_CommissionTriggerTypeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ACM_CommissionTriggerTypeDropEdit, "ACM_CommissionTriggerType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_CommissionTriggerType)));
			this.ACM_CommissionTriggerTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 42, true);
			this.ACM_CommissionTriggerTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACM_CommissionTriggerTypeDropEdit.Name = "ACM_CommissionTriggerTypeDropEdit";
			this.ACM_CommissionTriggerTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.ACM_CommissionTriggerTypeDropEdit.TabIndex = 1;
			// 
			// ACM_CommissionBasisDropEdit
			// 
			this.ACM_CommissionBasisDropEdit.AllowDrop = true;
			this.ACM_CommissionBasisDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ACM_CommissionBasisDropEdit, "ACM_CommissionBasis");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_CommissionBasis)));
			this.ACM_CommissionBasisDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 18, true);
			this.ACM_CommissionBasisDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.ACM_CommissionBasisDropEdit.Name = "ACM_CommissionBasisDropEdit";
			this.ACM_CommissionBasisDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.ACM_CommissionBasisDropEdit.TabIndex = 0;
			// 
			// newAgreementDefaultsGroupBox
			// 
			this.newAgreementDefaultsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.newAgreementDefaultsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("de9b896e-df3e-44f1-a526-bd79df9e41d8", "New Agreement Defaults");
			this.newAgreementDefaultsGroupBox.Controls.Add(this.ACM_CommissionTriggerTypeDropEdit);
			this.newAgreementDefaultsGroupBox.Controls.Add(this.ACM_CommissionBasisDropEdit);
			this.newAgreementDefaultsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 123, true);
			this.newAgreementDefaultsGroupBox.Name = "newAgreementDefaultsGroupBox";
			this.newAgreementDefaultsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 69, true);
			this.newAgreementDefaultsGroupBox.TabIndex = 1;
			this.newAgreementDefaultsGroupBox.TabStop = false;
			// 
			// AccCommissionRulePreviewPane
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.newAgreementDefaultsGroupBox);
			this.Controls.Add(this.ruleDetailsGroupBox);
			this.Name = "AccCommissionRulePreviewPane";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 192, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ruleDetailsGroupBox.ResumeLayout(false);
			this.ruleDetailsGroupBox.PerformLayout();
			this.ACM_EndDateDateEdit.ResumeLayout(true);
			this.ACM_EndDateDateEdit.PerformLayout();
			this.ACM_StartDateDateEdit.ResumeLayout(true);
			this.ACM_StartDateDateEdit.PerformLayout();
			this.ACM_SubModuleDropEdit.ResumeLayout(true);
			this.ACM_SubModuleDropEdit.PerformLayout();
			this.ACM_ServiceDropEdit.ResumeLayout(true);
			this.ACM_ServiceDropEdit.PerformLayout();
			this.ACM_ProductDropEdit.ResumeLayout(true);
			this.ACM_ProductDropEdit.PerformLayout();
			this.ACM_CommissionTriggerTypeDropEdit.ResumeLayout(true);
			this.ACM_CommissionTriggerTypeDropEdit.PerformLayout();
			this.ACM_CommissionBasisDropEdit.ResumeLayout(true);
			this.ACM_CommissionBasisDropEdit.PerformLayout();
			this.newAgreementDefaultsGroupBox.ResumeLayout(false);
			this.newAgreementDefaultsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox ruleDetailsGroupBox;
		private ZArchitecture.GUI.ZDropEdit ACM_SubModuleDropEdit;
		private ZArchitecture.GUI.ZDropEdit ACM_ServiceDropEdit;
		private ZArchitecture.GUI.ZDropEdit ACM_ProductDropEdit;
		private ZArchitecture.GUI.ZDropEdit ACM_CommissionBasisDropEdit;
		private ZArchitecture.GUI.ZDateEdit ACM_StartDateDateEdit;
		private ZArchitecture.GUI.ZDateEdit ACM_EndDateDateEdit;
		private ZArchitecture.GUI.ZDropEdit ACM_CommissionTriggerTypeDropEdit;
		protected System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
		private ZArchitecture.GUI.ZGroupBox newAgreementDefaultsGroupBox;
	}
}
