namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class CartageTypeDropModeDialog
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
		public new void InitializeComponent()
		{
			this.MessageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JobTypeButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.UseJobTypeDropModeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddressButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.UseAddressDropModeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.KeepExistingButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.JobTypeButtonPanel.SuspendLayout();
			this.AddressButtonPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Enabled = false;
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 167, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 28, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageLabel
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageLabel, false);
			this.MessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 12, true);
			this.MessageLabel.Name = "MessageLabel";
			this.MessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 39, true);
			this.MessageLabel.TabIndex = 17;
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.JobTypeButtonPanel);
			this.ButtonPanel.Controls.Add(this.AddressButtonPanel);
			this.ButtonPanel.Controls.Add(this.KeepExistingButton);
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 75, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 107, true);
			this.ButtonPanel.TabIndex = 21;
			// 
			// JobTypeButtonPanel
			// 
			this.JobTypeButtonPanel.Controls.Add(this.UseJobTypeDropModeButton);
			this.JobTypeButtonPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.JobTypeButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 65, true);
			this.JobTypeButtonPanel.Name = "JobTypeButtonPanel";
			this.JobTypeButtonPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 0, true);
			this.JobTypeButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 42, true);
			this.JobTypeButtonPanel.TabIndex = 26;
			this.JobTypeButtonPanel.Visible = false;
			// 
			// UseJobTypeDropModeButton
			// 
			this.UseJobTypeDropModeButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UseJobTypeDropModeButton.Enabled = false;
			this.UseJobTypeDropModeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.UseJobTypeDropModeButton.Name = "UseJobTypeDropModeButton";
			this.UseJobTypeDropModeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 39, true);
			this.UseJobTypeDropModeButton.TabIndex = 25;
			this.UseJobTypeDropModeButton.Visible = false;
			this.UseJobTypeDropModeButton.Click += new System.EventHandler(this.UseJobTypeDropModeButton_Click);
			// 
			// AddressButtonPanel
			// 
			this.AddressButtonPanel.AccessibleDescription = "";
			this.AddressButtonPanel.Controls.Add(this.UseAddressDropModeButton);
			this.AddressButtonPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.AddressButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 23, true);
			this.AddressButtonPanel.Name = "AddressButtonPanel";
			this.AddressButtonPanel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 3, 0, 0, true);
			this.AddressButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 42, true);
			this.AddressButtonPanel.TabIndex = 24;
			this.AddressButtonPanel.Visible = false;
			// 
			// UseAddressDropModeButton
			// 
			this.UseAddressDropModeButton.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UseAddressDropModeButton.Enabled = false;
			this.UseAddressDropModeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.UseAddressDropModeButton.Name = "UseAddressDropModeButton";
			this.UseAddressDropModeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 39, true);
			this.UseAddressDropModeButton.TabIndex = 1;
			this.UseAddressDropModeButton.Visible = false;
			this.UseAddressDropModeButton.Click += new System.EventHandler(this.UseAddressDropModeButton_Click);
			// 
			// KeepExistingButton
			// 
			this.KeepExistingButton.Dock = System.Windows.Forms.DockStyle.Top;
			this.KeepExistingButton.Enabled = false;
			this.KeepExistingButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.KeepExistingButton.Name = "KeepExistingButton";
			this.KeepExistingButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 23, true);
			this.KeepExistingButton.TabIndex = 20;
			this.KeepExistingButton.Visible = false;
			this.KeepExistingButton.Click += new System.EventHandler(this.KeepExistingButton_Click);
			// 
			// CartageTypeDropModeDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(370, 195, true);
			this.Controls.Add(this.ButtonPanel);
			this.Controls.Add(this.MessageLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 233, true);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 233, true);
			this.Name = "CartageTypeDropModeDialog";
			this.RememberFormSize = false;
			this.ShowIcon = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MessageLabel, 0);
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.JobTypeButtonPanel.ResumeLayout(false);
			this.AddressButtonPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		internal ZArchitecture.ZLabel MessageLabel;
		ZArchitecture.GUI.ZPanel ButtonPanel;
		internal ZArchitecture.GUI.ZButton KeepExistingButton;
		internal ZArchitecture.GUI.ZPanel AddressButtonPanel;
		internal ZArchitecture.GUI.ZButton UseAddressDropModeButton;
		internal ZArchitecture.GUI.ZPanel JobTypeButtonPanel;
		internal ZArchitecture.GUI.ZButton UseJobTypeDropModeButton;
	}
}