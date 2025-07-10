using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.MarketingManager.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.GUI.Testing
{
	class TradeLaneWithDetailsControlTest : TestCaseWithFactory
	{
		#region Properties

		#region Collapsed

		public void TestCollapsed()
		{
			using (var control = new SalesCollectionWithTradeDetailsControlForTest())
			{
				control.Collapsed = true;
				AssertEquals(true, control.MainSplitContainer_Exposed.Panel2Collapsed);

				control.Collapsed = false;
				AssertEquals(false, control.MainSplitContainer_Exposed.Panel2Collapsed);
			}
		}

		#endregion

		#endregion

		#region Inner Controls

		public void TestInnerControlsAdded()
		{
			var shipmentsProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, shipmentsProduct);

			using (var form = new ZForm(salesHeader))
			using (var control = new TradeLaneWithDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertNotNull(control.TradeLanesControl);
				AssertNotNull(control.TradeDetailsControl);
			}
		}

		public void TestTradeDetailsHeightAdjustment()
		{
			var shipmentsProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, shipmentsProduct);

			using (var form = new ZForm(salesHeader))
			using (var control = new TradeLaneWithDetailsControl())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(100), control.TradeDetailsHeightAdjustment);
			}
		}

		public void TestMainSplitContainerPanelLayout()
		{
			var shipmentsProduct = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, SystemDefinedSalesProductList.Codes.ForwardingShipment);
			var org = Factory.New<OrgHeader>();
			var salesHeader = new SalesHeader(org, shipmentsProduct);

			using (var form = new ZForm(salesHeader))
			using (var control = new SalesCollectionWithTradeDetailsControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				AssertEquals(FixedPanel.None, control.MainSplitContainer_Exposed.FixedPanel);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(0), control.MainSplitContainer_Exposed.Panel2MinSize);
			}
		}

		#endregion

		#region Implementation

		class SalesCollectionWithTradeDetailsControlForTest : TradeLaneWithDetailsControl
		{
			public SalesCollectionWithTradeDetailsControlForTest()
			{
			}

			public KSplitContainer MainSplitContainer_Exposed => mainSplitContainer;
		}

		#endregion
	}
}
