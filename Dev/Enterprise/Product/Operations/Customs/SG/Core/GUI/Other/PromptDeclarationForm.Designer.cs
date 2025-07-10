
namespace Enterprise.Customs.SG.V4.GUI
{
	partial class PromptDeclarationForm
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
            this.DeclarationGroupBox.SuspendLayout();
            this.MainTabControl.SuspendLayout();
            this.DetailsTabPage.SuspendLayout();
            this.BrokerDetailsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // OKButton
            // 
            this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(282, 279, true);
            this.OKButton.ReadOnly = false;
            // 
            // Cancel_Button
            // 
            this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 279, true);
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.SetChildIndex(this.DetailsTabPage, 0);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 309, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 24, true);
            // 
            // PromptDeclarationForm
            // 
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 333, true);
            this.Name = "PromptDeclarationForm";
            this.CaptionResourceString = Enterprise.Customs.SG.V4.GUI.Res.GetData("1AD8FFD6-765B-419D-A42E-A4FDF769828D", "Send Declaration");
            this.DeclarationGroupBox.ResumeLayout(false);
            this.DeclarationGroupBox.PerformLayout();
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.DetailsTabPage.ResumeLayout(false);
            this.DetailsTabPage.PerformLayout();
            this.BrokerDetailsGroupBox.ResumeLayout(false);
            this.BrokerDetailsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.SupportingDocumentsTabPage.ResumeLayout(false);
            this.SupportingDocumentsTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
		}

		#endregion
	}
}
