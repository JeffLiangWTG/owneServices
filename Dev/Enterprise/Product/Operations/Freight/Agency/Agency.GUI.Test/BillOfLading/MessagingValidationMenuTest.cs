using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	class MessagingValidationMenuTest : TestCaseWithFactory
	{
		public void TestPopup()
		{
			var pa = MenuAssertion.AssertHasMenu(Menu, "Port Authority Messaging");
			var eido = MenuAssertion.AssertHasMenu(Menu, "EIDO Messaging");
			var dgm = MenuAssertion.AssertHasMenu(Menu, "Dangerous Goods Manifest Messaging");
			PortAuthorityBusinessObjectValidation.UnregisterForFactory(Factory);
			EIDOBusinessObjectValidation.UnregisterForFactory(Factory);
			DangerousGoodsManifestMessageValidationStrategy.UnregisterForFactory(Factory);
			Menu.OnPopup(EventArgs.Empty);
			AssertEquals("Port Authority 1", false, pa.Checked);
			AssertEquals("EIDO 1", false, eido.Checked);
			AssertEquals("Dangerous Goods Manifest 1", false, dgm.Checked);
			PortAuthorityBusinessObjectValidation.RegisterForFactory(Factory);
			Menu.OnPopup(EventArgs.Empty);
			AssertEquals("Port Authority 2", true, pa.Checked);
			AssertEquals("EIDO 2", false, eido.Checked);
			AssertEquals("Dangerous Goods Manifest 2", false, dgm.Checked);
			EIDOBusinessObjectValidation.RegisterForFactory(Factory);
			Menu.OnPopup(EventArgs.Empty);
			AssertEquals("Port Authority 3", true, pa.Checked);
			AssertEquals("EIDO 3", true, eido.Checked);
			AssertEquals("Dangerous Goods Manifest 3", false, dgm.Checked);
			DangerousGoodsManifestMessageValidationStrategy.RegisterForFactory(Factory);
			Menu.OnPopup(EventArgs.Empty);
			AssertEquals("Port Authority 4", true, pa.Checked);
			AssertEquals("EIDO 4", true, eido.Checked);
			AssertEquals("Dangerous Goods Manifest 4", true, dgm.Checked);
		}

		public void TestTogglePortAuthority()
		{
			var pa = MenuAssertion.AssertHasMenu(Menu, "Port Authority Messaging");
			PortAuthorityBusinessObjectValidation.UnregisterForFactory(Factory);
			Menu.OnPopup(EventArgs.Empty);
			AssertEquals(false, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));
			Menu.OnPopup(EventArgs.Empty);
			pa.PerformClick();
			AssertEquals(true, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));
			Menu.OnPopup(EventArgs.Empty);
			pa.PerformClick();
			AssertEquals(false, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));
		}

		public void TestToggleDangerousGoodsManifest()
		{
			var dgm = MenuAssertion.AssertHasMenu(Menu, "Dangerous Goods Manifest Messaging");
			DangerousGoodsManifestMessageValidationStrategy.UnregisterForFactory(Factory);
			Menu.OnPopup(EventArgs.Empty);
			AssertEquals(false, DangerousGoodsManifestMessageValidationStrategy.IsRegisteredInFactory(Factory));
			Menu.OnPopup(EventArgs.Empty);
			dgm.PerformClick();
			AssertEquals(true, DangerousGoodsManifestMessageValidationStrategy.IsRegisteredInFactory(Factory));
			Menu.OnPopup(EventArgs.Empty);
			dgm.PerformClick();
			AssertEquals(false, DangerousGoodsManifestMessageValidationStrategy.IsRegisteredInFactory(Factory));
		}

		public void TestToggleEIDO()
		{
			var pa = MenuAssertion.AssertHasMenu(Menu, "EIDO Messaging");
			EIDOBusinessObjectValidation.UnregisterForFactory(Factory);
			Menu.OnPopup(EventArgs.Empty);
			AssertEquals(false, EIDOBusinessObjectValidation.IsRegisteredInFactory(Factory));
			Menu.OnPopup(EventArgs.Empty);
			pa.PerformClick();
			AssertEquals(true, EIDOBusinessObjectValidation.IsRegisteredInFactory(Factory));
			Menu.OnPopup(EventArgs.Empty);
			pa.PerformClick();
			AssertEquals(false, EIDOBusinessObjectValidation.IsRegisteredInFactory(Factory));
		}

		public void TestConstructor_ImportReleaseOrderPortsRegistryEnabled_AddReleaseOrderMessagingMenu()
		{
			var countryCode = "NZ";
			var homePort = "NZAKL";
			var portMessagingPortCollection = new PortMessagingPortCollection();
			portMessagingPortCollection.Add(new PortMessagingPort() { Port = homePort, Enabled = true, SenderID = "1" });

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			var branch = company.Branches.AddNew();
			branch.GB_Code = countryCode;
			branch.GB_RL_NKHomePort = homePort;
			Factory.Save();

			using (branch.SetAsTemporaryContext())
			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portMessagingPortCollection))
			{
				var menuItem = Menu.MenuItems.FindByText("Release Order Messaging");
				AssertNotNull("Release Order Messaging menu item", menuItem);
			}
		}

		public void TestConstructor_ImportReleaseOrderPortsRegistryDisabled_DoNotAddReleaseOrderMessagingMenu()
		{
			var portMessagingPortCollection = new PortMessagingPortCollection();
			portMessagingPortCollection.Add(new PortMessagingPort() { Port = "NZAKL", Enabled = false, SenderID = "1" });

			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portMessagingPortCollection))
			{
				var menuItem = Menu.MenuItems.FindByText("Release Order Messaging");
				AssertNull("Release Order Messaging menu item", menuItem);
			}
		}

		public void TestReleaseOrderMessagingMenuClick_RegisterReleaseOrderValidation()
		{
			var countryCode = "NZ";
			var homePort = "NZAKL";
			var portMessagingPortCollection = new PortMessagingPortCollection();
			portMessagingPortCollection.Add(new PortMessagingPort() { Port = homePort, Enabled = true, SenderID = "1" });

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;
			var branch = company.Branches.AddNew();
			branch.GB_Code = countryCode;
			branch.GB_RL_NKHomePort = homePort;
			Factory.Save();

			using (branch.SetAsTemporaryContext())
			using (AgencyRegistry.Instance.ImportReleaseOrderPorts.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, portMessagingPortCollection))
			{
				var menuItem = Menu.MenuItems.FindByText("Release Order Messaging");

				NZReleaseOrderMessageValidationStrategy.UnregisterForFactory(Factory);

				Menu.OnPopup(EventArgs.Empty);
				menuItem.PerformClick();
				AssertEquals(true, NZReleaseOrderMessageValidationStrategy.IsRegisteredInFactory(Factory));

				Menu.OnPopup(EventArgs.Empty);
				menuItem.PerformClick();
				AssertEquals(false, NZReleaseOrderMessageValidationStrategy.IsRegisteredInFactory(Factory));
			}
		}

		public void TestMessagingValidationMenuCustomise()
		{
			var portAuthorityMenunName = "Port Authority Messaging";
			var pa = MenuAssertion.AssertHasMenu(Menu, portAuthorityMenunName);
			var strategy = MessagingValidationStrategyFactory.GetStrategies()[0];
			strategy.IsEnabled = false;
			Menu.OnPopup(EventArgs.Empty);
			pa.PerformClick();
			AssertEquals(true, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));
			AssertEquals(true, strategy.IsEnabled);
			Menu.OnPopup(EventArgs.Empty);
			pa.PerformClick();
			AssertEquals(false, PortAuthorityBusinessObjectValidation.IsRegisteredInFactory(Factory));
			AssertEquals(false, strategy.IsEnabled);
		}

		#region Implementation
		DummyBusinessObject Dummy
		{
			get
			{
				return dummy ?? (dummy = Factory.New<DummyBusinessObject>());
			}
		}

		DummyBusinessObject dummy;
		ZForm Form
		{
			get
			{
				return form ?? (form = new ZForm(Dummy));
			}
		}

		ZForm form;
		MessagingValidationMenu Menu
		{
			get
			{
				return menu ?? (menu = new MessagingValidationMenu(Form));
			}
		}

		MessagingValidationMenu menu;
		protected override void TearDown()
		{
			base.TearDown();
			if (menu != null)
			{
				menu.Dispose();
				menu = null;
			}

			if (form != null)
			{
				form.Dispose();
				form = null;
			}
		}
		#endregion
	}
}
