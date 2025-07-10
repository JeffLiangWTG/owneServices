using CargoWise.Windows.UI.Testing;

using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.MasterFiles.GUI
{
	[SuppressFormDesignerAnalysis]
	public partial class OrganisationEmdeddedModulePopup : EmbeddedModulePopup
	{
		#region Windows Form Designer generated code

		internal ZButton TemporaryButton;
		ZPanel panelTemporaryButton;

		new void InitializeComponent()
		{
			this.panelTemporaryButton = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TemporaryButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.panelTemporaryButton.SuspendLayout();
			this.SuspendLayout();
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.Controls.Add(this.panelTemporaryButton);
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 237, true);
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 37, true);
			this.ButtonPanel.Controls.SetChildIndex(this.panelTemporaryButton, 0);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 274, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(176);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(177);
			// 
			// panelTemporaryButton
			// 
			this.panelTemporaryButton.Controls.Add(this.TemporaryButton);
			this.panelTemporaryButton.Dock = System.Windows.Forms.DockStyle.Right;
			this.panelTemporaryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(66, 0, true);
			this.panelTemporaryButton.Name = "panelTemporaryButton";
			this.panelTemporaryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(128, 37, true);
			this.panelTemporaryButton.TabIndex = 0;
			// 
			// TemporaryButton
			// 
			this.TemporaryButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("OrganisationEmdeddedModulePopup|10bce126-27b5-4e63-a49e-5bd3cb236017", "Temporary Account", "Creates a Temporary Organization Account.");
			this.TemporaryButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 7, true);
			this.TemporaryButton.Name = "TemporaryButton";
			this.TemporaryButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.TemporaryButton.TabIndex = 0;
			this.TemporaryButton.Click += new System.EventHandler(this.TemporaryButton_Click);
			// 
			// OrganisationEmdeddedModulePopup
			// 

			this.Name = "OrganisationEmdeddedModulePopup";
			this.ButtonPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.panelTemporaryButton.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		#endregion
	}
}
