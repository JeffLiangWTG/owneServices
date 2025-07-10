namespace Enterprise.Customs.TR.GUI
{
	partial class NCTSPhase5CredentialsRegistryItemUserControl
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
		void InitializeComponent()
		{
			this.BasicAuthUsernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.BasicAuthPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FirmIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequestUserIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequestPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.NCTSPhase5Credentials);
			// 
			// BasicAuthUsernameTextBox
			// 
			this.BindingSource.SetBindingMember(this.BasicAuthUsernameTextBox, "BasicAuthUsername");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.NCTSPhase5Credentials)(null)).BasicAuthUsername)));
			this.BasicAuthUsernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BasicAuthUsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 0, true);
			this.BasicAuthUsernameTextBox.Name = "BasicAuthUsernameTextBox";
			this.BasicAuthUsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.BasicAuthUsernameTextBox.TabIndex = 1;
			// 
			// BasicAuthPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.BasicAuthPasswordTextBox, "BasicAuthPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.NCTSPhase5Credentials)(null)).BasicAuthPassword)));
			this.BasicAuthPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BasicAuthPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 20, true);
			this.BasicAuthPasswordTextBox.Name = "BasicAuthPasswordTextBox";
			this.BasicAuthPasswordTextBox.PasswordChar = '*';
			this.BasicAuthPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.BasicAuthPasswordTextBox.TabIndex = 2;
			// 
			// FirmIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.FirmIDTextBox, "FirmID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.NCTSPhase5Credentials)(null)).FirmID)));
			this.FirmIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FirmIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 40, true);
			this.FirmIDTextBox.Name = "FirmIDTextBox";
			this.FirmIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.FirmIDTextBox.TabIndex = 3;
			// 
			// RequestUserIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.RequestUserIDTextBox, "RequestUserID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.NCTSPhase5Credentials)(null)).RequestUserID)));
			this.RequestUserIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RequestUserIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 60, true);
			this.RequestUserIDTextBox.Name = "RequestUserIDTextBox";
			this.RequestUserIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.RequestUserIDTextBox.TabIndex = 4;
			// 
			// RequestPasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.RequestPasswordTextBox, "RequestPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.NCTSPhase5Credentials)(null)).RequestPassword)));
			this.RequestPasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.RequestPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(163, 80, true);
			this.RequestPasswordTextBox.Name = "RequestPasswordTextBox";
			this.RequestPasswordTextBox.PasswordChar = '*';
			this.RequestPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 40, true);
			this.RequestPasswordTextBox.TabIndex = 5;
			// 
			// NCTSPhase5CredentialsRegistryItemUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BasicAuthUsernameTextBox);
			this.Controls.Add(this.BasicAuthPasswordTextBox);
			this.Controls.Add(this.FirmIDTextBox);
			this.Controls.Add(this.RequestUserIDTextBox);
			this.Controls.Add(this.RequestPasswordTextBox);
			this.Name = "NCTSPhase5CredentialsRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 250, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.ZTextBox BasicAuthUsernameTextBox;
		public Enterprise.ZArchitecture.ZTextBox BasicAuthPasswordTextBox;
		public Enterprise.ZArchitecture.ZTextBox FirmIDTextBox;
		public Enterprise.ZArchitecture.ZTextBox RequestUserIDTextBox;
		public Enterprise.ZArchitecture.ZTextBox RequestPasswordTextBox;
	}
}
