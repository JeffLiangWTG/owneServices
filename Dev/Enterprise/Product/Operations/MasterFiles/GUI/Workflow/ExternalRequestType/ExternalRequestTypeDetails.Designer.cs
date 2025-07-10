
namespace Enterprise.MasterFiles.GUI
{
	partial class ExternalRequestTypeDetails
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
			this.detailsGroup = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RQT_RequiredInDaysCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RQT_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RQT_DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RQT_JobTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RQT_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RQT_FormTypeGuiFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.RQT_AssigneeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RQT_ReviewerDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RQT_RequiredInDaysLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.detailsGroup.SuspendLayout();
			this.RQT_JobTypeDropEdit.SuspendLayout();
			this.RQT_FormTypeGuiFindBox.SuspendLayout();
			this.RQT_AssigneeDropEdit.SuspendLayout();
			this.RQT_ReviewerDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ExternalRequestType);
			// 
			// detailsGroup
			// 
			this.detailsGroup.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("369aebd4-679d-48e6-b2d8-d55079b958f9", "Details");
			this.detailsGroup.Controls.Add(this.RQT_CodeTextBox);
			this.detailsGroup.Controls.Add(this.RQT_DescriptionTextBox);
			this.detailsGroup.Controls.Add(this.RQT_JobTypeDropEdit);
			this.detailsGroup.Controls.Add(this.RQT_IsActiveCheckBox);
			this.detailsGroup.Controls.Add(this.RQT_FormTypeGuiFindBox);
			this.detailsGroup.Controls.Add(this.RQT_AssigneeDropEdit);
			this.detailsGroup.Controls.Add(this.RQT_ReviewerDropEdit);
			this.detailsGroup.Controls.Add(this.RQT_RequiredInDaysCalcEdit);
			this.detailsGroup.Controls.Add(this.RQT_RequiredInDaysLabel);
			this.detailsGroup.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsGroup.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.detailsGroup.Name = "detailsGroup";
			this.detailsGroup.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 357, true);
			this.detailsGroup.TabIndex = 0;
			this.detailsGroup.TabStop = false;
			// 
			// RQT_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RQT_CodeTextBox, "RQT_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ExternalRequestType)(null)).RQT_Code)));
			this.RQT_CodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3a6d4207-ebd0-4f55-8783-bbcf93e381c6", "Code");
			this.RQT_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 32, true);
			this.RQT_CodeTextBox.Name = "RQT_CodeTextBox";
			this.RQT_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 15, true);
			this.RQT_CodeTextBox.TabIndex = 1;
			// 
			// RQT_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.RQT_DescriptionTextBox, "RQT_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ExternalRequestType)(null)).RQT_Description)));
			this.RQT_DescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5ac2684b-788a-4c7f-904b-401fe2432cff", "Description");
			this.RQT_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 72, true);
			this.RQT_DescriptionTextBox.Name = "RQT_DescriptionTextBox";
			this.RQT_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(646, 15, true);
			this.RQT_DescriptionTextBox.TabIndex = 2;
			// 
			// RQT_JobTypeDropEdit
			// 
			this.RQT_JobTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RQT_JobTypeDropEdit, "RQT_JobType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ExternalRequestType)(null)).RQT_JobType)));
			this.RQT_JobTypeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e321bec8-6540-4349-98d8-58892190413a", "Job Type");
			this.RQT_JobTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 123, true);
			this.RQT_JobTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.RQT_JobTypeDropEdit.Name = "RQT_JobTypeDropEdit";
			this.RQT_JobTypeDropEdit.PreBoundMaxLength = 3;
			this.RQT_JobTypeDropEdit.ShowDescriptionBox = false;
			this.RQT_JobTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 15, true);
			this.RQT_JobTypeDropEdit.TabIndex = 3;
			// 
			// RQT_IsActiveCheckBox
			// 
			this.RQT_IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RQT_IsActiveCheckBox, "RQT_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.ExternalRequestType)(null)).RQT_IsActive)));
			this.RQT_IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ff567fbb-8c57-41a8-8c11-4d447a9c4f1e", "Is Active");
			this.RQT_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 152, true);
			this.RQT_IsActiveCheckBox.Name = "RQT_IsActiveCheckBox";
			this.RQT_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 14, true);
			this.RQT_IsActiveCheckBox.TabIndex = 4;
			// 
			// RQT_FormTypeGuiFindBox
			// 
			this.RQT_FormTypeGuiFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RQT_FormTypeGuiFindBox, "RQT_RIT_Template");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.ExternalRequestType)(null)).RQT_RIT_Template)));
			this.RQT_FormTypeGuiFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ccb222b2-62d0-4f61-9426-998dcf6b11c0", "Request Template");
			this.RQT_FormTypeGuiFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 192, true);
			this.RQT_FormTypeGuiFindBox.Name = "RQT_FormTypeGuiFindBox";
			this.RQT_FormTypeGuiFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.ExternalRequestInfoTemplate;
			this.RQT_FormTypeGuiFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.RQT_FormTypeGuiFindBox.ParentType = null;
			this.RQT_FormTypeGuiFindBox.PreBoundMaxLength = 3;
			this.RQT_FormTypeGuiFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 15, true);
			this.RQT_FormTypeGuiFindBox.TabIndex = 5;
			// 
			// RQT_AssigneeDropEdit
			// 
			this.RQT_AssigneeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RQT_AssigneeDropEdit, "RQT_Assignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ExternalRequestType)(null)).RQT_Assignee)));
			this.RQT_AssigneeDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("17520430-52d4-4c64-bb6a-7599e4cbe96b", "Assignee");
			this.RQT_AssigneeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 224, true);
			this.RQT_AssigneeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.RQT_AssigneeDropEdit.Name = "RQT_AssigneeDropEdit";
			this.RQT_AssigneeDropEdit.PreBoundMaxLength = 3;
			this.RQT_AssigneeDropEdit.ShowDescriptionBox = false;
			this.RQT_AssigneeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 15, true);
			this.RQT_AssigneeDropEdit.TabIndex = 6;
			// 
			// RQT_ReviewerDropEdit
			// 
			this.RQT_ReviewerDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RQT_ReviewerDropEdit, "RQT_Reviewer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ExternalRequestType)(null)).RQT_Reviewer)));
			this.RQT_ReviewerDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("367eff5b-eb7e-4ffb-bd51-670c673d7af6", "Reviewer");
			this.RQT_ReviewerDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 259, true);
			this.RQT_ReviewerDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.RQT_ReviewerDropEdit.Name = "RQT_ReviewerDropEdit";
			this.RQT_ReviewerDropEdit.PreBoundMaxLength = 3;
			this.RQT_ReviewerDropEdit.ShowDescriptionBox = false;
			this.RQT_ReviewerDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 15, true);
			this.RQT_ReviewerDropEdit.TabIndex = 7;
			// 
			// RQT_RequiredInDaysCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RQT_RequiredInDaysCalcEdit, "RQT_RequiredInDays");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.MasterFiles.Business.ExternalRequestType)(null)).RQT_RequiredInDays)));
			this.RQT_RequiredInDaysCalcEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("1e2e18de-3551-4087-8fc4-24ffc5aa8176", "Required In");
			this.RQT_RequiredInDaysCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 291, true);
			this.RQT_RequiredInDaysCalcEdit.Name = "RQT_RequiredInDaysCalcEdit";
			this.RQT_RequiredInDaysCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 15, true);
			this.RQT_RequiredInDaysCalcEdit.TabIndex = 8;
			// 
			// RQT_RequiredInDaysLabel
			// 
			this.RQT_RequiredInDaysLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RQT_RequiredInDaysLabel.Text = Res.GetString("76fa7292-3018-479f-92c4-c00fd75c9ac7", "Days");
			this.RQT_RequiredInDaysLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 290, true);
			this.RQT_RequiredInDaysLabel.Name = "RQT_RequiredInDaysLabel";
			this.RQT_RequiredInDaysLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 17, true);
			this.RQT_RequiredInDaysLabel.TabIndex = 9;
			this.RQT_RequiredInDaysLabel.UseMnemonic = false;
			// 
			// ExternalRequestTypeDetails
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.detailsGroup);
			this.Name = "ExternalRequestTypeDetails";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(268, 357, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.detailsGroup.ResumeLayout(false);
			this.detailsGroup.PerformLayout();
			this.RQT_JobTypeDropEdit.ResumeLayout(true);
			this.RQT_JobTypeDropEdit.PerformLayout();
			this.RQT_FormTypeGuiFindBox.ResumeLayout(true);
			this.RQT_FormTypeGuiFindBox.PerformLayout();
			this.RQT_AssigneeDropEdit.ResumeLayout(true);
			this.RQT_AssigneeDropEdit.PerformLayout();
			this.RQT_ReviewerDropEdit.ResumeLayout(true);
			this.RQT_ReviewerDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZTextBox RQT_CodeTextBox;
		private ZArchitecture.ZTextBox RQT_DescriptionTextBox;
		private ZArchitecture.GUI.ZDropEdit RQT_JobTypeDropEdit;
		private ZArchitecture.GUI.ZDropEdit RQT_ReviewerDropEdit;
		private ZArchitecture.GUI.ZDropEdit RQT_AssigneeDropEdit;
		private ZArchitecture.GUI.ZCheckBox RQT_IsActiveCheckBox;
		private ZArchitecture.GUI.ZGroupBox detailsGroup;
		private ZArchitecture.GUI.ZGuidFindBox RQT_FormTypeGuiFindBox;
		private ZArchitecture.ZTextBox RQT_RequiredInDaysCalcEdit;
		private ZArchitecture.ZLabel RQT_RequiredInDaysLabel;
	}
}
