namespace Enterprise.MasterFiles.GUI
{
	partial class TaskSkillsForm
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
			this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.HeadingLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SimpleDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SkillsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SkillsControl = new Enterprise.MasterFiles.GUI.TaskSkillsControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.SkillsGroupBox.SuspendLayout();
			this.SkillsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 369, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 24, true);
			this.MainStatusBar.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.HeadingLabel);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 39, true);
			this.TopPanel.TabIndex = 1;
			// 
			// HeadingLabel
			// 
			this.HeadingLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.HeadingLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.HeadingLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.HeadingLabel.Name = "HeadingLabel";
			this.HeadingLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 27, true);
			this.HeadingLabel.TabIndex = 2;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.SimpleDescriptionTextBox);
			this.BottomPanel.Controls.Add(this.CloseButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 326, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 43, true);
			this.BottomPanel.TabIndex = 7;
			// 
			// SimpleDescriptionTextBox
			// 
			this.SimpleDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.SimpleDescriptionTextBox, "P9_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ProcessTask)(null)).P9_Description)));
			this.SimpleDescriptionTextBox.CaptionResourceString = null;
			this.SimpleDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SimpleDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 10, true);
			this.SimpleDescriptionTextBox.Name = "SimpleDescriptionTextBox";
			this.SimpleDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 20, true);
			this.SimpleDescriptionTextBox.TabIndex = 6;
			this.SimpleDescriptionTextBox.Visible = false;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("481E3C08-5599-473F-868A-70CB94BBBF45", "Close");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 10, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// SkillsGroupBox
			// 
			this.SkillsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("B659B5BE-95AD-476F-AB0F-189F5B5E78AF", "Required Competency");
			this.SkillsGroupBox.Controls.Add(this.SkillsControl);
			this.SkillsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SkillsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 39, true);
			this.SkillsGroupBox.Name = "SkillsGroupBox";
			this.SkillsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 287, true);
			this.SkillsGroupBox.TabIndex = 3;
			this.SkillsGroupBox.TabStop = false;
			// 
			// SkillsControl
			// 
			this.SkillsControl.AllowDrop = true;
			this.SkillsControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SkillsControl, ".");
			this.SkillsControl.BindTo = null;
			this.SkillsControl.CreateSkillLearningTaskFunction = null;
			this.SkillsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.SkillsControl.Name = "SkillsControl";
			this.SkillsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(567, 262, true);
			this.SkillsControl.TabIndex = 4;
			// 
			// TaskSkillsForm
			// 
			this.AcceptButton = this.CloseButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("D99E9AED-BEB2-467F-9B65-337461DAFD4B", "Competency Requirements");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(573, 393, true);
			this.Controls.Add(this.SkillsGroupBox);
			this.Controls.Add(this.BottomPanel);
			this.Controls.Add(this.TopPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessTask);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 300, true);
			this.Name = "TaskSkillsForm";
			this.Text = "TaskSkillsForm";
			this.Controls.SetChildIndex(this.TopPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.SkillsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.SkillsGroupBox.ResumeLayout(false);
			this.SkillsGroupBox.PerformLayout();
			this.SkillsControl.ResumeLayout(true);
			this.SkillsControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZLabel HeadingLabel;
		internal ZArchitecture.GUI.ZButton CloseButton;
		internal Enterprise.MasterFiles.GUI.TaskSkillsControl SkillsControl;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox SkillsGroupBox;
		internal ZArchitecture.ZTextBox SimpleDescriptionTextBox;
		internal Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		internal Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
	}
}
