namespace Enterprise.Customs.TW.Module.OperationalActions
{
	partial class SendOriginalEntryConfigurationControl
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
			this.ConfigurationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ConfigurationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UseDaysOfDelayedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ReMergeAndCalculateCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.SuppressNotificationDialogsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IgnoreMessageWarningsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfigurationPanel.SuspendLayout();
			this.ConfigurationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Module.OperationalActions.DeclarationMessageOperationalActionMethodApplicator);
			// 
			// ConfigurationPanel
			// 
			this.ConfigurationPanel.BackColor = System.Drawing.SystemColors.Control;
			this.ConfigurationPanel.Controls.Add(this.ConfigurationGroupBox);
			this.ConfigurationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfigurationPanel.Name = "ConfigurationPanel";
			this.ConfigurationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 220, true);
			this.ConfigurationPanel.TabIndex = 8;
			// 
			// ConfigurationGroupBox
			// 
			this.ConfigurationGroupBox.CaptionResourceString = Enterprise.Customs.TW.Module.Res.GetData("2e240db9-2044-4051-96b3-a38a4d06af3a", "Send Original Entry to Taiwan Customs Configuration");
			this.ConfigurationGroupBox.Controls.Add(this.UseDaysOfDelayedCheckBox);
			this.ConfigurationGroupBox.Controls.Add(this.ReMergeAndCalculateCheckBox);
			this.ConfigurationGroupBox.Controls.Add(this.SuppressNotificationDialogsCheckBox);
			this.ConfigurationGroupBox.Controls.Add(this.IgnoreMessageWarningsCheckBox);
			this.ConfigurationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfigurationGroupBox.Name = "ConfigurationGroupBox";
			this.ConfigurationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 220, true);
			this.ConfigurationGroupBox.TabIndex = 17;
			this.ConfigurationGroupBox.TabStop = false;
			// 
			// UseDaysOfDelayedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UseDaysOfDelayedCheckBox, "UseDaysOfDelayed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.TW.Module.OperationalActions.DeclarationMessageOperationalActionMethodApplicator)(null)).UseDaysOfDelayed)));
			this.UseDaysOfDelayedCheckBox.CaptionResourceString = Enterprise.Customs.TW.Module.Res.GetData("a1765a84-08bd-4e89-8b9f-287103e40150", "Use system calculated days of delayed");
			this.UseDaysOfDelayedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseDaysOfDelayedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 134, true);
			this.UseDaysOfDelayedCheckBox.Name = "UseDaysOfDelayedCheckBox";
			this.UseDaysOfDelayedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.UseDaysOfDelayedCheckBox.TabIndex = 6;
			this.UseDaysOfDelayedCheckBox.UseVisualStyleBackColor = true;
			// 
			// ReMergeAndCalculateCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ReMergeAndCalculateCheckBox, "ReMergeAndCalculate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.TW.Module.OperationalActions.DeclarationMessageOperationalActionMethodApplicator)(null)).ReMergeAndCalculate)));
			this.ReMergeAndCalculateCheckBox.CaptionResourceString = Enterprise.Customs.TW.Module.Res.GetData("ef46d3a2-7980-4c6d-bd3b-ef21995149f7", "Re-merge and re-calculate before sending");
			this.ReMergeAndCalculateCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReMergeAndCalculateCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 100, true);
			this.ReMergeAndCalculateCheckBox.Name = "ReMergeAndCalculateCheckBox";
			this.ReMergeAndCalculateCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.ReMergeAndCalculateCheckBox.TabIndex = 5;
			this.ReMergeAndCalculateCheckBox.UseVisualStyleBackColor = true;
			// 
			// SuppressNotificationDialogsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SuppressNotificationDialogsCheckBox, "SuppressNotificationPopout");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.TW.Module.OperationalActions.DeclarationMessageOperationalActionMethodApplicator)(null)).SuppressNotificationPopout)));
			this.SuppressNotificationDialogsCheckBox.CaptionResourceString = Enterprise.Customs.TW.Module.Res.GetData("88ebe1a2-0736-42a0-981c-bfc3c04081e7", "Suppress pop-up notification dialogs");
			this.SuppressNotificationDialogsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SuppressNotificationDialogsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 66, true);
			this.SuppressNotificationDialogsCheckBox.Name = "SuppressNotificationDialogsCheckBox";
			this.SuppressNotificationDialogsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.SuppressNotificationDialogsCheckBox.TabIndex = 4;
			this.SuppressNotificationDialogsCheckBox.UseVisualStyleBackColor = true;
			// 
			// IgnoreMessageWarningsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IgnoreMessageWarningsCheckBox, "IgnoreMessageWarnings");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.Customs.TW.Module.OperationalActions.DeclarationMessageOperationalActionMethodApplicator)(null)).IgnoreMessageWarnings)));
			this.IgnoreMessageWarningsCheckBox.CaptionResourceString = Enterprise.Customs.TW.Module.Res.GetData("8a5ae8f7-21fe-4f0f-8347-5ff61c42e6ae", "Ignore message warnings and continue sending");
			this.IgnoreMessageWarningsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IgnoreMessageWarningsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(36, 32, true);
			this.IgnoreMessageWarningsCheckBox.Name = "IgnoreMessageWarningsCheckBox";
			this.IgnoreMessageWarningsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 17, true);
			this.IgnoreMessageWarningsCheckBox.TabIndex = 3;
			this.IgnoreMessageWarningsCheckBox.UseVisualStyleBackColor = true;
			// 
			// SendOriginalEntryConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ConfigurationPanel);
			this.Name = "SendOriginalEntryConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 220, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfigurationPanel.ResumeLayout(false);
			this.ConfigurationPanel.PerformLayout();
			this.ConfigurationGroupBox.ResumeLayout(false);
			this.ConfigurationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ConfigurationPanel;
		private ZArchitecture.GUI.ZGroupBox ConfigurationGroupBox;
		private ZArchitecture.GUI.ZCheckBox UseDaysOfDelayedCheckBox;
		private ZArchitecture.GUI.ZCheckBox ReMergeAndCalculateCheckBox;
		private ZArchitecture.GUI.ZCheckBox SuppressNotificationDialogsCheckBox;
		private ZArchitecture.GUI.ZCheckBox IgnoreMessageWarningsCheckBox;
	}
}
