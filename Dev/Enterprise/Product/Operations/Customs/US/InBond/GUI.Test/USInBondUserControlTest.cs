using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.InBond.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.InBond.GUI.Testing
{
	sealed class USInBondUserControlTest : TestCaseWithFactory
	{
		public void TestHiddenTabShouldNotGetNotifications()
		{
			var header = Factory.New<CusInBondHeader>();
			using (var userControl = new USInBondUserControl(header))
			using (var form = new ZForm(header))
			{
				form.Controls.Add(userControl);
				form.Show();
				userControl.MainTabControl.SelectTab(userControl.BillsTabPage);
				var billGrid = userControl.BillsTabPage.Controls.Find("BillsGrid", true)[0];
				header.BH_ImportTransportMode = "40";
				userControl.MainTabControl.SelectTab(userControl.AirBillsAndMovementsDetailsTabPage);
				NotificationBroadcaster.Instance.BroadcastVisibilityChange(billGrid);
				Assert(!userControl.BillsTabPage.TabVisible);
				NotificationBroadcaster.Instance.BroadcastNotificationsChange(CargoWise.EntityFramework.NotificationType.MessageError, billGrid);
				Assert(!userControl.BillsTabPage.TabVisible);
			}
		}

		public void TestControlsVisibility()
		{
			CusInBondHeader header = Factory.New<CusInBondHeader>();
			using (var userControl = new USInBondUserControl(header))
			using (var form = new ZForm(header))
			{
				form.Controls.Add(userControl);
				form.Show();
				// AIR
				header.BH_ImportTransportMode = "40";
				AssertEquals("BillsTabPage is hidden", 0, form.Controls.Find("BillsTabPage", true).Length);
				AssertEquals("MovementDetailsTabPage is hidden", 0, form.Controls.Find("MovementTabPage", true).Length);
				AssertEquals("AirBillsAndMovementsDetailsTabPage is shown", 1, form.Controls.Find("AirBillsAndMovementsDetailsTabPage", true).Length);
				// NONAIR
				header.BH_ImportTransportMode = "10";
				AssertEquals("BillsTabPage is shown", 1, form.Controls.Find("BillsTabPage", true).Length);
				AssertEquals("MovementDetailsTabPage is shown", 1, form.Controls.Find("MovementTabPage", true).Length);
				AssertEquals("AirBillsAndMovementsDetailsTabPage is hidden", 0, form.Controls.Find("AirBillsAndMovementsDetailsTabPage", true).Length);
			}
		}

		public void TestOverrideValuesCheckBoxVisibility()
		{
			var consol = Factory.New<ForwardingConsol>();
			var header = Factory.New<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = consol.TablePrefix;
			using (var userControl = new USInBondUserControl(header))
			using (var form = new ZForm(header))
			{
				form.Controls.Add(userControl);
				form.Show();
				var checkBox = userControl.Controls.Find("OverrideValuesCheckBox", true).Cast<ZCheckBox>().First();
				Assert("OverrideValuesCheckBox should not be shown", !checkBox.Visible);
			}
			var shipment = Factory.New<ForwardingShipment>();
			header.BH_ParentID = shipment.PK;
			header.BH_ParentTableCode = shipment.TablePrefix;
			using (var userControl = new USInBondUserControl(header))
			using (var form = new ZForm(header))
			{
				form.Controls.Add(userControl);
				form.Show();
				var checkBox = userControl.Controls.Find("OverrideValuesCheckBox", true).Cast<ZCheckBox>().First();
				Assert("OverrideValuesCheckBox should be shown", checkBox.Visible);
			}
		}
	}
}
