using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	partial class AuditMessageForm : ZChildForm
	{
		ZButton WriteToLogButton;
		ZButton CloseButton;
		ZTextBox ReferenceTextBox;

		protected override void InitializeComponent()
		{
			this.WriteToLogButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 85, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// WriteToLogButton
			// 
			this.WriteToLogButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.WriteToLogButton.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.WriteToLogButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AuditMessageForm|71e9cd63-5af4-48af-a208-d48d375f8cd5", "Log && Close");
			this.WriteToLogButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 56, true);
			this.WriteToLogButton.Name = "WriteToLogButton";
			this.WriteToLogButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.WriteToLogButton.TabIndex = 2;
			this.WriteToLogButton.UseVisualStyleBackColor = true;
			// 
			// ReferenceTextBox
			// 
			this.ReferenceTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.ReferenceTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ReferenceTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AuditMessageForm|fa494196-411a-42d9-9d30-2a5bebe35bd1", "Reference");
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 8, true);
			this.ReferenceTextBox.Multiline = true;
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(403, 42, true);
			this.ReferenceTextBox.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AuditMessageForm|e93d42f9-7d28-466f-8de6-87e11b3f0d53", "Cancel");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 56, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(82, 23, true);
			this.CloseButton.TabIndex = 3;
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// AuditMessageForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(487, 109, true);
			this.Controls.Add(this.ReferenceTextBox);
			this.Controls.Add(this.WriteToLogButton);
			this.Controls.Add(this.CloseButton);
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 149, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(495, 143, true);
			this.Name = "AuditMessageForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.WriteToLogButton, 0);
			this.Controls.SetChildIndex(this.ReferenceTextBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
