using CargoWise.EntityFramework;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.GUI;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.GUI.Orders.DataTransfer
{
	public partial class CsvOrderDataImporterForm : DataImporterForm
	{
		new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 527, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 26, true);
			// 
			// CsvOrderDataImporterForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(680, 553, true);
			this.Name = "CsvOrderDataImporterForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
