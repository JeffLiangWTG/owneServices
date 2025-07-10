using Enterprise.Messaging.GUI;

namespace Enterprise.Freight.CFS.GUI
{
	partial class HtmlInterpretationForm
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
			this.components = new System.ComponentModel.Container();
			this.HtmlInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.ButtonPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ButtonPanel.SuspendLayout();
			this.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 276, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// HtmlInterpretationBox
			// 
			this.HtmlInterpretationBox.AllowWebBrowserDrop = false;
			this.HtmlInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HtmlInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HtmlInterpretationBox.Name = "HtmlInterpretationBox";
			this.HtmlInterpretationBox.ScriptErrorsSuppressed = true;
			this.HtmlInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 237, true);
			this.HtmlInterpretationBox.TabIndex = 0;
			// 
			// ButtonPanel
			// 
			this.ButtonPanel.BackColor = System.Drawing.SystemColors.Control;
			this.ButtonPanel.Controls.Add(this.OKButton);
			this.ButtonPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 237, true);
			this.ButtonPanel.Name = "ButtonPanel";
			this.ButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 39, true);
			this.ButtonPanel.TabIndex = 1;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = System.Windows.Forms.AnchorStyles.Top;
			this.OKButton.CaptionResourceString = Enterprise.Freight.CFS.GUI.Res.GetData("a897b099-0607-445c-9dc9-e0d797289e48", "OK");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(116, 8, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 0;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// MessageDetailsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 300, true);
			this.Controls.Add(this.HtmlInterpretationBox);
			this.Controls.Add(this.ButtonPanel);
			this.Name = "HtmlInterpretationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ButtonPanel, 0);
			this.Controls.SetChildIndex(this.HtmlInterpretationBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ButtonPanel.ResumeLayout(false);
			this.ButtonPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		protected HtmlInterpretationBox HtmlInterpretationBox;
		private Enterprise.ZArchitecture.GUI.ZPanel ButtonPanel;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
	}
}