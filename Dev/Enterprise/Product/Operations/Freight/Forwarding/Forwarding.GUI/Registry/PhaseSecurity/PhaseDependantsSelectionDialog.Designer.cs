namespace Enterprise.Freight.Forwarding.GUI
{
	partial class PhaseDependantsSelectionDialog
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
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelZButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.InformationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.InformationLabel = new Enterprise.ZArchitecture.ZLabel();
			this.DependantsTreeView = new CargoWise.Windows.UI.KTreeView();
			this.MandatoryPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.MandatoryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonsPanel.SuspendLayout();
			this.InformationPanel.SuspendLayout();
			this.MandatoryPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 438, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 24, true);
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.OKButton);
			this.ButtonsPanel.Controls.Add(this.CancelZButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 406, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 32, true);
			this.ButtonsPanel.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PhaseDependantsSelectionDialog|f8bc7e4d-d54f-4be1-86a7-403b67e142bf", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(366, 5, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.OKButton.TabIndex = 1;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelZButton
			// 
			this.CancelZButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PhaseDependantsSelectionDialog|905ec33c-b19a-4923-b3ff-aecc1fa27fdd", "Cancel");
			this.CancelZButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelZButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(472, 5, true);
			this.CancelZButton.Name = "CancelZButton";
			this.CancelZButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CancelZButton.TabIndex = 0;
			this.CancelZButton.UseVisualStyleBackColor = true;
			this.CancelZButton.Click += new System.EventHandler(this.CancelZButton_Click);
			// 
			// InformationPanel
			// 
			this.InformationPanel.Controls.Add(this.InformationLabel);
			this.InformationPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.InformationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InformationPanel.Name = "InformationPanel";
			this.InformationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 30, true);
			this.InformationPanel.TabIndex = 3;
			// 
			// InformationLabel
			// 
			this.InformationLabel.AutoSize = true;
			this.InformationLabel.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PhaseDependantsSelectionDialog|907f2491-2dfb-452a-8ef7-119d10257ef8", "Select properties/collections/child objects that should be forced to be mandatory or read-only");
			this.InformationLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.InformationLabel.Name = "InformationLabel";
			this.InformationLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(463, 14, true);
			this.InformationLabel.TabIndex = 0;
			// 
			// DependantsTreeView
			// 
			this.DependantsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
				| System.Windows.Forms.AnchorStyles.Left)
				| System.Windows.Forms.AnchorStyles.Right)));
			this.DependantsTreeView.CheckBoxes = true;
			this.DependantsTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 36, true);
			this.DependantsTreeView.Name = "DependantsTreeView";
			this.DependantsTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 342, true);
			this.DependantsTreeView.TabIndex = 1;
			// 
			// MandatoryPanel
			// 
			this.MandatoryPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.MandatoryPanel.Controls.Add(this.MandatoryCheckBox);
			this.MandatoryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 378, true);
			this.MandatoryPanel.Name = "MandatoryPanel";
			this.MandatoryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 28, true);
			this.MandatoryPanel.TabIndex = 4;
			// 
			// MandatoryCheckBox
			// 
			this.MandatoryCheckBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("c7633110-f2ba-4db9-bab6-3440734f795d", "Mandatory");
			this.MandatoryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.MandatoryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 4, true);
			this.MandatoryCheckBox.Name = "MandatoryCheckBox";
			this.MandatoryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 19, true);
			this.MandatoryCheckBox.TabIndex = 0;
			this.MandatoryCheckBox.UseVisualStyleBackColor = true;
			// 
			// PhaseDependantsSelectionDialog
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelZButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("PhaseDependantsSelectionDialog|f8da79d9-3e42-4c3a-90d9-282a42f4b6f7", "Read Only Properties Selection");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 462, true);
			this.Controls.Add(this.MandatoryPanel);
			this.Controls.Add(this.InformationPanel);
			this.Controls.Add(this.DependantsTreeView);
			this.Controls.Add(this.ButtonsPanel);
			this.MinimizeBox = false;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 500, true);
			this.Name = "PhaseDependantsSelectionDialog";
			this.Text = "PhaseDependantsSelectionDialog";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonsPanel, 0);
			this.Controls.SetChildIndex(this.DependantsTreeView, 0);
			this.Controls.SetChildIndex(this.InformationPanel, 0);
			this.Controls.SetChildIndex(this.MandatoryPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.InformationPanel.ResumeLayout(false);
			this.InformationPanel.PerformLayout();
			this.MandatoryPanel.ResumeLayout(false);
			this.MandatoryPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel ButtonsPanel;
		private ZArchitecture.GUI.ZPanel InformationPanel;
		protected ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton CancelZButton;
		protected CargoWise.Windows.UI.KTreeView DependantsTreeView;
		private ZArchitecture.ZLabel InformationLabel;
		private ZArchitecture.GUI.ZPanel MandatoryPanel;
		protected ZArchitecture.GUI.ZCheckBox MandatoryCheckBox;
	}
}
