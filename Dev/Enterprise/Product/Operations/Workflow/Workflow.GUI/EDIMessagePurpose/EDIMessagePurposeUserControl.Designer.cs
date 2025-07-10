namespace Enterprise.Workflow.GUI
{
	partial class EDIMessagePurposeUserControl
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
			this.triggerPurposes = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.zTextBox3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox2 = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.disableOrgProxyRecipientOverride = new ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.triggerPurposes.SuspendLayout();
			this.zTextBox3.SuspendLayout();
			this.zTextBox2.SuspendLayout();
			this.disableOrgProxyRecipientOverride.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Workflow.Business.EDIMessagePurpose);
			// 
			// triggerPurposes
			// 
			this.triggerPurposes.CaptionResourceString = Enterprise.Workflow.GUI.Res.GetData("2b0021c3-a6b2-4119-bca2-cd8f7f986899", "Purpose Code");
			this.triggerPurposes.Controls.Add(this.zTextBox3);
			this.triggerPurposes.Controls.Add(this.zTextBox1);
			this.triggerPurposes.Controls.Add(this.zTextBox2);
			this.triggerPurposes.Controls.Add(this.disableOrgProxyRecipientOverride);
			this.triggerPurposes.Dock = System.Windows.Forms.DockStyle.Fill;
			this.triggerPurposes.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.triggerPurposes.Name = "triggerPurposes";
			this.triggerPurposes.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 186, true);
			this.triggerPurposes.TabIndex = 1;
			this.triggerPurposes.TabStop = false;
			// 
			// zTextBox3
			// 
			this.zTextBox3.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zTextBox3, "EMP_ECF_Filter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Workflow.Business.EDIMessagePurpose)(null)).EMP_ECF_Filter)));
			this.zTextBox3.IsPrimaryKeyFromCodeRequired = false;
			this.zTextBox3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 71, true);
			this.zTextBox3.Name = "zTextBox3";
			this.zTextBox3.PreBoundMaxLength = 34;
			this.zTextBox3.ShouldResize = true;
			this.zTextBox3.ShowDescriptionBox = false;
			this.zTextBox3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.zTextBox3.TabIndex = 3;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "EMP_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessagePurpose)(null)).EMP_Code)));
			this.zTextBox1.CaptionResourceString = null;
			this.zTextBox1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 19, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.zTextBox1.TabIndex = 1;
			// 
			// zTextBox2
			// 
			this.zTextBox2.AcceptsReturn = false;
			this.zTextBox2.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zTextBox2, "EMP_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Workflow.Business.EDIMessagePurpose)(null)).EMP_Description)));
			this.zTextBox2.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.zTextBox2.GridCurrent = null;
			this.zTextBox2.GridMember = null;
			this.zTextBox2.IsLanguageEditingEnabled = true;
			this.zTextBox2.IsMultiLine = false;
			this.zTextBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 45, true);
			this.zTextBox2.Name = "zTextBox2";
			this.zTextBox2.ReadOnly = false;
			this.zTextBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.zTextBox2.TabIndex = 2;
			// 
			// disableOrgProxyRecipientOverride
			// 
			this.disableOrgProxyRecipientOverride.AutoSize = true;
			this.BindingSource.SetBindingMember(this.disableOrgProxyRecipientOverride, "EMP_DisableOrgProxyRecipientOverride");
			this.disableOrgProxyRecipientOverride.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.disableOrgProxyRecipientOverride.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 97, true);
			this.disableOrgProxyRecipientOverride.Name = "disableOrgProxyRecipientOverride";
			this.disableOrgProxyRecipientOverride.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.disableOrgProxyRecipientOverride.TabIndex = 4;
			// 
			// EDIMessagePurposeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.triggerPurposes);
			this.Name = "EDIMessagePurposeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 186, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.triggerPurposes.ResumeLayout(false);
			this.triggerPurposes.PerformLayout();
			this.zTextBox3.ResumeLayout(true);
			this.zTextBox3.PerformLayout();
			this.zTextBox2.ResumeLayout(true);
			this.zTextBox2.PerformLayout();
			this.disableOrgProxyRecipientOverride.ResumeLayout(true);
			this.disableOrgProxyRecipientOverride.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZGroupBox triggerPurposes;
		private ZArchitecture.ZTranslatableTextControl zTextBox2;
		private ZArchitecture.ZTextBox zTextBox1;
		private ZArchitecture.GUI.ZGuidFindBox zTextBox3;
		private ZArchitecture.GUI.ZCheckBox disableOrgProxyRecipientOverride;
	}
}
