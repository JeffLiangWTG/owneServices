namespace Enterprise.Customs.TW.GUI
{
	partial class TWNCATKClientSettingRegistryItemUserControl
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
			this.MachineNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RunningIntervalInSecondsCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.SendFolderTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EHubClientIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.EHubClientStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.TWNCATKClientSetting);
			// 
			// MachineNameTextBox
			// 
			this.MachineNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MachineNameTextBox, "MachineName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWNCATKClientSetting)(null)).MachineName)));
			this.MachineNameTextBox.CaptionResourceString = null;
			this.MachineNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MachineNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 26, true);
			this.MachineNameTextBox.Name = "MachineNameTextBox";
			this.MachineNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.MachineNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.MachineNameTextBox.TabIndex = 0;
			// 
			// RunningIntervalInSecondsCalcEdit
			// 
			this.RunningIntervalInSecondsCalcEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.RunningIntervalInSecondsCalcEdit, "RunningIntervalInSeconds");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.TW.Business.TWNCATKClientSetting)(null)).RunningIntervalInSeconds)));
			this.RunningIntervalInSecondsCalcEdit.CaptionResourceString = null;
			this.RunningIntervalInSecondsCalcEdit.DecimalPlaces = 0;
			this.RunningIntervalInSecondsCalcEdit.Decimals = 0;
			this.RunningIntervalInSecondsCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 104, true);
			this.RunningIntervalInSecondsCalcEdit.Name = "RunningIntervalInSecondsCalcEdit";
			this.RunningIntervalInSecondsCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.RunningIntervalInSecondsCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.RunningIntervalInSecondsCalcEdit.TabIndex = 3;
			this.RunningIntervalInSecondsCalcEdit.Text = "0";
			this.RunningIntervalInSecondsCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// SendFolderTextBox
			// 
			this.SendFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SendFolderTextBox, "SendToFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWNCATKClientSetting)(null)).SendToFolder)));
			this.SendFolderTextBox.CaptionResourceString = null;
			this.SendFolderTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 130, true);
			this.SendFolderTextBox.Name = "SendFolderTextBox";
			this.SendFolderTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.SendFolderTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.SendFolderTextBox.TabIndex = 4;
			// 
			// EHubClientIDTextBox
			// 
			this.EHubClientIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EHubClientIDTextBox, "EHubClientID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWNCATKClientSetting)(null)).EHubClientID)));
			this.EHubClientIDTextBox.CaptionResourceString = null;
			this.EHubClientIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 52, true);
			this.EHubClientIDTextBox.Name = "EHubClientIDTextBox";
			this.EHubClientIDTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.EHubClientIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.EHubClientIDTextBox.TabIndex = 1;
			// 
			// EHubClientStatusTextBox
			// 
			this.EHubClientStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EHubClientStatusTextBox, "EHubClientStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.TWNCATKClientSetting)(null)).EHubClientStatus)));
			this.EHubClientStatusTextBox.CaptionResourceString = null;
			this.EHubClientStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(173, 78, true);
			this.EHubClientStatusTextBox.Name = "EHubClientStatusTextBox";
			this.EHubClientStatusTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.EHubClientStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 20, true);
			this.EHubClientStatusTextBox.TabIndex = 2;
			// 
			// TWNCATKClientSettingRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.EHubClientStatusTextBox);
			this.Controls.Add(this.EHubClientIDTextBox);
			this.Controls.Add(this.SendFolderTextBox);
			this.Controls.Add(this.MachineNameTextBox);
			this.Controls.Add(this.RunningIntervalInSecondsCalcEdit);
			this.Name = "TWNCATKClientSettingRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 180, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZTextBox MachineNameTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit RunningIntervalInSecondsCalcEdit;

		#endregion

		private ZArchitecture.ZTextBox SendFolderTextBox;
		private ZArchitecture.ZTextBox EHubClientIDTextBox;
		private ZArchitecture.ZTextBox EHubClientStatusTextBox;
	}
}
