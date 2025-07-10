namespace Enterprise.Customs.US.InBond.GUI
{
	partial class USInBondForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.BillsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InBondBillsUserControl = new Enterprise.Customs.US.InBond.GUI.USInBondBillsUserControl();
			this.MovementDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InBondMovementsDetailsUserControl = new Enterprise.Customs.US.InBond.GUI.USInBondMovementsDetailsUserControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InBondMessageDetailsUserControl = new Enterprise.Customs.US.InBond.GUI.USInBondMessageDetailsUserControl();
			this.CBP7512TabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InBond7512DataUserControl = new Enterprise.Customs.US.InBond.GUI.USInBond7512DataUserControl();
			this.InBondHeaderDetailsUserControl = new Enterprise.Customs.US.InBond.GUI.USInBondHeaderDetailUserControl(BusinessEntity);
			this.AirBillsAndMovementsDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AirInBondBillsAndMovementsDetailsUserControl = new Enterprise.Customs.US.InBond.GUI.USAirInBondBillsAndMovementsDetailsUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BillsTabPage.SuspendLayout();
			this.MovementDetailsTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.CBP7512TabPage.SuspendLayout();
			this.AirBillsAndMovementsDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.BillsTabPage);
			this.MainTabControl.Controls.Add(this.MovementDetailsTabPage);
			this.MainTabControl.Controls.Add(this.AirBillsAndMovementsDetailsTabPage);
			this.MainTabControl.Controls.Add(this.CBP7512TabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 576, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CBP7512TabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.AirBillsAndMovementsDetailsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MovementDetailsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.BillsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.InBondHeaderDetailsUserControl);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 549, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 549, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 576, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.InBond.Business.CusInBondHeader);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 549, true);
			this.WorkflowTabPage.TabIndex = 4;
			// 
			// BillsTabPage
			// 
			this.BillsTabPage.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("USInBondForm|214e9bb1-7743-462b-9c41-991ca9ab93a7", "Bills");
			this.BillsTabPage.Controls.Add(this.InBondBillsUserControl);
			this.BillsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BillsTabPage.Name = "BillsTabPage";
			this.BillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 549, true);
			this.BillsTabPage.TabIndex = 3;
			// 
			// InBondBillsUserControl
			// 
			this.InBondBillsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InBondBillsUserControl, ".");
			this.InBondBillsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondBillsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InBondBillsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.InBondBillsUserControl.Name = "InBondBillsUserControl";
			this.InBondBillsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 549, true);
			this.InBondBillsUserControl.TabIndex = 0;
			// 
			// MovementDetailsTabPage
			// 
			this.MovementDetailsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MovementDetailsTabPage.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("USInBondForm|fa744282-794c-4638-8395-e2790a9ce962", "Movements");
			this.MovementDetailsTabPage.Controls.Add(this.InBondMovementsDetailsUserControl);
			this.MovementDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MovementDetailsTabPage.Name = "MovementDetailsTabPage";
			this.MovementDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MovementDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(968, 430, true);
			this.MovementDetailsTabPage.TabIndex = 4;
			this.MovementDetailsTabPage.Text = "Movement Details";
			// 
			// InBondMovementsDetailsUserControl
			// 
			this.InBondMovementsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InBondMovementsDetailsUserControl, ".");
			this.InBondMovementsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondMovementsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InBondMovementsDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.InBondMovementsDetailsUserControl.Name = "InBondMovementsDetailsUserControl";
			this.InBondMovementsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.InBondMovementsDetailsUserControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("USInBondForm|70de8a89-c293-4e34-b211-7311ec0268e6", "Messages");
			this.MessagesTabPage.CheckForNotifications = false;
			this.MessagesTabPage.Controls.Add(this.InBondMessageDetailsUserControl);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 549, true);
			this.MessagesTabPage.TabIndex = 5;
			// 
			// InBondMessageDetailsUserControl
			// 
			this.InBondMessageDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InBondMessageDetailsUserControl, ".");
			this.InBondMessageDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondMessageDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InBondMessageDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.InBondMessageDetailsUserControl.Name = "InBondMessageDetailsUserControl";
			this.InBondMessageDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 543, true);
			this.InBondMessageDetailsUserControl.TabIndex = 0;
			// 
			// CBP7512TabPage
			// 
			this.CBP7512TabPage.BackColor = System.Drawing.SystemColors.Control;
			this.CBP7512TabPage.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("USInBondForm|1744c9b0-4a8f-46bd-8258-89fec5e036a4", "7512 Data");
			this.CBP7512TabPage.Controls.Add(this.InBond7512DataUserControl);
			this.CBP7512TabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CBP7512TabPage.Name = "CBP7512TabPage";
			this.CBP7512TabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CBP7512TabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 549, true);
			this.CBP7512TabPage.TabIndex = 6;
			// 
			// InBond7512DataUserControl
			// 
			this.InBond7512DataUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InBond7512DataUserControl, ".");
			this.InBond7512DataUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBond7512DataUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.InBond7512DataUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.InBond7512DataUserControl.Name = "InBond7512DataUserControl";
			this.InBond7512DataUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 543, true);
			this.InBond7512DataUserControl.TabIndex = 0;
			// 
			// InBondHeaderDetailsUserControl
			// 
			this.InBondHeaderDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.InBondHeaderDetailsUserControl, ".");
			this.InBondHeaderDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondHeaderDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InBondHeaderDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.InBondHeaderDetailsUserControl.Name = "InBondHeaderDetailsUserControl";
			this.InBondHeaderDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 549, true);
			this.InBondHeaderDetailsUserControl.TabIndex = 0;
			// 
			// AirBillsAndMovementsDetailsTabPage
			// 
			this.AirBillsAndMovementsDetailsTabPage.CaptionResourceString = Enterprise.Customs.US.InBond.GUI.Res.GetData("USInBondForm|57d420f0-67f8-48be-be1e-9383e489dc34", "Bills/Movement Details");
			this.AirBillsAndMovementsDetailsTabPage.Controls.Add(this.AirInBondBillsAndMovementsDetailsUserControl);
			this.AirBillsAndMovementsDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AirBillsAndMovementsDetailsTabPage.Name = "AirBillsAndMovementsDetailsTabPage";
			this.AirBillsAndMovementsDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AirBillsAndMovementsDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 549, true);
			this.AirBillsAndMovementsDetailsTabPage.TabIndex = 1;
			this.AirBillsAndMovementsDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// AirInBondBillsAndMovementsDetailsUserControl
			// 
			this.AirInBondBillsAndMovementsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirInBondBillsAndMovementsDetailsUserControl, ".");
			this.AirInBondBillsAndMovementsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AirInBondBillsAndMovementsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AirInBondBillsAndMovementsDetailsUserControl.Name = "AirInBondBillsAndMovementsDetailsUserControl";
			this.AirInBondBillsAndMovementsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(994, 543, true);
			this.AirInBondBillsAndMovementsDetailsUserControl.TabIndex = 0;
			// 
			// USInBondForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 668, true);
			this.DataSourceType = typeof(Enterprise.Customs.US.InBond.Business.CusInBondHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 670, true);
			this.Name = "USInBondForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "USInBondForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BillsTabPage.ResumeLayout(false);
			this.MovementDetailsTabPage.ResumeLayout(false);
			this.MessagesTabPage.ResumeLayout(false);
			this.CBP7512TabPage.ResumeLayout(false);
			this.AirBillsAndMovementsDetailsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZTabPage MovementDetailsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage BillsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage CBP7512TabPage;
		private Enterprise.Customs.US.InBond.GUI.USInBond7512DataUserControl InBond7512DataUserControl;
		private Enterprise.Customs.US.InBond.GUI.USInBondMovementsDetailsUserControl InBondMovementsDetailsUserControl;
		private Enterprise.Customs.US.InBond.GUI.USInBondMessageDetailsUserControl InBondMessageDetailsUserControl;
		internal Enterprise.Customs.US.InBond.GUI.USInBondBillsUserControl InBondBillsUserControl;
		private Enterprise.Customs.US.InBond.GUI.USInBondHeaderDetailUserControl InBondHeaderDetailsUserControl;
		internal ZArchitecture.GUI.ZTabPage AirBillsAndMovementsDetailsTabPage;
		private USAirInBondBillsAndMovementsDetailsUserControl AirInBondBillsAndMovementsDetailsUserControl;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;

	}
}
