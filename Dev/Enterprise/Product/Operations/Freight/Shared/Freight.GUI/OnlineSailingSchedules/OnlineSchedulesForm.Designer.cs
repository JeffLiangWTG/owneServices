namespace Enterprise.Freight.GUI.OnlineSailingSchedules
{
	partial class OnlineSchedulesForm
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
		protected new void InitializeComponent()
		{
			this.FilterPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ImportButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 739, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.OnlineSailingSchedules.OnlineSchedules);
			// 
			// FilterPanel
			// 
			this.FilterPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterPanel.BackColor = System.Drawing.SystemColors.Control;
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 1, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 703, true);
			this.FilterPanel.TabIndex = 2;
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("688ec02a-6156-41bd-ac46-491f1ce00b55", "Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(750, 708, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 27, true);
			this.ImportButton.TabIndex = 0;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("f5d65e9d-95de-4159-a1fd-0bcf1ba1e34c", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(834, 708, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 27, true);
			this.CloseButton.TabIndex = 1;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// OnlineSchedulesForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("44646b69-be04-47f3-9f17-ced4ce6f775a", "Global Sailing Schedules");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(922, 763, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.FilterPanel);
			this.DataSourceType = typeof(Enterprise.Freight.OnlineSailingSchedules.OnlineSchedules);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 700, true);
			this.Name = "OnlineSchedulesForm";
			this.Text = "Global Sailing Schedules";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FilterPanel, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel FilterPanel;
		protected ZArchitecture.GUI.ZButton CloseButton;
		protected ZArchitecture.GUI.ZButton ImportButton;
	}
}