using Enterprise.ZArchitecture.GUI;

using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class SecuritySelectionForm
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
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.SecurityTreeView = new Enterprise.MasterFiles.GUI.ZSecurityTreeView();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 431, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 23, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);
			// 
			// SecurityTreeView
			// 
			this.SecurityTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SecurityTreeView.HideSelection = false;
			this.SecurityTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SecurityTreeView.Name = "SecurityTreeView";
			this.SecurityTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 396, true);
			this.SecurityTreeView.TabIndex = 0;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SecuritySelectionForm|77f45d06-0cea-44e8-ab74-7de70d807828", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 403, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.CloseButton.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SecuritySelectionForm|0db33c45-955a-451d-a59a-e635430aa24e", "OK");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 403, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.OKButton.TabIndex = 1;
			// 
			// SecuritySelectionForm
			// 
			this.AcceptButton = this.OKButton;

			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(328, 454, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SecuritySelectionForm|53a72691-e3f1-4676-9c74-ff314d9b9630", "Select a Security Right");
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.SecurityTreeView);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 230, true);
			this.Name = "SecuritySelectionForm";
			this.Text = "SecuritySelectionForm";
			this.Controls.SetChildIndex(this.SecurityTreeView, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		internal ZSecurityTreeView SecurityTreeView;
		internal ZButton OKButton;
		internal ZButton CloseButton;
	}
}
