namespace Enterprise.Customs.US.InBond.GUI
{
	partial class USInBondUserControl
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
			this.MainTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.BillsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MovementTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DocumentTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.usInBondHeaderDetailUserControl1 = new Enterprise.Customs.US.InBond.GUI.USInBondHeaderDetailUserControl(inBond);
			this.usInBondBillsUserControl1 = new Enterprise.Customs.US.InBond.GUI.USInBondBillsUserControl();
			this.usInBondMovementsDetailsUserControl1 = new Enterprise.Customs.US.InBond.GUI.USInBondMovementsDetailsUserControl();
			this.usInBond7512DataUserControl1 = new Enterprise.Customs.US.InBond.GUI.USInBond7512DataUserControl();
			this.usInBondMessageDetailsUserControl1 = new Enterprise.Customs.US.InBond.GUI.USInBondMessageDetailsUserControl();
			this.AirBillsAndMovementsDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AirInBondBillsAndMovementsDetailsUserControl = new Enterprise.Customs.US.InBond.GUI.USAirInBondBillsAndMovementsDetailsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.BillsTabPage.SuspendLayout();
			this.MovementTabPage.SuspendLayout();
			this.DocumentTabPage.SuspendLayout();
			this.MessagesTabPage.SuspendLayout();
			this.AirBillsAndMovementsDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MainTabControl.Controls.Add(this.DetailsTabPage);
			this.MainTabControl.Controls.Add(this.BillsTabPage);
			this.MainTabControl.Controls.Add(this.MovementTabPage);
			this.MainTabControl.Controls.Add(this.AirBillsAndMovementsDetailsTabPage);
			this.MainTabControl.Controls.Add(this.DocumentTabPage);
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainTabControl.Name = "MainTabControl";
			this.MainTabControl.SelectedIndex = 0;
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 650, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.usInBondHeaderDetailUserControl1);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 623, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.Text = "Details";
			// 
			// BillsTabPage
			// 
			this.BillsTabPage.Controls.Add(this.usInBondBillsUserControl1);
			this.BillsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.BillsTabPage.Name = "BillsTabPage";
			this.BillsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.BillsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 623, true);
			this.BillsTabPage.TabIndex = 1;
			this.BillsTabPage.Text = "Bills";
			// 
			// MovementTabPage
			// 
			this.MovementTabPage.Controls.Add(this.usInBondMovementsDetailsUserControl1);
			this.MovementTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MovementTabPage.Name = "MovementTabPage";
			this.MovementTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MovementTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 623, true);
			this.MovementTabPage.TabIndex = 2;
			this.MovementTabPage.Text = "Movement Details";
			// 
			// AirBillsAndMovementsDetailsTabPage
			// 
			this.AirBillsAndMovementsDetailsTabPage.Controls.Add(this.AirInBondBillsAndMovementsDetailsUserControl);
			this.AirBillsAndMovementsDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AirBillsAndMovementsDetailsTabPage.Name = "AirBillsAndMovementsDetailsTabPage";
			this.AirBillsAndMovementsDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AirBillsAndMovementsDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 623, true);
			this.AirBillsAndMovementsDetailsTabPage.TabIndex = 3;
			this.AirBillsAndMovementsDetailsTabPage.Text = "Bills/Movement Details";
			// 
			// AirInBondBillsAndMovementsDetailsUserControl
			// 
			this.AirInBondBillsAndMovementsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AirInBondBillsAndMovementsDetailsUserControl, ".");
			this.AirInBondBillsAndMovementsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AirInBondBillsAndMovementsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.AirInBondBillsAndMovementsDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.AirInBondBillsAndMovementsDetailsUserControl.Name = "AirInBondBillsAndMovementsDetailsUserControl";
			this.AirInBondBillsAndMovementsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 617, true);
			this.AirInBondBillsAndMovementsDetailsUserControl.TabIndex = 0;
			// 
			// DocumentTabPage
			// 
			this.DocumentTabPage.Controls.Add(this.usInBond7512DataUserControl1);
			this.DocumentTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DocumentTabPage.Name = "DocumentTabPage";
			this.DocumentTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DocumentTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 623, true);
			this.DocumentTabPage.TabIndex = 4;
			this.DocumentTabPage.Text = "7512 Data";
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Controls.Add(this.usInBondMessageDetailsUserControl1);
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1010, 623, true);
			this.MessagesTabPage.TabIndex = 5;
			this.MessagesTabPage.Text = "Messages";
			// 
			// usInBondHeaderDetailUserControl1
			// 
			this.usInBondHeaderDetailUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usInBondHeaderDetailUserControl1, ".");
			this.usInBondHeaderDetailUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usInBondHeaderDetailUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.usInBondHeaderDetailUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.usInBondHeaderDetailUserControl1.Name = "usInBondHeaderDetailUserControl1";
			this.usInBondHeaderDetailUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 617, true);
			this.usInBondHeaderDetailUserControl1.TabIndex = 0;
			// 
			// usInBondBillsUserControl1
			// 
			this.usInBondBillsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usInBondBillsUserControl1, ".");
			this.usInBondBillsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usInBondBillsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.usInBondBillsUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.usInBondBillsUserControl1.Name = "usInBondBillsUserControl1";
			this.usInBondBillsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 617, true);
			this.usInBondBillsUserControl1.TabIndex = 0;
			// 
			// usInBondMovementsDetailsUserControl1
			// 
			this.usInBondMovementsDetailsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usInBondMovementsDetailsUserControl1, ".");
			this.usInBondMovementsDetailsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usInBondMovementsDetailsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.usInBondMovementsDetailsUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.usInBondMovementsDetailsUserControl1.Name = "usInBondMovementsDetailsUserControl1";
			this.usInBondMovementsDetailsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 617, true);
			this.usInBondMovementsDetailsUserControl1.TabIndex = 0;
			// 
			// usInBond7512DataUserControl1
			// 
			this.usInBond7512DataUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usInBond7512DataUserControl1, ".");
			this.usInBond7512DataUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usInBond7512DataUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.usInBond7512DataUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.usInBond7512DataUserControl1.Name = "usInBond7512DataUserControl1";
			this.usInBond7512DataUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 617, true);
			this.usInBond7512DataUserControl1.TabIndex = 0;
			// 
			// usInBondMessageDetailsUserControl1
			// 
			this.usInBondMessageDetailsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.usInBondMessageDetailsUserControl1, ".");
			this.usInBondMessageDetailsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.usInBondMessageDetailsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.usInBondMessageDetailsUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.usInBondMessageDetailsUserControl1.Name = "usInBondMessageDetailsUserControl1";
			this.usInBondMessageDetailsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1004, 617, true);
			this.usInBondMessageDetailsUserControl1.TabIndex = 0;
			// 
			// USInBondUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.MainTabControl);
			this.Name = "USInBondUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1018, 650, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainTabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.BillsTabPage.ResumeLayout(false);
			this.MovementTabPage.ResumeLayout(false);
			this.DocumentTabPage.ResumeLayout(false);
			this.MessagesTabPage.ResumeLayout(false);
			this.AirBillsAndMovementsDetailsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		protected USInBondHeaderDetailUserControl fUSInBondHeaderDetailUserControl;
		protected USInBondBillsUserControl fUSInBondBillsUserControl;
		protected USInBondMovementsDetailsUserControl fUSInBondMovementsDetailsUserControl;
		protected USInBond7512DataUserControl fUSInBond7512DataUserControl;
		protected USInBondMessageDetailsUserControl fUSInBondMessageDetailsUserControl;
		public ZArchitecture.GUI.ZTabControl MainTabControl;
		public Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private USInBondHeaderDetailUserControl usInBondHeaderDetailUserControl1;
		public Enterprise.ZArchitecture.GUI.ZTabPage BillsTabPage;
		private USInBondBillsUserControl usInBondBillsUserControl1;
		public Enterprise.ZArchitecture.GUI.ZTabPage MovementTabPage;
		private USInBondMovementsDetailsUserControl usInBondMovementsDetailsUserControl1;
		public Enterprise.ZArchitecture.GUI.ZTabPage DocumentTabPage;
		private USInBond7512DataUserControl usInBond7512DataUserControl1;
		public Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private USInBondMessageDetailsUserControl usInBondMessageDetailsUserControl1;
		internal ZArchitecture.GUI.ZTabPage AirBillsAndMovementsDetailsTabPage;
		private USAirInBondBillsAndMovementsDetailsUserControl AirInBondBillsAndMovementsDetailsUserControl;
	}
}
