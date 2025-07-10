
namespace Enterprise.Customs.SG.V4.GUI
{
	partial class PromtCancellationForm
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
		new void InitializeComponent()
		{
			this.zDropEdit1 = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CancellationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DeclarationGroupBox.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.CancellationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(281, 336, true);
			this.OKButton.ReadOnly = false;
			this.OKButton.TabIndex = 2;
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(359, 336, true);
			this.Cancel_Button.TabIndex = 3;
			// 
			// DeclarationGroupBox
			// 
			this.DeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 192, true);
			// 
			// DeclarationCheckBox
			// 
			this.DeclarationCheckBox.Checked = true;
			this.DeclarationCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 181, true);
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.Controls.Add(this.CancellationGroupBox);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 154, true);
			this.DetailsTabPage.Controls.SetChildIndex(this.BrokerDetailsGroupBox, 0);
			this.DetailsTabPage.Controls.SetChildIndex(this.CancellationGroupBox, 0);
			// 
			// BrokerDetailsGroupBox
			// 
			this.BrokerDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 65, true);
			this.BrokerDetailsGroupBox.TabIndex = 1;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 371, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// zDropEdit1
			// 
			this.zDropEdit1.BindTo = "AM_CancellationCode";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_CancellationCodeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).AM_CancellationCode)));
			this.zDropEdit1.BindToList = "Lookups+CancellationCodeList";
			this.zDropEdit1.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("4F5D11F0-506C-4A04-9408-F8C43FE4776D", "Code:");
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Customs.SG.V4.Business.AdditionalMessageInformation)(null)).Lookups.CancellationCodeList)));
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 21, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.PreBoundMaxLength = 3;
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.zDropEdit1.TabIndex = 1;
			// 
			// CancellationGroupBox
			//
			this.CancellationGroupBox.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("EC9F26C4-1E7E-44C1-8C47-F018AF7C732A", "Cancellation");
			this.CancellationGroupBox.Controls.Add(this.zDropEdit1);
			this.CancellationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CancellationGroupBox.Name = "CancellationGroupBox";
			this.CancellationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 59, true);
			this.CancellationGroupBox.TabIndex = 0;
			this.CancellationGroupBox.TabStop = false;
			// 
			// PromtCancellationForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 395, true);
			this.Name = "PromtCancellationForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("9D871375-B3ED-4508-A97F-366FB0934019", "Send Cancellation");
			this.DeclarationGroupBox.ResumeLayout(false);
			this.DeclarationGroupBox.PerformLayout();
			this.MainTabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.CancellationGroupBox.ResumeLayout(false);
			this.CancellationGroupBox.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZDropEdit zDropEdit1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CancellationGroupBox;
	}
}
