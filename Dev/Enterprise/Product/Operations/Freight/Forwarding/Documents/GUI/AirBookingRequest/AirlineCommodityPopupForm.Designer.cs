namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	partial class AirlineCommodityPopupForm
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

		protected new void InitializeComponent()
		{
			this.buttonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.panelOkCancelButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OkBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.filterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.buttonPanel.SuspendLayout();
			this.panelOkCancelButtons.SuspendLayout();
			this.SuspendLayout();
			//
			// ButtonPanel
			//
			this.buttonPanel.Controls.Add(this.panelOkCancelButtons);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			this.buttonPanel.Name = "buttonPanel";
			this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 37, true);
			this.buttonPanel.TabIndex = 2;
			//
			// panelOkCancelButtons
			//
			this.panelOkCancelButtons.Controls.Add(this.OkBtn);
			this.panelOkCancelButtons.Controls.Add(this.CancelBtn);
			this.panelOkCancelButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelOkCancelButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(918, 0, true);
			this.panelOkCancelButtons.Name = "panelOkCancelButtons";
			this.panelOkCancelButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 37, true);
			this.panelOkCancelButtons.TabIndex = 1;
			//
			// OkBtn
			//
			this.OkBtn.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("AirlineCommodityPopupForm|ffd211d8-47fa-4555-8185-e1f69100f90c", "OK");
			this.OkBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.OkBtn.Name = "okBtn";
			this.OkBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OkBtn.TabIndex = 0;
			this.OkBtn.Click += new System.EventHandler(this.OkBtn_Click);
			//
			// CancelBtn
			//
			CancelBtn.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("AirlineCommodityPopupForm|110ac777-f8c4-49a6-bb26-19e4648f4215", "Cancel");
			CancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 7, true);
			CancelBtn.Name = "cancelBtn";
			CancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			CancelBtn.TabIndex = 1;
			CancelBtn.Click += new System.EventHandler(this.CancelBtn_Clicked);
			//
			// FilterControlPanel
			//
			this.filterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 32, true);
			this.filterPanel.Name = "filterPanel";
			this.filterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 203, true);
			this.filterPanel.TabIndex = 1;
			//
			// AirlineCommodityPopupForm
			//
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.Documents.GUI.Res.GetData("73593fb8-d9d6-41aa-94e8-8d99c70d648e", "Goods Description Lookup");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 294, true);
			this.Controls.Add(this.filterPanel);
			this.Controls.Add(this.buttonPanel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 305, true);
			this.Name = "AirlineCommodityPopupForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.buttonPanel, 0);
			this.Controls.SetChildIndex(this.filterPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.panelOkCancelButtons.ResumeLayout(false);
			this.panelOkCancelButtons.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected Enterprise.ZArchitecture.GUI.ZButton CancelBtn;
		protected Enterprise.ZArchitecture.GUI.ZButton OkBtn;

		Enterprise.ZArchitecture.GUI.ZPanel panelOkCancelButtons;
		Enterprise.ZArchitecture.GUI.ZPanel buttonPanel;
		Enterprise.ZArchitecture.GUI.ZPanel filterPanel;
	}
}
