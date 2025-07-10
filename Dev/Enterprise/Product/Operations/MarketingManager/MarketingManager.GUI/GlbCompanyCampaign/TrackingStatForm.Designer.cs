namespace Enterprise.MarketingManager.GUI
{
	partial class TrackingStatForm
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
		protected override void InitializeComponent()
		{
			this.clickStatUserControl = new Enterprise.MarketingManager.GUI.CampaignClickStatDetailsUserControl();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 737, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MarketingManager.Business.ClickStatModel);
			// 
			// linkTrackCampaignClickStatUserControl1
			// 
			this.clickStatUserControl.AllowDrop = true;
			this.clickStatUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.clickStatUserControl, ".");
			this.clickStatUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.clickStatUserControl.Name = "linkTrackCampaignClickStatUserControl1";
			this.clickStatUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 704, true);
			this.clickStatUserControl.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("b9ae41b0-1937-4feb-af1b-c44b99d09386", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1058, 707, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(114, 24, true);
			this.CloseButton.TabIndex = 2;
			this.CloseButton.UseVisualStyleBackColor = true;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// TrackingStatForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.Color.White;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("c7d29652-c6fb-4ecb-98f7-20419d435805", "Timeline View");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1184, 761, true);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.clickStatUserControl);
			this.DataSourceType = typeof(Enterprise.MarketingManager.Business.ClickStatModel);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 725, true);
			this.Name = "TrackingStatForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.clickStatUserControl, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CampaignClickStatDetailsUserControl clickStatUserControl;
		private ZArchitecture.GUI.ZButton CloseButton;
	}
}