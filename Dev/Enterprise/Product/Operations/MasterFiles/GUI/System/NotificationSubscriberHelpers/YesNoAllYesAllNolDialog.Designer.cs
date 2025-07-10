namespace Enterprise.MasterFiles.GUI
{
	partial class YesNoAllYesAllNoAllDialog
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
			this.NoToAllButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 62, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// YesButton
			// 
			this.YesButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.YesButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("YesNoAllYesAllNoAllDialog|91ab293d-0934-4a08-851d-c7937003199a", "Yes");
			this.YesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 55, true);
			this.YesButton.Name = "YesButton";
			this.YesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.YesButton.TabIndex = 1;
			this.YesButton.Click += new System.EventHandler(this.YesButton_Click);
			// 
			// NoButton
			// 
			this.NoButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.NoButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("YesNoAllYesAllNoAllDialog|f3da57f0-2281-4f46-a122-5b03cba5194f", "No");
			this.NoButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 55, true);
			this.NoButton.Name = "NoButton";
			this.NoButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.NoButton.TabIndex = 2;
			this.NoButton.Click += new System.EventHandler(this.NoButton_Click);
			// 
			// YesToAllButton
			// 
			this.YesToAllButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.YesToAllButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("YesNoAllYesAllNoAllDialog|fe114c39-98cb-498f-a04f-1c431a3dbbf1", "Yes to all");
			this.YesToAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(164, 55, true);
			this.YesToAllButton.Name = "YesToAllButton";
			this.YesToAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.YesToAllButton.TabIndex = 3;
			this.YesToAllButton.Click += new System.EventHandler(this.YesToAllButton_Click);
			// 
			// NoToAllButton
			// 
			this.NoToAllButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.NoToAllButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("YesNoAllYesAllNoAllDialog|1db27ca6-a00d-4e56-9a2f-eb6a369f2073", "No to all");
			this.NoToAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(244, 55, true);
			this.NoToAllButton.Name = "NoToAllButton";
			this.NoToAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.NoToAllButton.TabIndex = 4;
			this.NoToAllButton.Click += new System.EventHandler(this.NoToAllButton_Click);
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
			// YesNoAllYesAllNoAllDialog
			// 
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 86, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("YesNoAllYesAllNoAllDialog|82034f41-ab88-4354-b79a-62d8e4c528e7", "Notification");
			this.Controls.Add(this.YesButton);
			this.Controls.Add(this.NoButton);
			this.Controls.Add(this.YesToAllButton);
			this.Controls.Add(this.NoToAllButton);
			this.Controls.Add(this.MessageLabel);
			this.Cursor = System.Windows.Forms.Cursors.Arrow;
			this.MinimizeBox = false;
			this.Name = "YesNoAllYesAllNoAllDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.NoToAllButton, 0);
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
		protected Enterprise.ZArchitecture.GUI.ZButton NoToAllButton;
	}
}
