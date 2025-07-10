using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI.Testing
{
	abstract class BaseNeedAddApplicationLockHVLVMenuItemTest : TestCaseWithFactory
	{
		public void TestAddApplicatonLockForShipmentWhenClickMenuItem()
		{
			var shipment = PrepareShipment();
			var consignmentHeader = HVLVConsignmentHeader.GetOrCreate(shipment);
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_HCH_Header = consignmentHeader.PK;

			var item = consignment.Items.AddNew();
			if (shipment.Consols.Count > 0 && shipment.Consols[0].Containers.Count > 0)
			{
				item.HVI_ContainerNumber = shipment.Consols[0].Containers[0].ContainerNumberForBinding;
			}

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(TestingCountry))
			using (var form = new ZForm(shipment))
			{
				form.PlugIns.Add(ControllerIDs.ETailShipment);
				form.Show();

				var menuItem = GetMenuItemForAppLockKeyTest(form);

				var connection = Db.NewExtraConnectionToMainDb();
				Assert(connection.TryGetLock((ExpectedAppLockKey + "," + shipment.PK.ToString()).ToUpperInvariant(), out var appLock));

				using (appLock)
				{
					menuItem.PerformClick();

					AssertEquals("Shipment has been locked",
						"Failed to acquire lock for rows in table JobShipment, data being processed by other user.",
						UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		protected virtual ZMenuItem GetMenuItemForAppLockKeyTest(ZForm form)
		{
			AssertNotNullOrEmpty("Pre-req: Related job name is provided", RelatedJobName);

			var plugin = form.PlugIns.GetPlugIn(ControllerIDs.ETailShipment);
			var menuHVLV = plugin.TopLevelMenu;
			menuHVLV.PerformSelect();

			var customsMenu = menuHVLV.MenuItems.OfType<HVLVCustomsMenuGroup>().Single();
			var commandJobTypeMenuGroup = customsMenu.MenuItems.OfType<ZMenuItem>().Single((x => x.Caption == RelatedJobName));

			return commandJobTypeMenuGroup.MenuItems.OfType<ZMenuItem>().Single(menu => menu.Caption == $"Create {RelatedJobName}");
		}

		protected abstract ForwardingShipment PrepareShipment();

		protected abstract string TestingCountry { get; }
		protected virtual string RelatedJobName => null;
		protected abstract string ExpectedAppLockKey { get; }
	}
}
