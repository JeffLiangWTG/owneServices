using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class NewOrAttachConsolidatedDeclarationForm
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

			if (jobDeclarationModule != null)
			{
				jobDeclarationModule.Dispose();
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
			this.buttonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.filterControlPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.panelCreateCancelButtons = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.createButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.closeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.filterControlPanel.SuspendLayout();
			this.buttonPanel.SuspendLayout();
			this.panelCreateCancelButtons.SuspendLayout();
			this.SuspendLayout();
			// 
			// buttonPanel
			// 
			this.buttonPanel.Controls.Add(this.panelCreateCancelButtons);
			this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.buttonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 235, true);
			this.buttonPanel.Name = "ButtonPanel";
			this.buttonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1084, 37, true);
			this.buttonPanel.TabIndex = 2;
			// 
			// panelCreateCancelButtons
			// 
			this.panelCreateCancelButtons.Controls.Add(this.createButton);
			this.panelCreateCancelButtons.Controls.Add(this.closeButton);
			this.panelCreateCancelButtons.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelCreateCancelButtons.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(918, 0, true);
			this.panelCreateCancelButtons.Name = "panelOkCancelButtons";
			this.panelCreateCancelButtons.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(166, 37, true);
			this.panelCreateCancelButtons.TabIndex = 1;
			// 
			// CreateButton
			// 
			this.createButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.createButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.createButton.Name = "CreateButton";
			this.createButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.createButton.TabIndex = 1;
			this.createButton.Click += new System.EventHandler(this.CreateButton_Click);
			// 
			// CloseButton
			// 
			this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.closeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 7, true);
			this.closeButton.Name = "CloseButton";
			this.closeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 23, true);
			this.closeButton.TabIndex = 2;
			this.closeButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("A862B67B-CFA7-4519-941E-BE3508E6D629", "Close");
			this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// FilterControlPanel
			// 
			this.filterControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.filterControlPanel.Name = "FilterControlPanel";
			this.filterControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 293, true);
			this.filterControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.filterControlPanel.TabIndex = 1;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 76, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 24, true);
			// 
			// NewConsolidatedDeclarationForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 358, true);
			this.Controls.Add(this.buttonPanel);
			this.Controls.Add(this.filterControlPanel);
			this.Name = "NewConsolidatedDeclarationForm";
			this.Controls.SetChildIndex(this.buttonPanel, 0);
			this.Controls.SetChildIndex(this.filterControlPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.buttonPanel.ResumeLayout(false);
			this.buttonPanel.PerformLayout();
			this.panelCreateCancelButtons.ResumeLayout(false);
			this.panelCreateCancelButtons.PerformLayout();
			this.filterControlPanel.ResumeLayout(false);
			this.filterControlPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZButton createButton;
		private Enterprise.ZArchitecture.GUI.ZButton closeButton;
		private ZPanel filterControlPanel;
		private ZPanel buttonPanel;
		private ZPanel panelCreateCancelButtons;
	}
}
