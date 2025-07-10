namespace Enterprise.Customs.GUI
{
	partial class MessagesChooserDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		public Enterprise.ZArchitecture.GUI.ZCheckedListBox MessagesCheckedListBox;
		private Enterprise.ZArchitecture.GUI.ZPanel ButtonPanel;
		internal Enterprise.ZArchitecture.GUI.ZButton SendButton;
		private CargoWise.Windows.UI.KButton SelectAllButton;
		private CargoWise.Windows.UI.KButton DeselectAllButton;
		private System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DeselectAllButton = new CargoWise.Windows.UI.KButton();
			this.SelectAllButton = new CargoWise.Windows.UI.KButton();
			this.SendButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Dock = System.Windows.Forms.DockStyle.None;
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 357, true);
			this.MainStatusBar.ShowPanels = false;
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(8, 7, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.DeselectAllButton);
			this.ButtonPanel.Controls.Add(this.SelectAllButton);
			this.ButtonPanel.Controls.Add(this.SendButton);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 30, true);
			this.ButtonPanel.TabIndex = 1;
			// 
			// DeselectAllButton
			// 
			this.DeselectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 5, true);
			this.DeselectAllButton.Name = "DeselectAllButton";
			this.DeselectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.DeselectAllButton.TabIndex = 1;
			this.DeselectAllButton.Text = "Deselect All";
			this.DeselectAllButton.Click += new System.EventHandler(this.DeselectAllButton_Click);
			// 
			// SelectAllButton
			// 
			this.SelectAllButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 4, true);
			this.SelectAllButton.Name = "SelectAllButton";
			this.SelectAllButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SelectAllButton.TabIndex = 0;
			this.SelectAllButton.Text = "Select All";
			this.SelectAllButton.Click += new System.EventHandler(this.SelectAllButton_Click);
			// 
			// SendButton
			// 
			this.SendButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(308, 5, true);
			this.SendButton.Name = "SendButton";
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.SendButton.TabIndex = 2;
			this.SendButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("858F6B2C-C375-4EF7-A0F2-395EB1184E7D", "Send");
			this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
			// 
			// MessagesChooserDialog
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 222, true);
			this.Controls.Add(this.ButtonPanel);
			InitializeMessageListControl();
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceTypeName = "Enterprise.Customs.Business.MessageChooserNonPersistent";
			this.Name = "MessagesChooserDialog";
			this.SendButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("489154FA-9B4E-4CA9-89DB-F68C498F4127", "Messages Chooser Dialog");
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}
