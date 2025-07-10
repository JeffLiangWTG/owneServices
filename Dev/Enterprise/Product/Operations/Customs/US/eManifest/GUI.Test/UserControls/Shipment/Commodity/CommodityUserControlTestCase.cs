using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class CommodityUserControlTestCase : TestCaseWithFactory
	{
		public void TestC4CodesTabPageDoesNotDisappearAfterClick()
		{
			var trip = Factory.New<Trip>();
			var shipment = trip.Shipments.AddNew();
			shipment.B0_ShipmentType = ShipmentTypes.Codes.BRASS;
			using (var form = new ManifestForm(trip))
			{
				form.Show();
				var tabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(form, "TabControl");
				tabControl.SelectTab("ShipmentsTabPage");
				Application.DoEvents();
				var shipmentsTabPage = tabControl.SelectedTab;
				var shipmentTabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(shipmentsTabPage, "ShipmentTabControl");
				var commoditiesTabPage = (ZTabPage)ManifestUserControlTestCase.FindControl(shipmentTabControl, "CommoditiesTabPage");
				shipmentTabControl.SelectTab(commoditiesTabPage);
				Application.DoEvents();
				var commodityDetailsTabControl = (ZTabControl)ManifestUserControlTestCase.FindControl(commoditiesTabPage, "CommodityDetailsTabControl");
				var c4CodesTabPage = (ZTabPage)ManifestUserControlTestCase.FindControl(commodityDetailsTabControl, "C4CodesTabPage");
				Assert(c4CodesTabPage.TabVisible);
				commodityDetailsTabControl.SelectTab(c4CodesTabPage);
				Application.DoEvents();
				Assert(c4CodesTabPage.TabVisible);
			}
		}
	}
}
