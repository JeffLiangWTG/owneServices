using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GUI
{
    partial class ApportionmentProgressForm
    {
		private new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ApportionmentProgressForm));
			this.MainPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 85, true);
			// 
			// TopPanel
			// 
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 52, true);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 33, true);
			// 
			// EnterpriseLogo
			// 
			this.EnterpriseLogo.Image = ((System.Drawing.Image)(resources.GetObject("EnterpriseLogo.Image")));
			this.EnterpriseLogo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 6, true);
			// 
			// CancelProgressButton
			// 
			this.CancelProgressButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 14, true);
			this.CancelProgressButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			// 
			// ProgressBar
			// 
			this.ProgressBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 14, true);
			this.ProgressBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 15, true);
			// 
			// ProgressLabel
			// 
			this.ProgressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(60, 6, true);
			this.ProgressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(248, 29, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 77, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 8, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(161);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(161);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(BaseJobDeclaration);
			// 
			// ApportionmentProgressForm
			// 

			this.CancelProgressButtonText = Res.GetString("2D33FEC4-6914-4350-B183-1B1DE03BB60C", "Cancel");
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ApportionmentProgressForm|afca0aa9-4327-494b-a6ed-83612ee94237", "Apportioning Charges");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 85, true);
			this.CaptionRenderingEnabled = true;
			this.DataSourceAssemblyName = "Enterprise.Customs.Business";
			this.DataSourceType = typeof(BaseJobDeclaration);
			this.DataSourceTypeName = "Enterprise.Customs.Business.BaseJobDeclaration";
			this.Name = "ApportionmentProgressForm";
			this.Status = "This process can take some time...";
			this.MainPanel.ResumeLayout(false);
			this.TopPanel.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.EnterpriseLogo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
