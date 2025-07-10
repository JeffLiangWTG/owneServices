namespace Enterprise.Customs.US.AMS.GUI
{
	partial class USAMSConsolManifestUserControl
	{
		private ZArchitecture.GUI.ZTabControl MainTabControl;
		private ZArchitecture.GUI.ZTabPage MainTabPage;
		private ZArchitecture.GUI.ZTabPage BillsTabPage;

		private void InitializeComponent()
		{
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.MainTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.usamsMainUserControl1 = new Enterprise.Customs.US.AMS.GUI.USAMSMainUserControl();
			this.BillsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillsDetailsUserControl = new Enterprise.Customs.US.AMS.GUI.USAMSBillsUserControl();
			this.PTTTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PermitToTransferUserControl = new Enterprise.Customs.US.AMS.GUI.PermitToTransferUserControl();
			this.InBondTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AMSInBondUserControl = new Enterprise.Customs.US.AMS.GUI.USAMSInBondUserControl();
			this.AMSNVOCCInBondUserControl = new Enterprise.Customs.US.AMS.GUI.USAMSNVOCCInBondUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesUserControl = new Enterprise.Customs.US.AMS.GUI.MessagesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.BillsTabPage.SuspendLayout();
			this.PTTTabPage.SuspendLayout();
			this.InBondTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.CusInBondHeader);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.MainTabPage);
			this.MainTabControl.Controls.Add(this.BillsTabPage);
			this.MainTabControl.Controls.Add(this.PTTTabPage);
			this.MainTabControl.Controls.Add(this.InBondTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(898, 620, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("USAMSConsolManifestUserControl|39051fe5-d8c0-4dd0-992c-ebdde5617a1e", "Main");
			this.MainTabPage.Controls.Add(this.usamsMainUserControl1);
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MainTabPage.Name = "MainTabPage";
			this.MainTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 593, true);
			this.MainTabPage.TabIndex = 0;
			// 
			// usamsMainUserControl1
			// 
			this.usamsMainUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usamsMainUserControl1, ".");
			this.usamsMainUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usamsMainUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.usamsMainUserControl1.Name = "usamsMainUserControl1";
			this.usamsMainUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 587, true);
			this.usamsMainUserControl1.TabIndex = 0;
			// 
			// BillsTabPage
			// 
			this.BillsTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("USAMSConsolManifestUserControl|19129186-b4f9-4074-8b48-edf05d59e619", "Bills Details");
			this.BillsTabPage.Controls.Add(this.BillsDetailsUserControl);
			this.BillsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BillsTabPage.Name = "BillsTabPage";
			this.BillsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(890, 593, true);
			this.BillsTabPage.TabIndex = 1;
			// 
			// BillsDetailsUserControl
			// 
			this.BillsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BillsDetailsUserControl, ".");
			this.BillsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BillsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.BillsDetailsUserControl.Name = "BillsDetailsUserControl";
			this.BillsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 587, true);
			this.BillsDetailsUserControl.TabIndex = 0;
			// 
			// PTTTabPage
			// 
			this.PTTTabPage.Controls.Add(this.PermitToTransferUserControl);
			this.PTTTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.PTTTabPage.Name = "PTTTabPage";
			this.PTTTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 593, true);
			this.PTTTabPage.TabIndex = 3;
			this.PTTTabPage.Text = "Permit To Transfer";
			// 
			// PermitToTransferUserControl
			// 
			this.PermitToTransferUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PermitToTransferUserControl, ".");
			this.PermitToTransferUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PermitToTransferUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PermitToTransferUserControl.Name = "PermitToTransferUserControl";
			this.PermitToTransferUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 593, true);
			this.PermitToTransferUserControl.TabIndex = 0;
			// 
			// InBondTabPage
			// 
			this.InBondTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("46903338-d7c3-426a-b2cc-884d7710733f", "In-Bond");
			this.InBondTabPage.Controls.Add(this.AMSInBondUserControl);
			this.InBondTabPage.Controls.Add(this.AMSNVOCCInBondUserControl);
			this.InBondTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InBondTabPage.Name = "InBondTabPage";
			this.InBondTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InBondTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 593, true);
			this.InBondTabPage.TabIndex = 3;
			// 
			// AMSInBondUserControl
			// 
			this.AMSInBondUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AMSInBondUserControl, ".");
			this.AMSInBondUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AMSInBondUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AMSInBondUserControl.Name = "AMSInBondUserControl";
			this.AMSInBondUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 587, true);
			this.AMSInBondUserControl.TabIndex = 2;
			// 
			// AMSNVOCCInBondUserControl
			// 
			this.AMSNVOCCInBondUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AMSNVOCCInBondUserControl, ".");
			this.AMSNVOCCInBondUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AMSNVOCCInBondUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AMSNVOCCInBondUserControl.Name = "AMSNVOCCInBondUserControl";
			this.AMSNVOCCInBondUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 587, true);
			this.AMSNVOCCInBondUserControl.TabIndex = 2;
			this.AMSNVOCCInBondUserControl.TabStop = false;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("USAMSConsolManifestUserControl|61b88fc6-16f3-4dac-9c73-e17c29138aeb", "Messages");
			this.MessagesTabPage.Controls.Add(this.MessagesUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 593, true);
			this.MessagesTabPage.TabIndex = 2;
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, ".");
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 587, true);
			this.MessagesUserControl.TabIndex = 1;
			this.MessagesUserControl.TabStop = false;
			// 
			// USAMSConsolManifestUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainTabControl);
			this.Name = "USAMSConsolManifestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(898, 620, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.BillsTabPage.ResumeLayout(false);
			this.PTTTabPage.ResumeLayout(false);
			this.InBondTabPage.ResumeLayout(false);
			this.MessagesTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		private ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private ZArchitecture.GUI.ZTabPage PTTTabPage;
		private ZArchitecture.GUI.ZTabPage InBondTabPage;
		private USAMSBillsUserControl BillsDetailsUserControl;
		private PermitToTransferUserControl PermitToTransferUserControl;
		private USAMSInBondUserControl AMSInBondUserControl;
		private USAMSNVOCCInBondUserControl AMSNVOCCInBondUserControl;
		private MessagesUserControl MessagesUserControl;
		private USAMSMainUserControl usamsMainUserControl1;
	}
}
