using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class Phase5GoodsItemsTabUserControlTest : TestCaseWithFactory
	{
		public void TestGoodsItemAdditionalDocumentsTabPage()
		{
			var goodsItemsExportToOpenTabUserControl = userControl.GoodsItemExportToOpenTabUserControl;
			var exportToOpenTabPage = userControl.GoodsItemExportToOpenTabPage;
			CombineAssertions(() =>
			{
				AssertEquals("GoodsItemExportToOpenTabPage.Caption", "Export To Open", exportToOpenTabPage.CaptionResourceString.Caption);

				AssertEquals("GoodsItemExportToOpenTabPageUserControl.Dock", DockStyle.Fill, goodsItemsExportToOpenTabUserControl.Dock);
				AssertEquals("GoodsItemExportToOpenTabPageUserControl.Name", "GoodsItemExportToOpenTabUserControl", goodsItemsExportToOpenTabUserControl.Name);
				AssertEquals("GoodsItemExportToOpenTabPageUserControl.BindingMember", ".", goodsItemsExportToOpenTabUserControl.GetBindingMember());
			});
		}
		protected override void SetUp()
		{
			base.SetUp();
			userControl = new Phase5GoodsItemsTabUserControl();
		}
		Phase5GoodsItemsTabUserControl userControl;

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}
	}
}
