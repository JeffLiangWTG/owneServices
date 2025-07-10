namespace Enterprise.MasterFiles.GUI
{
	partial class YesNoYesAllDialog
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.YesButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.NoButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.YesToAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 62, true);
			this.MainStatusBar.Visible = false;
			// 
			// YesButton
			// 
			this.YesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.YesButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("YesNoYesAllDialog|ab77db31-7c47-4bae-8701-aa38584066a6", "Yes");
			this.YesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(32, 55, true);
			this.YesButton.Name = "YesButton";
			this.YesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.YesButton.TabIndex = 1;
			this.YesButton.Click += new System.EventHandler(this.YesButton_Click);
			// 
			// NoButton
			// 
			this.NoButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.NoButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("YesNoYesAllDialog|8669f34a-f29a-4756-935b-11bfe2c808ea", "No");
			this.NoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 55, true);
			this.NoButton.Name = "NoButton";
			this.NoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.NoButton.TabIndex = 2;
			this.NoButton.Click += new System.EventHandler(this.NoButton_Click);
			// 
			// YesToAllButton
			// 
			this.YesToAllButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.YesToAllButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("YesNoYesAllDialog|7cc8b57a-b780-4079-9424-43506ad93d09", "Yes to all");
			this.YesToAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 55, true);
			this.YesToAllButton.Name = "YesToAllButton";
			this.YesToAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.YesToAllButton.TabIndex = 3;
			this.YesToAllButton.Click += new System.EventHandler(this.YesToAllButton_Click);
			// 
			// MessageLabel
			// 
			this.MessageLabel.AutoSize = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageLabel, false);
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.MessageLabel.TabIndex = 5;
			// 
			// YesNoYesAllDialog
			// 
			this.AutoSize = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 86, true);
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.YesButton);
			this.Controls.Add(this.NoButton);
			this.Controls.Add(this.YesToAllButton);
			this.Controls.Add(this.MessageLabel);
			this.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.MinimizeBox = false;
			this.Name = "YesNoYesAllDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.YesToAllButton, 0);
			this.Controls.SetChildIndex(this.NoButton, 0);
			this.Controls.SetChildIndex(this.YesButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZButton YesButton;
		protected Enterprise.ZArchitecture.GUI.ZButton NoButton;
		protected Enterprise.ZArchitecture.GUI.ZButton YesToAllButton;
		protected Enterprise.ZArchitecture.ZLabel MessageLabel;
	}
}
