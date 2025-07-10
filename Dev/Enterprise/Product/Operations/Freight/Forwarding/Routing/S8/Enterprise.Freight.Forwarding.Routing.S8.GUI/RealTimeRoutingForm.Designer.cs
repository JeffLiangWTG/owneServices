using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI
{
	partial class RealTimeRoutingForm
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
			this.ImportAndCreateMAWBsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 513, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(917, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Routing.S8.Business.RoutingManager);
			// 
			// FilterPanel
			// 
			this.FilterPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FilterPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FilterPanel.Name = "FilterPanel";
			this.FilterPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 482, true);
			this.FilterPanel.TabIndex = 3;
			// 
			// ImportAndCreateMAWBsButton
			// 
			this.ImportAndCreateMAWBsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportAndCreateMAWBsButton.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingForm|ABDA7574-D309-4A6F-8370-207A6B038327", "Import and Create MAWBs");
			this.ImportAndCreateMAWBsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(598, 487, true);
			this.ImportAndCreateMAWBsButton.Name = "ImportAndCreateMAWBsButton";
			this.ImportAndCreateMAWBsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 23, true);
			this.ImportAndCreateMAWBsButton.TabIndex = 4;
			this.ImportAndCreateMAWBsButton.UseVisualStyleBackColor = true;
			this.ImportAndCreateMAWBsButton.Click += new System.EventHandler(this.ImportAndCreateMAWBsButton_Click);
			// 
			// ImportButton
			// 
			this.ImportButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportButton.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("75c989b9-f22f-4d34-8997-aa60a8d0bce9", "Import");
			this.ImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(755, 487, true);
			this.ImportButton.Name = "ImportButton";
			this.ImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ImportButton.TabIndex = 5;
			this.ImportButton.UseVisualStyleBackColor = true;
			this.ImportButton.Click += new System.EventHandler(this.ImportButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingForm|3ff48893-8927-4c8e-82d1-950c8174349b", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(834, 487, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 6;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// RealTimeRoutingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ImportAndCreateMAWBsButton);
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(915, 537, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.Routing.S8.GUI.Res.GetData("RealTimeRoutingForm|30acd9e2-4230-4ba7-98bc-a6f2402e924f", "Global Flight Schedules");
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.ImportButton);
			this.Controls.Add(this.FilterPanel);
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Routing.S8.Business.RoutingManager);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(931, 573, true);
			this.Name = "RealTimeRoutingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FilterPanel, 0);
			this.Controls.SetChildIndex(this.ImportButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.ImportAndCreateMAWBsButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

		private ZPanel FilterPanel;
		private ZButton CloseButton;
		protected ZButton ImportButton;
		protected ZButton ImportAndCreateMAWBsButton;
	}
}
