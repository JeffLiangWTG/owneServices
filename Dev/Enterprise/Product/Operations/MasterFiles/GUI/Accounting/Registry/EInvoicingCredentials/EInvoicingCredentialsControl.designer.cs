namespace Enterprise.MasterFiles.GUI
{
	partial class EInvoicingCredentialsControl
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
			this.ViewButtonForClientSecret = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ApiKeyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GenerateAPIKeyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CopyApiKeyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EInvoicingCredentials);
			// 
			// ClientIdTextBox
			// 
			this.ClientIdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientIdTextBox, "ClientId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCredentials)(null)).ClientId)));
			this.ClientIdTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 30, true);
			this.ClientIdTextBox.Name = "ClientIdTextBox";
			this.ClientIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.ClientIdTextBox.TabIndex = 1;
			// 
			// ClientSecretTextBox
			// 
			this.ClientSecretTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ClientSecretTextBox, "ClientSecret");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCredentials)(null)).ClientSecret)));
			this.ClientSecretTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientSecretTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 56, true);
			this.ClientSecretTextBox.Name = "ClientSecretTextBox";
			this.ClientSecretTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.ClientSecretTextBox.TabIndex = 2;
			// 
			// ViewButtonForClientSecret
			// 
			this.ViewButtonForClientSecret.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ViewButtonForClientSecret.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("e930d120-bfb8-4f1c-8676-25eb120a9d3c", "View");
			this.ViewButtonForClientSecret.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 77, true);
			this.ViewButtonForClientSecret.Name = "ViewButtonForClientSecret";
			this.ViewButtonForClientSecret.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ViewButtonForClientSecret.TabIndex = 3;
			this.ViewButtonForClientSecret.ToolTipCaption = null;
			this.ViewButtonForClientSecret.Visible = false;
			this.ViewButtonForClientSecret.Click += new System.EventHandler(this.ViewButton_Click);
			// 
			// ApiKeyTextBox
			// 
			this.ApiKeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ApiKeyTextBox, "APIKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EInvoicingCredentials)(null)).APIKey)));
			this.ApiKeyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ApiKeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 107, true);
			this.ApiKeyTextBox.Name = "APIKeyTextBox";
			this.ApiKeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 20, true);
			this.ApiKeyTextBox.PasswordChar = '*';
			this.ApiKeyTextBox.ReadOnly = true;
			this.ApiKeyTextBox.TabIndex = 4;
			// 
			// GenerateApiKeyButton
			// 
			this.GenerateAPIKeyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.GenerateAPIKeyButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b274653b-7b9a-4ec6-8408-905a4335025b", "Generate API Key");
			this.GenerateAPIKeyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(211, 128, true);
			this.GenerateAPIKeyButton.Name = "GenerateAPIKeyButton";
			this.GenerateAPIKeyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 23, true);
			this.GenerateAPIKeyButton.TabIndex = 5;
			this.GenerateAPIKeyButton.ToolTipCaption = null;
			this.GenerateAPIKeyButton.Click += new System.EventHandler(this.GenerateAPIKeyButton_Click);
			// 
			// CopyApiKeyButton
			// 
			this.CopyApiKeyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CopyApiKeyButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("985f1c4e-06c0-49fd-863c-e2c563e8782b", "Copy");
			this.CopyApiKeyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 128, true);
			this.CopyApiKeyButton.Name = "CopyAPIKeyButton";
			this.CopyApiKeyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CopyApiKeyButton.TabIndex = 6;
			this.CopyApiKeyButton.ToolTipCaption = null;
			this.CopyApiKeyButton.Click += new System.EventHandler(this.CopyAPIKeyButton_Click);
			// 
			// EInvoicingCredentialsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CopyApiKeyButton);
			this.Controls.Add(this.GenerateAPIKeyButton);
			this.Controls.Add(this.ApiKeyTextBox);
			this.Controls.Add(this.ClientIdTextBox);
			this.Controls.Add(this.ClientSecretTextBox);
			this.Controls.Add(this.ViewButtonForClientSecret);
			this.Name = "EInvoicingCredentialsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 163, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.ZTextBox ClientIdTextBox;
		private Enterprise.ZArchitecture.ZTextBox ClientSecretTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton ViewButtonForClientSecret;
		private ZArchitecture.ZTextBox ApiKeyTextBox;
		private ZArchitecture.GUI.ZButton GenerateAPIKeyButton;
		private ZArchitecture.GUI.ZButton CopyApiKeyButton;

		#endregion
	}
}
