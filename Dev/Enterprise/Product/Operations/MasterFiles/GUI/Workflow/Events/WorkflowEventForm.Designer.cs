
namespace Enterprise.MasterFiles.GUI
{
	partial class WorkflowEventForm
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
		private new void InitializeComponent()
		{
			this.eventGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsCustomisableCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.eventDescriptionTextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.eventCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.isReferenceFormatOverriddenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.referenceFormatOverriddenTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.referenceFormatTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.propagateToParentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.eventGroupbox.SuspendLayout();
			this.eventDescriptionTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 307, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.eventGroupbox);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 285, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 285, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 285, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 307, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ZArchitecture.Business.StmEvent);
			// 
			// eventGroupbox
			// 
			this.eventGroupbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("668e9880-b9fd-11e3-b91e-1c6f653fb9f5", "Event Details");
			this.eventGroupbox.Controls.Add(this.propagateToParentCheckBox);
			this.eventGroupbox.Controls.Add(this.IsActiveCheckBox);
			this.eventGroupbox.Controls.Add(this.IsCustomisableCheckBox);
			this.eventGroupbox.Controls.Add(this.eventDescriptionTextBox);
			this.eventGroupbox.Controls.Add(this.eventCodeTextBox);
			this.eventGroupbox.Controls.Add(this.isReferenceFormatOverriddenCheckBox);
			this.eventGroupbox.Controls.Add(this.referenceFormatOverriddenTextBox);
			this.eventGroupbox.Controls.Add(this.referenceFormatTextBox);
			this.eventGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eventGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.eventGroupbox.Name = "eventGroupbox";
			this.eventGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 285, true);
			this.eventGroupbox.TabIndex = 0;
			this.eventGroupbox.TabStop = false;
			// 
			// IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "SE_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.StmEvent)(null)).SE_IsActive)));
			this.IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d2351154-e0e2-45a5-a3b3-815425d6f79e", "Is Active");
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 206, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 24, true);
			this.IsActiveCheckBox.TabIndex = 7;
			// 
			// IsCustomisableCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsCustomisableCheckBox, "SE_IsCustomizable");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.StmEvent)(null)).SE_IsCustomizable)));
			this.IsCustomisableCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3c337fae-f10a-43a1-87fb-0a9ffd176ff7", "Is Customizable");
			this.IsCustomisableCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsCustomisableCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 178, true);
			this.IsCustomisableCheckBox.Name = "IsCustomisableCheckBox";
			this.IsCustomisableCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 24, true);
			this.IsCustomisableCheckBox.TabIndex = 6;
			// 
			// eventDescriptionTextBox
			// 
			this.eventDescriptionTextBox.AcceptsReturn = false;
			this.eventDescriptionTextBox.AllowDrop = true;
			this.eventDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.eventDescriptionTextBox, "SE_Desc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmEvent)(null)).SE_Desc)));
			this.eventDescriptionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dbd567f1-01d0-4aea-b15d-7e621d1f81b0", "Event name");
			this.eventDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.eventDescriptionTextBox.GridCurrent = null;
			this.eventDescriptionTextBox.GridMember = null;
			this.eventDescriptionTextBox.IsLanguageEditingEnabled = true;
			this.eventDescriptionTextBox.IsMultiLine = false;
			this.eventDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 59, true);
			this.eventDescriptionTextBox.Name = "eventDescriptionTextBox";
			this.eventDescriptionTextBox.ReadOnly = false;
			this.eventDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(335, 20, true);
			this.eventDescriptionTextBox.TabIndex = 2;
			// 
			// eventCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.eventCodeTextBox, "SE_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmEvent)(null)).SE_Code)));
			this.eventCodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("c88c7b8a-ba12-11e3-b0c5-1c6f653fb9f5", "Event code");
			this.eventCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 28, true);
			this.eventCodeTextBox.Name = "eventCodeTextBox";
			this.eventCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 17, true);
			this.eventCodeTextBox.TabIndex = 0;
			// 
			// isReferenceFormatOverriddenCheckBox
			// 
			this.BindingSource.SetBindingMember(this.isReferenceFormatOverriddenCheckBox, "SE_IsRefernceFormatOverridden");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.StmEvent)(null)).SE_IsRefernceFormatOverridden)));
			this.isReferenceFormatOverriddenCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6e3b8492-2e51-4fd8-9d60-d55fa516f2f8", "Use overridden reference format");
			this.isReferenceFormatOverriddenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isReferenceFormatOverriddenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 121, true);
			this.isReferenceFormatOverriddenCheckBox.Name = "isReferenceFormatOverriddenCheckBox";
			this.isReferenceFormatOverriddenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 24, true);
			this.isReferenceFormatOverriddenCheckBox.TabIndex = 4;
			// 
			// referenceFormatOverriddenTextBox
			// 
			this.referenceFormatOverriddenTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.referenceFormatOverriddenTextBox, "SE_OverriddenReferenceFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmEvent)(null)).SE_OverriddenReferenceFormat)));
			this.referenceFormatOverriddenTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("84413948-77ec-4836-ad7f-fadd723195bc", "Overridden reference format");
			this.referenceFormatOverriddenTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.referenceFormatOverriddenTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 152, true);
			this.referenceFormatOverriddenTextBox.Name = "referenceFormatOverriddenTextBox";
			this.referenceFormatOverriddenTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 17, true);
			this.referenceFormatOverriddenTextBox.TabIndex = 5;
			// 
			// referenceFormatTextBox
			// 
			this.referenceFormatTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.referenceFormatTextBox, "SE_ReferenceFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ZArchitecture.Business.StmEvent)(null)).SE_ReferenceFormat)));
			this.referenceFormatTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e437eab1-7731-4855-9e71-305b65e020c1", "Reference format");
			this.referenceFormatTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.referenceFormatTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 90, true);
			this.referenceFormatTextBox.Name = "referenceFormatTextBox";
			this.referenceFormatTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 17, true);
			this.referenceFormatTextBox.TabIndex = 3;
			// 
			// propagateToParentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.propagateToParentCheckBox, "SE_PropagateToParent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.ZArchitecture.Business.StmEvent)(null)).SE_PropagateToParent)));
			this.propagateToParentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.propagateToParentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(162, 234, true);
			this.propagateToParentCheckBox.Name = "propagateToParentCheckBox";
			this.propagateToParentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 24, true);
			this.propagateToParentCheckBox.TabIndex = 8;
			// 
			// WorkflowEventForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e43f898a-ba12-11e3-a1b8-1c6f653fb9f5", "Workflow Event");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(596, 363, true);
			this.DataSourceType = typeof(Enterprise.ZArchitecture.Business.StmEvent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 400, true);
			this.Name = "WorkflowEventForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.eventGroupbox.ResumeLayout(false);
			this.eventGroupbox.PerformLayout();
			this.eventDescriptionTextBox.ResumeLayout(true);
			this.eventDescriptionTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox eventCodeTextBox;
		private ZArchitecture.ZTranslatableTextControl eventDescriptionTextBox;
		private ZArchitecture.ZTextBox referenceFormatTextBox;
		private ZArchitecture.ZTextBox referenceFormatOverriddenTextBox;
		private ZArchitecture.GUI.ZCheckBox isReferenceFormatOverriddenCheckBox;
		private ZArchitecture.GUI.ZGroupBox eventGroupbox;
		private ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private ZArchitecture.GUI.ZCheckBox IsCustomisableCheckBox;
		private ZArchitecture.GUI.ZCheckBox propagateToParentCheckBox;
	}
}
