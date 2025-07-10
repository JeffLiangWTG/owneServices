using CargoWise.Windows.UI;

namespace Enterprise.MasterFiles.GUI
{
	partial class MDMSupportCertificateUserControl
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
			this.clientIdBox = new Enterprise.ZArchitecture.ZTextBox();
			this.tenantIdBox = new Enterprise.ZArchitecture.ZTextBox();
			this.targetClientIdBox = new Enterprise.ZArchitecture.ZTextBox();
			this.loadCertificateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.loadPrivateKeyButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.certificateBox = new Enterprise.ZArchitecture.ZTextBox();
			this.privateKeyBox = new Enterprise.ZArchitecture.ZTextBox();
			this.panel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.SystemToSystemTrustInfo);
			// 
			// clientIdBox
			// 
			this.BindingSource.SetBindingMember(this.clientIdBox, "ClientId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Registry.Business.SystemToSystemTrustInfo)(null)).ClientId)));
			this.clientIdBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("67ba7744-cda0-472d-bfb1-6fe58ce99057", "Client ID");
			this.clientIdBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 17, true);
			this.clientIdBox.Name = "clientIdBox";
			this.clientIdBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 17, true);
			this.clientIdBox.TabIndex = 0;
			this.clientIdBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
			// 
			// tenantIdBox
			// 
			this.BindingSource.SetBindingMember(this.tenantIdBox, "TenantId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Registry.Business.SystemToSystemTrustInfo)(null)).TenantId)));
			this.tenantIdBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("16ab002c-3087-4a59-8dc0-98af3d6c4c8b", "Tenant ID");
			this.tenantIdBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 44, true);
			this.tenantIdBox.Name = "tenantIdBox";
			this.tenantIdBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 17, true);
			this.tenantIdBox.TabIndex = 1;
			this.tenantIdBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
			// 
			// targetClientIdBox
			// 
			this.BindingSource.SetBindingMember(this.targetClientIdBox, "OperationId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Registry.Business.SystemToSystemTrustInfo)(null)).OperationId)));
			this.targetClientIdBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 73, true);
			this.targetClientIdBox.Name = "targetClientIdBox";
			this.targetClientIdBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 17, true);
			this.targetClientIdBox.TabIndex = 2;
			this.targetClientIdBox.TextChanged += new System.EventHandler(this.TextBox_TextChanged);
			// 
			// loadCertificateButton
			// 
			this.loadCertificateButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("64a9cf64-d072-4326-917c-cbd048e7bb8f", "Load Certificate");
			this.loadCertificateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 103, true);
			this.loadCertificateButton.Name = "loadCertificateButton";
			this.loadCertificateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 19, true);
			this.loadCertificateButton.TabIndex = 3;
			this.loadCertificateButton.ToolTipCaption = null;
			this.loadCertificateButton.Click += new System.EventHandler(this.LoadCertificateButton_Click);
			// 
			// loadPrivateKeyButton
			// 
			this.loadPrivateKeyButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dc98a024-601e-4985-b6cc-46e8a474e859", "Load Private Key");
			this.loadPrivateKeyButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 136, true);
			this.loadPrivateKeyButton.Name = "loadPrivateKeyButton";
			this.loadPrivateKeyButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 19, true);
			this.loadPrivateKeyButton.TabIndex = 5;
			this.loadPrivateKeyButton.ToolTipCaption = null;
			this.loadPrivateKeyButton.Click += new System.EventHandler(this.LoadPrivateKeyButton_Click);
			// 
			// certificateBox
			// 
			this.certificateBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("bfaa6633-ce0a-487a-ae16-cb0806b12591", "Certificate");
			this.certificateBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 104, true);
			this.certificateBox.Name = "certificateBox";
			this.certificateBox.ReadOnly = true;
			this.certificateBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.certificateBox.TabIndex = 4;
			// 
			// privateKeyBox
			// 
			this.privateKeyBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("39c1e774-18e5-436d-9b6d-1048449e6781", "Private Key");
			this.privateKeyBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 137, true);
			this.privateKeyBox.Name = "privateKeyBox";
			this.privateKeyBox.ReadOnly = true;
			this.privateKeyBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 17, true);
			this.privateKeyBox.TabIndex = 6;
			// 
			// panel
			// 
			this.panel.Controls.Add(this.clientIdBox);
			this.panel.Controls.Add(this.tenantIdBox);
			this.panel.Controls.Add(this.targetClientIdBox);
			this.panel.Controls.Add(this.loadCertificateButton);
			this.panel.Controls.Add(this.certificateBox);
			this.panel.Controls.Add(this.loadPrivateKeyButton);
			this.panel.Controls.Add(this.privateKeyBox);
			this.panel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.panel.Name = "panel";
			this.panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 400, true);
			this.panel.TabIndex = 0;
			this.panel.TabStop = false;
			// 
			// MDMSupportCertificateUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.panel);
			this.Name = "MDMSupportCertificateUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panel.ResumeLayout(false);
			this.panel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		protected Enterprise.ZArchitecture.ZTextBox clientIdBox;
		protected Enterprise.ZArchitecture.ZTextBox tenantIdBox;
		protected Enterprise.ZArchitecture.ZTextBox targetClientIdBox;
		protected Enterprise.ZArchitecture.GUI.ZButton loadCertificateButton;
		protected Enterprise.ZArchitecture.GUI.ZButton loadPrivateKeyButton;
		protected Enterprise.ZArchitecture.ZTextBox certificateBox;
		protected Enterprise.ZArchitecture.ZTextBox privateKeyBox;
		private Enterprise.ZArchitecture.GUI.ZPanel panel;

		#endregion
	}
}
