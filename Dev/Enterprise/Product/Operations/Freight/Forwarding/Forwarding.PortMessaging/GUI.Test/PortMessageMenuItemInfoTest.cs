using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.PortMessaging.Business;

namespace Enterprise.Freight.Forwarding.PortMessaging.GUI.Testing
{
	sealed class PortMessageMenuItemInfoTest : TestCaseWithFactory
	{
		public void TestGetAllMenuItemInfos()
		{
			const string expectedMenuItems = @"Port Order with HDS (DEHAM):
- Send Port Order With HDS
- Cancellation Because of Errors
- Cancellation of Export on Exit
- Forwarding to Another Port
Port Order (DEHAM):
- Send Port Order for Inbound Delivery
- Send Port Order for Inbound Delivery Cancellation
- Send Port Order for Outbound Delivery
- Send Port Order for Outbound Delivery Cancellation
- Send Stop Request
- Send Gate Pass
- Send Gate Pass Cancellation
- Send Request for Port Services
- Send Request for Port Services Cancellation
- Send Certificate of Obligation
- Send Certificate of Obligation Cancellation
- Send Request for Rail Discharge
- Send Request for Rail Discharge Cancellation
";

			var manager = new ConsolPortMessagingManager(Factory.New<ForwardingConsol>());
			var list = PortMessageMenuItemInfo.GetAllMenuItemInfos(manager);
			AssertMenuList(expectedMenuItems, list);
		}

		void AssertMenuList(string expectedMenu, IEnumerable<PortMessageMenuItemInfo> list)
		{
			var actualMenu = "";
			foreach (var group in list.GroupBy(x => x.Category))
			{
				actualMenu += group.First().Category + ":\r\n";
				foreach (var item in group)
				{
					actualMenu += "- " + item.MenuItemName + "\r\n";
				}
			}

			AssertMultilineASCIIEquals("PortMessageMenuItemInfo.GetAllMenuItemInfos", expectedMenu, actualMenu);
		}
	}
}
