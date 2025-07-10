using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.GUI
{
	public partial class JobDeclarationForm
	{
		private new void InitializeComponent()
		{
			this.BottomButtonPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// BottomButtonPanel
			// 
			this.BottomButtonPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 660, true);
			this.BottomButtonPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 28, true);
			// 
			// oPostingButtonsUserControl
			// 
			this.oPostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(768, 3, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 668, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 24, true);
			// 
			// JobDeclarationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1310, 710, true);
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.JobDeclaration";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1310, 740, true);
			this.Name = "JobDeclarationForm";
			this.Text = "JobDeclarationForm";
			this.BottomButtonPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
