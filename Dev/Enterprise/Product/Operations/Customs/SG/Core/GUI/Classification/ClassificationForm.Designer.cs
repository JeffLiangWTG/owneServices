using Enterprise.Customs.SG.V4.Business;
using Enterprise.Environment;
using Enterprise.Security;

namespace Enterprise.Customs.SG.V4.GUI
{
	public partial class ClassificationForm : Customs.GUI.BaseClassificationForm
	{
		protected override void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 386, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(591, 359, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 24, true);
			// 
			// ClassificationForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(599, 442, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 495, true);
			this.Name = "ClassificationForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
