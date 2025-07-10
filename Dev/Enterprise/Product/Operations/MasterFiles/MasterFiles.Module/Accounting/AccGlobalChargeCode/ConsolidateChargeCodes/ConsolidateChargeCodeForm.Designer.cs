namespace Enterprise.MasterFiles.Module
{
	partial class ConsolidateChargeCodeForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.FilterControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.panelOkCancelButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ConsolidateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.panelOkCancelButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 365, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 24, true);
			// 
			// FilterControlPanel
			// 
			this.FilterControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FilterControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterControlPanel.Name = "FilterControlPanel";
			this.FilterControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 328, true);
			this.FilterControlPanel.TabIndex = 1;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.panelOkCancelButtons);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 328, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 37, true);
			this.zPanel1.TabIndex = 2;
			// 
			// panelOkCancelButtons
			// 
			this.panelOkCancelButtons.Controls.Add(this.ConsolidateButton);
			this.panelOkCancelButtons.Controls.Add(this.CloseButton);
			this.panelOkCancelButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelOkCancelButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(546, 0, true);
			this.panelOkCancelButtons.Name = "panelOkCancelButtons";
			this.panelOkCancelButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 37, true);
			this.panelOkCancelButtons.TabIndex = 1;
			// 
			// ConsolidateButton
			// 
			this.ConsolidateButton.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("ConsolidateChargeCodeForm|2b271b75-589f-4cc4-813f-77c5d5f3e206", "Consolidate");
			this.ConsolidateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.ConsolidateButton.Name = "ConsolidateButton";
			this.ConsolidateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.ConsolidateButton.TabIndex = 0;
			this.ConsolidateButton.Click += new System.EventHandler(this.OnConsolidateClicked);
			// 
			// CloseButton
			// 
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("ConsolidateChargeCodeForm|0c5dc0c8-44e0-491c-8d5e-38f5054a1cba", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 7, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.Click += new System.EventHandler(this.Close_Button_Click);
			// 
			// ConsolidateChargeCodeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("ConsolidateChargeCodeForm|4fa19f3f-cc21-487e-b101-af5bafefa3ca", "Consolidate Local Charge Codes");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 389, true);
			this.Controls.Add(this.FilterControlPanel);
			this.Controls.Add(this.zPanel1);
			this.Name = "ConsolidateChargeCodeForm";
			this.Text = "Consolidate Local Charge Codes";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.FilterControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.panelOkCancelButtons.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.GUI.ZPanel FilterControlPanel;
		protected ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.GUI.ZPanel panelOkCancelButtons;
		private ZArchitecture.GUI.ZButton ConsolidateButton;
		private ZArchitecture.GUI.ZButton CloseButton;
	}
}