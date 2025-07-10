namespace Enterprise.Customs.US.DIS.GUI
{
	partial class DISForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelOrCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.disUserControl1 = new Enterprise.Customs.US.DIS.GUI.DISUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.disUserControl1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 659, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.DIS.Business.DISHostWrapper);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.SendButton);
			this.MainPanel.Controls.Add(this.CancelOrCloseButton);
			this.MainPanel.Controls.Add(this.SaveButton);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 628, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 31, true);
			this.MainPanel.TabIndex = 1;
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(475, 5, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
			this.SendButton.TabIndex = 0;
			this.SendButton.Text = "&Send";
			this.SendButton.UseVisualStyleBackColor = true;
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// CancelOrCloseButton
			// 
			this.CancelOrCloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelOrCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelOrCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(693, 5, true);
			this.CancelOrCloseButton.Name = "CancelOrCloseButton";
			this.CancelOrCloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelOrCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelOrCloseButton.TabIndex = 2;
			this.CancelOrCloseButton.Text = "&Close";
			this.CancelOrCloseButton.UseVisualStyleBackColor = true;
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(614, 5, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 1;
			this.SaveButton.Text = "S&ave";
			this.SaveButton.UseVisualStyleBackColor = true;
			// 
			// disUserControl1
			// 
			this.disUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.disUserControl1, ".");
			this.disUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.disUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.disUserControl1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(778, 627, true);
			this.disUserControl1.Name = "disUserControl1";
			this.disUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 628, true);
			this.disUserControl1.TabIndex = 2;
			// 
			// DISForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelOrCloseButton;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 683, true);
			this.Controls.Add(this.disUserControl1);
			this.Controls.Add(this.MainPanel);
			this.DataSourceType = typeof(Enterprise.Customs.US.DIS.Business.DISHostWrapper);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(795, 720, true);
			this.Name = "DISForm";
			this.Text = "DISForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.Controls.SetChildIndex(this.disUserControl1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.disUserControl1.ResumeLayout(true);
			this.disUserControl1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel MainPanel;
		internal ZArchitecture.GUI.ZButton SaveButton;
		internal DISUserControl disUserControl1;
		internal ZArchitecture.GUI.ZButton SendButton;
		internal ZArchitecture.GUI.ZButton CancelOrCloseButton;
	}
}