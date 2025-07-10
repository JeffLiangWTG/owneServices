namespace Enterprise.Customs.US.AMS.GUI
{
	partial class USAMSForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.usamsMainUserControl = new Enterprise.Customs.US.AMS.GUI.USAMSMainUserControl();
			this.billDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.usamsBillsUserControl = new Enterprise.Customs.US.AMS.GUI.USAMSBillsUserControl();
			this.permitToTransferTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.permitToTransferUserControl = new Enterprise.Customs.US.AMS.GUI.PermitToTransferUserControl();
			this.inBondTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.usamsNVOCCInBondUserControl = new Enterprise.Customs.US.AMS.GUI.USAMSNVOCCInBondUserControl();
			this.usamsInBondUserControl = new Enterprise.Customs.US.AMS.GUI.USAMSInBondUserControl();
			this.messageTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.messagesUserControl = new Enterprise.Customs.US.AMS.GUI.MessagesUserControl();
			this.USAMSSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.sailingUserControl = new Enterprise.Customs.US.AMS.GUI.SailingUserControl();
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.billDetailsTabPage.SuspendLayout();
			this.permitToTransferTabPage.SuspendLayout();
			this.inBondTabPage.SuspendLayout();
			this.messageTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.USAMSSplitContainer)).BeginInit();
			this.USAMSSplitContainer.Panel1.SuspendLayout();
			this.USAMSSplitContainer.Panel2.SuspendLayout();
			this.USAMSSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.billDetailsTabPage);
			this.MainTabControl.Controls.Add(this.permitToTransferTabPage);
			this.MainTabControl.Controls.Add(this.inBondTabPage);
			this.MainTabControl.Controls.Add(this.messageTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 631, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.messageTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.inBondTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.permitToTransferTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.billDetailsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("ZTemplateForm|C106AE89-03A9-4D35-AF4B-AE31C01882A2", "Main");
			this.MainTabPage.Controls.Add(this.USAMSSplitContainer);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 604, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 604, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 631, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.CusInBondHeader);
			// 
			// usamsMainUserControl
			// 
			this.usamsMainUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usamsMainUserControl, ".");
			this.usamsMainUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usamsMainUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.usamsMainUserControl.Name = "usamsMainUserControl";
			this.usamsMainUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 522, true);
			this.usamsMainUserControl.TabIndex = 0;
			// 
			// billDetailsTabPage
			// 
			this.billDetailsTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("2cffb448-9444-4ae8-b14f-a93fbee548ae", "Bill Details");
			this.billDetailsTabPage.Controls.Add(this.usamsBillsUserControl);
			this.billDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.billDetailsTabPage.Name = "billDetailsTabPage";
			this.billDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.billDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 485, true);
			this.billDetailsTabPage.TabIndex = 3;
			// 
			// usamsBillsUserControl
			// 
			this.usamsBillsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usamsBillsUserControl, ".");
			this.usamsBillsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usamsBillsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.usamsBillsUserControl.Name = "usamsBillsUserControl";
			this.usamsBillsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 479, true);
			this.usamsBillsUserControl.TabIndex = 0;
			// 
			// permitToTransferTabPage
			// 
			this.permitToTransferTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("48a832a1-1937-4b5d-b7cc-a818031a2a87", "Permit to Transfer");
			this.permitToTransferTabPage.Controls.Add(this.permitToTransferUserControl);
			this.permitToTransferTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.permitToTransferTabPage.Name = "permitToTransferTabPage";
			this.permitToTransferTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.permitToTransferTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 485, true);
			this.permitToTransferTabPage.TabIndex = 4;
			// 
			// permitToTransferUserControl
			// 
			this.permitToTransferUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.permitToTransferUserControl, ".");
			this.permitToTransferUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.permitToTransferUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.permitToTransferUserControl.Name = "permitToTransferUserControl";
			this.permitToTransferUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 479, true);
			this.permitToTransferUserControl.TabIndex = 0;
			// 
			// inBondTabPage
			// 
			this.inBondTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("86c42837-0a6e-4e85-9536-c4e9af09129a", "In-Bond");
			this.inBondTabPage.Controls.Add(this.usamsNVOCCInBondUserControl);
			this.inBondTabPage.Controls.Add(this.usamsInBondUserControl);
			this.inBondTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.inBondTabPage.Name = "inBondTabPage";
			this.inBondTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.inBondTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 485, true);
			this.inBondTabPage.TabIndex = 5;
			// 
			// usamsNVOCCInBondUserControl
			// 
			this.usamsNVOCCInBondUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usamsNVOCCInBondUserControl, ".");
			this.usamsNVOCCInBondUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usamsNVOCCInBondUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.usamsNVOCCInBondUserControl.Name = "usamsNVOCCInBondUserControl";
			this.usamsNVOCCInBondUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 479, true);
			this.usamsNVOCCInBondUserControl.TabIndex = 0;
			// 
			// usamsInBondUserControl
			// 
			this.usamsInBondUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usamsInBondUserControl, ".");
			this.usamsInBondUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usamsInBondUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.usamsInBondUserControl.Name = "usamsInBondUserControl";
			this.usamsInBondUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 479, true);
			this.usamsInBondUserControl.TabIndex = 0;
			// 
			// messageTabPage
			// 
			this.messageTabPage.CaptionResourceString = Enterprise.Customs.US.AMS.GUI.Res.GetData("f90b534b-761f-45e4-9e97-b1dc60bc4775", "Messages");
			this.messageTabPage.Controls.Add(this.messagesUserControl);
			this.messageTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messageTabPage.Name = "messageTabPage";
			this.messageTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.messageTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 485, true);
			this.messageTabPage.TabIndex = 6;
			// 
			// messagesUserControl
			// 
			this.messagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagesUserControl, ".");
			this.messagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messagesUserControl.Name = "messagesUserControl";
			this.messagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 479, true);
			this.messagesUserControl.TabIndex = 0;
			// 
			// USAMSSplitContainer
			// 
			this.USAMSSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.USAMSSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.USAMSSplitContainer.IsSplitterFixed = true;
			this.USAMSSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.USAMSSplitContainer.Name = "USAMSSplitContainer";
			this.USAMSSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// USAMSSplitContainer.Panel1
			// 
			this.USAMSSplitContainer.Panel1.Controls.Add(this.sailingUserControl);
			// 
			// USAMSSplitContainer.Panel2
			// 
			this.USAMSSplitContainer.Panel2.Controls.Add(this.usamsMainUserControl);
			this.USAMSSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 604, true);
			this.USAMSSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(78);
			this.USAMSSplitContainer.TabIndex = 1;
			this.USAMSSplitContainer.TabStop = false;
			// 
			// sailingUserControl
			// 
			this.sailingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sailingUserControl, ".");
			this.sailingUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sailingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sailingUserControl.Name = "sailingUserControl";
			this.sailingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(984, 78, true);
			this.sailingUserControl.TabIndex = 0;
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 485, true);
			this.WorkflowTabPage.TabIndex = 7;
			// 
			// USAMSForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(992, 687, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.AMS.Business.CusInBondHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 725, true);
			this.Name = "USAMSForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "USAMSForm";
			this.CaptionRenderingEnabled = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.billDetailsTabPage.ResumeLayout(false);
			this.permitToTransferTabPage.ResumeLayout(false);
			this.inBondTabPage.ResumeLayout(false);
			this.messageTabPage.ResumeLayout(false);
			this.USAMSSplitContainer.Panel1.ResumeLayout(false);
			this.USAMSSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.USAMSSplitContainer)).EndInit();
			this.USAMSSplitContainer.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		USAMSMainUserControl usamsMainUserControl;
		private ZArchitecture.GUI.ZTabPage billDetailsTabPage;
		private USAMSBillsUserControl usamsBillsUserControl;
		private ZArchitecture.GUI.ZTabPage permitToTransferTabPage;
		private PermitToTransferUserControl permitToTransferUserControl;
		private ZArchitecture.GUI.ZTabPage inBondTabPage;
		private ZArchitecture.GUI.ZTabPage messageTabPage;
		private USAMSInBondUserControl usamsInBondUserControl;
		private USAMSNVOCCInBondUserControl usamsNVOCCInBondUserControl;
		private MessagesUserControl messagesUserControl;
		private CargoWise.Windows.UI.KSplitContainer USAMSSplitContainer;
		internal SailingUserControl sailingUserControl;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;

	}
}
