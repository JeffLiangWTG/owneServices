using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI.Testing
{
	public class CalculateLMCDepotDetailsMenuItemTest : TestCaseWithFactory
	{
		public void TestCalculateLMCDepotDetailsActionMenu()
		{
			var shipment = HVLVMenuItemTestHelper.CreateShipmentWithAddress(Factory, out var depotAddress, out var carrier, out var agent);
			Factory.Save();

			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
				var topLevelMenu = plugin.TopLevelMenu;
				topLevelMenu.PerformSelect();

				var menuItem = topLevelMenu.MenuItems.OfType<CalculateLMCDepotDetailsMenuItem>().Single();
				AssertNotNull(menuItem);

				foreach (HVLVConsignment consignment in shipment.HVLVConsignments)
				{
					AssertEquals(ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
					AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
					AssertEquals(ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
				}

				menuItem.PerformClick();

				foreach (HVLVConsignment consignment in shipment.HVLVConsignments)
				{
					AssertEquals(depotAddress.PK, consignment.HVC_OA_DestinationDepot);
					AssertEquals(carrier.PK, consignment.HVC_OH_LastMileCarrier);
					AssertEquals("EXP", consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals(agent.PK, consignment.HVC_OH_LastMileCarrierBookingAgent);
				}

				PortHubSelectionTestDataCreator.ClearUpAllZoneItems(Factory);
				menuItem.PerformClick();

				foreach (HVLVConsignment consignment in shipment.HVLVConsignments)
				{
					AssertEquals(ZGuid.Empty, consignment.HVC_OA_DestinationDepot);
					AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrier);
					AssertEquals(ZString.Empty, consignment.HVC_PL_NKLastMileCarrierServiceLevel);
					AssertEquals(ZGuid.Empty, consignment.HVC_OH_LastMileCarrierBookingAgent);
				}
			}
		}
	}
}
