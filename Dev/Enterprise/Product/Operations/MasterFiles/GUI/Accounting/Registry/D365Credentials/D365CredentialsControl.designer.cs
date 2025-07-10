namespace Enterprise.MasterFiles.GUI
{
	partial class D365CredentialsControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ClientIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientSecretTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TenantIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.D365Credentials);
			// 
			// ClientIdTextBox
			// 
			this.ClientIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientIdTextBox, "ClientID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.D365Credentials)(null)).ClientID)));
			this.ClientIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 30, true);
			this.ClientIdTextBox.Name = "ClientIdTextBox";
			this.ClientIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.ClientIdTextBox.TabIndex = 1;
			// 
			// ClientSecretTextBox
			// 
			this.ClientSecretTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientSecretTextBox, "ClientSecret");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.D365Credentials)(null)).ClientSecret)));
			this.ClientSecretTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientSecretTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 56, true);
			this.ClientSecretTextBox.Name = "ClientSecretTextBox";
			this.ClientSecretTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.ClientSecretTextBox.TabIndex = 2;
			// 
			// TenantIDTextBox
			// 
			this.TenantIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TenantIDTextBox, "TenantID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.D365Credentials)(null)).TenantID)));
			this.TenantIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.TenantIDTextBox.Enabled = false;
			this.TenantIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 84, true);
			this.TenantIDTextBox.Name = "TenantIDTextBox";
			this.TenantIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 17, true);
			this.TenantIDTextBox.TabIndex = 4;
			// 
			// D365CredentialsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.TenantIDTextBox);
			this.Controls.Add(this.ClientIdTextBox);
			this.Controls.Add(this.ClientSecretTextBox);
			this.Name = "D365CredentialsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 132, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZTextBox ClientIdTextBox;
		private Enterprise.ZArchitecture.ZTextBox ClientSecretTextBox;

		#endregion

		private ZArchitecture.ZTextBox TenantIDTextBox;
	}
}
