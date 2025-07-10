using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.eTail.GUI.Testing
{
	public class HVLVManifestMenuItemTest : TestCaseWithFactory
	{
		public void TestHVLVShipmentsItemDetailsReport_StmMenuItemExists()
		{
			var stmMenuItem = Factory.LoadTop1<StmMenuItem>(new ZQuery(StmMenuItemSchema.SU_MenuName, "HVLV Shipments Item Details Report"));
			AssertNotNull("Expected an StmMenuItem SU_MenuName == 'HVLV Shipments Item Details Report'", stmMenuItem);
		}

		public void TestGivenHVLVManifestMenuItem_WhenActionCalled_ThenReportShipmentIDFilterPrefilled()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentWithAddress(Factory, out _, out _, out _);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var menuItems = topLevelMenu.MenuItems;
				var hvlvManifestMenuItem = (HVLVManifestMenuItem)(menuItems.OfType<ZMenuItem>().First(x => x.GetType() == typeof(HVLVManifestMenuItem)));

				hvlvManifestMenuItem.PrepareDocumentPackForInvokeAction();

				using (var report = hvlvManifestMenuItem.ReportForTesting)
				{
					var shipmentFilterValue = ((LookupField)report.FilterCollection["Shipment ID"]).Value;
					AssertEquals(shipment.PK.ToGuid(), shipmentFilterValue);
				}
			}
		}
	}
}
