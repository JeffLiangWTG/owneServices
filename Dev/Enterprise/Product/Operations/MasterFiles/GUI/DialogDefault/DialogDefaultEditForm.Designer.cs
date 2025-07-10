namespace Enterprise.MasterFiles.GUI
{
	partial class DialogDefaultEditForm
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
		new void InitializeComponent()
		{
			this.showDialogCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OverrideChildrenCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LevelDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OwnerFindBox = new Enterprise.MasterFiles.GUI.DialogDefault.DialogDefaultGuidFindBox();
			this.serializedDefaultsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.saveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.exitButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.appliesToSimilarDefaultsCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.captionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LevelDropEdit.SuspendLayout();
			this.OwnerFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 454, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Core.DialogDefault.StmDialogDefault);
			// 
			// showDialogCheckBox
			// 
			this.BindingSource.SetBindingMember(this.showDialogCheckBox, "SDD_ShowDialog");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.StmDialogDefault)(null)).SDD_ShowDialog)));
			this.showDialogCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("56e8a8c2-79bd-432f-b81c-83514867181e", "Show Dialog");
			this.showDialogCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.showDialogCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 88, true);
			this.showDialogCheckBox.Name = "showDialogCheckBox";
			this.showDialogCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.showDialogCheckBox.TabIndex = 3;
			this.showDialogCheckBox.UseVisualStyleBackColor = true;
			// 
			// OverrideChildrenCheckBox
			// 
			this.OverrideChildrenCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideChildrenCheckBox, "SDD_OverrideAllChildLevels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.StmDialogDefault)(null)).SDD_OverrideAllChildLevels)));
			this.OverrideChildrenCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4ec59751-b98f-404b-a29e-2952b4f79222", "Override Lower Levels");
			this.OverrideChildrenCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideChildrenCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 92, true);
			this.OverrideChildrenCheckBox.Name = "OverrideChildrenCheckBox";
			this.OverrideChildrenCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 17, true);
			this.OverrideChildrenCheckBox.TabIndex = 4;
			this.OverrideChildrenCheckBox.UseVisualStyleBackColor = true;
			// 
			// LevelDropEdit
			// 
			this.LevelDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LevelDropEdit, "SDD_Level");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Core.DialogDefault.StmDialogDefault)(null)).SDD_Level)));
			this.LevelDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("f008a70d-b319-452f-86ca-78961bfda97f", "Owner Level");
			this.LevelDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 36, true);
			this.LevelDropEdit.Name = "LevelDropEdit";
			this.LevelDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.LevelDropEdit.TabIndex = 1;
			// 
			// OwnerFindBox
			// 
			this.OwnerFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerFindBox, "SDD_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Core.DialogDefault.StmDialogDefault)(null)).SDD_Owner)));
			this.OwnerFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("3d1051ae-7ebe-4b26-9f9c-b8fe8220e668", "Owner");
			this.OwnerFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 62, true);
			this.OwnerFindBox.Name = "OwnerFindBox";
			this.OwnerFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.OwnerFindBox.TabIndex = 2;
			// 
			// serializedDefaultsTextBox
			// 
			this.serializedDefaultsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.serializedDefaultsTextBox, "SDD_SerializedDefaults");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Core.DialogDefault.StmDialogDefault)(null)).SDD_SerializedDefaults)));
			this.serializedDefaultsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.serializedDefaultsTextBox, false);
			this.serializedDefaultsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 124, true);
			this.serializedDefaultsTextBox.Multiline = true;
			this.serializedDefaultsTextBox.Name = "serializedDefaultsTextBox";
			this.serializedDefaultsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(624, 295, true);
			this.serializedDefaultsTextBox.TabIndex = 6;
			// 
			// saveButton
			// 
			this.saveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.saveButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("24bf89cf-3848-4779-821e-a9fb4955155c", "Save and Exit");
			this.saveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 425, true);
			this.saveButton.Name = "saveButton";
			this.saveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(101, 23, true);
			this.saveButton.TabIndex = 7;
			this.saveButton.UseVisualStyleBackColor = true;
			// 
			// exitButton
			// 
			this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.exitButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("4789f618-882b-4c84-8daa-5e55736c4e3f", "Cancel");
			this.exitButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(562, 425, true);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.exitButton.TabIndex = 8;
			this.exitButton.UseVisualStyleBackColor = true;
			// 
			// appliesToSimilarDefaultsCheckbox
			// 
			this.appliesToSimilarDefaultsCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.appliesToSimilarDefaultsCheckbox, "AppliesToSimilarDialogs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Core.DialogDefault.StmDialogDefault)(null)).AppliesToSimilarDialogs)));
			this.appliesToSimilarDefaultsCheckbox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8BD677D7-69DA-4076-BC9A-0A8C2D1ACD2A", "Applies to Similar Dialogs", "This saved default applies to similar dialogs. A similar dialog would be one that asks the same question but with different criteria, such as a different company.");
			this.appliesToSimilarDefaultsCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.appliesToSimilarDefaultsCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 92, true);
			this.appliesToSimilarDefaultsCheckbox.Name = "appliesToSimilarDefaultsCheckbox";
			this.appliesToSimilarDefaultsCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(141, 17, true);
			this.appliesToSimilarDefaultsCheckbox.TabIndex = 5;
			this.appliesToSimilarDefaultsCheckbox.UseVisualStyleBackColor = true;
			// 
			// captionTextBox
			// 
			this.BindingSource.SetBindingMember(this.captionTextBox, "SDD_Caption");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Core.DialogDefault.StmDialogDefault)(null)).SDD_Caption)));
			this.captionTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e84bc2a8-a3ea-4279-aa97-7a58b0c90cb0", "Caption", "The caption of the dialog when it was last saved");
			this.captionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 13, true);
			this.captionTextBox.Name = "captionTextBox";
			this.captionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.captionTextBox.TabIndex = 0;
			// 
			// DialogDefaultEditForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("DB4E1A0E-912E-4EF8-9304-05F2BAAFBF77", "Dialog Default");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 478, true);
			this.Controls.Add(this.captionTextBox);
			this.Controls.Add(this.appliesToSimilarDefaultsCheckbox);
			this.Controls.Add(this.exitButton);
			this.Controls.Add(this.saveButton);
			this.Controls.Add(this.serializedDefaultsTextBox);
			this.Controls.Add(this.OwnerFindBox);
			this.Controls.Add(this.LevelDropEdit);
			this.Controls.Add(this.OverrideChildrenCheckBox);
			this.Controls.Add(this.showDialogCheckBox);
			this.DataSourceType = typeof(Enterprise.Core.DialogDefault.StmDialogDefault);
			this.Name = "DialogDefaultEditForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.showDialogCheckBox, 0);
			this.Controls.SetChildIndex(this.OverrideChildrenCheckBox, 0);
			this.Controls.SetChildIndex(this.LevelDropEdit, 0);
			this.Controls.SetChildIndex(this.OwnerFindBox, 0);
			this.Controls.SetChildIndex(this.serializedDefaultsTextBox, 0);
			this.Controls.SetChildIndex(this.saveButton, 0);
			this.Controls.SetChildIndex(this.exitButton, 0);
			this.Controls.SetChildIndex(this.appliesToSimilarDefaultsCheckbox, 0);
			this.Controls.SetChildIndex(this.captionTextBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LevelDropEdit.ResumeLayout(true);
			this.LevelDropEdit.PerformLayout();
			this.OwnerFindBox.ResumeLayout(true);
			this.OwnerFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZCheckBox showDialogCheckBox;
		protected ZArchitecture.GUI.ZCheckBox OverrideChildrenCheckBox;
		protected ZArchitecture.GUI.ZDropEdit LevelDropEdit;
		protected MasterFiles.GUI.DialogDefault.DialogDefaultGuidFindBox OwnerFindBox;
		protected ZArchitecture.ZTextBox serializedDefaultsTextBox;
		protected internal ZArchitecture.GUI.ZButton saveButton;
		protected internal ZArchitecture.GUI.ZButton exitButton;
		protected ZArchitecture.GUI.ZCheckBox appliesToSimilarDefaultsCheckbox;
		private ZArchitecture.ZTextBox captionTextBox;
	}
}
