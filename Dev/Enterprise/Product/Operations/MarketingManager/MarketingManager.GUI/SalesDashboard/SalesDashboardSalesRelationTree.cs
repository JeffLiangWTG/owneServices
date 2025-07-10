using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MarketingManager.GUI
{
	class SalesDashboardSalesRelationTree : SalesRelationTree
	{
		protected override void OnDragDrop(DragEventArgs e)
		{
			if (Globals.Message.Show(ResString.GetMultilingualString("BA53EE64-40B1-4370-ABAD-4B424765468C", "Are you sure you want to move the selected Sales Activity? This action will be saved immediately."),
				ResString.GetMultilingualString("A8E1AF44-485B-4B77-8207-C02ADC42AFD2", "Move"), MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
			{
				base.OnDragDrop(e);
			}
		}
	}
}
