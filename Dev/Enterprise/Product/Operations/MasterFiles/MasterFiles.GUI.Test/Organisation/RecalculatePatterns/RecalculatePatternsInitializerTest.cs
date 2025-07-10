using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RecalculatePatternsInitializerTest : TestCaseWithFactory
	{
		#region organisation specific recalculator test

		public void TestCreateRecalculateMenuForOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			org.OH_FullName = "ABC Transports";

			AssertCreateRecalculateMenuResultForOrg(org, false);

			Factory.Save();

			AssertCreateRecalculateMenuResultForOrg(org, true);

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.DHL))
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertCreateRecalculateMenuResultForOrg(org, false);
			}
		}

		void AssertCreateRecalculateMenuResultForOrg(OrgHeader org, bool menuEnabled)
		{
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				ObjectFactory.Get<IMasterDataProviderGUI>().CreateOrgMenu(dummyForm, org);
				var recalculator = new RecalculatePatternsInitializer(org);
				recalculator.CreateRecalculateMenuItem(((IFileMenuItemsProvider)dummyForm).ActionsMenuItem, delegate
				{ org.RegeneratePatternTables(); });
				dummyForm.Show();
				var actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var item = actionsMenuItem.MenuItems.FindByText("Recalculate Pattern Tables");
				AssertNotNull("Check Recalculate Pattern Tables menu item exists", item);
				AssertEquals("Recalculate menu enabled", menuEnabled, item.Enabled);
				var index = actionsMenuItem.MenuItems.IndexOf(item);
				if (index > 0 && index <= actionsMenuItem.MenuItems.Count - 1)
				{
					var fdpitem = actionsMenuItem.MenuItems[index - 1];
					AssertNotNull("Check Deduplication menu item exists", fdpitem);
					Assert("Check Deduplication menu item is above", fdpitem.Text.Equals("Find &Duplicates"));
				}
			}
		}

		#endregion

		#region persons specific recalculator test

		public void TestCreateRecalculateMenuForPerson()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = "Fake Name";

			AssertCreateRecalculateMenuResultUsingPerson(person, false);

			Factory.Save();

			AssertCreateRecalculateMenuResultUsingPerson(person, true);

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.DHL))
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertCreateRecalculateMenuResultUsingPerson(person, false);
			}
		}

		void AssertCreateRecalculateMenuResultUsingPerson(GlbPerson person, bool menuEnabled)
		{
			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				var recalculator = new RecalculatePatternsInitializer(person);
				recalculator.CreateRecalculateMenuItem(((IFileMenuItemsProvider)dummyForm).ActionsMenuItem, delegate
				{ person.RegeneratePatternTables(); });
				dummyForm.Show();
				var actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var item = actionsMenuItem.MenuItems.FindByText("Recalculate Pattern Tables");
				AssertNotNull("Check Recalculate Pattern Tables menu item exists", item);
				AssertEquals("Recalculate menu enabled", menuEnabled, item.Enabled);
			}
		}

		#endregion

		#region generic tests

		public void TestMenuItemsOnClick_Disabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "EDIORGFAKE";
			org.OH_FullName = "CargoWise ediProd";
			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var initializer = new RecalculatePatternsInitializer_ForTest(org);
				initializer.CreateRecalculateMenuItem(((IFileMenuItemsProvider)dummyForm).ActionsMenuItem, null);
				dummyForm.Show();

				org.Contacts.AddNew().OC_ContactName = "x";
				MenuItem actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				MenuItem item = actionsMenuItem.MenuItems.FindByText("Recalculate Pattern Tables");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				item.PerformClick();

				AssertEquals("Please save the form before running this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemsOnClick_Enabled()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "EDIORGFAKE";
			org.OH_FullName = "CargoWise ediProd";
			Factory.Save();

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var initializer = new RecalculatePatternsInitializer_ForTest(org);
				initializer.CreateRecalculateMenuItem(((IFileMenuItemsProvider)dummyForm).ActionsMenuItem, null);
				dummyForm.Show();

				MenuItem actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				MenuItem item = actionsMenuItem.MenuItems.FindByText("Recalculate Pattern Tables");

				item.PerformClick();

				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllUsersOfEdiProdHaveRegenRunnerMenu()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "EDIORGFAKE";
			org.OH_FullName = "CargoWise ediProd";
			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var initializer = new RecalculatePatternsInitializer(org);
				initializer.CreateRecalculateMenuItem(((IFileMenuItemsProvider)dummyForm).ActionsMenuItem, null);
				dummyForm.Show();
				MenuItem actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				MenuItem item = actionsMenuItem.MenuItems.FindByText("Recalculate Pattern Tables");
				AssertNotNull("Check Recalculate Pattern Tables menu item exists", item);
				Assert("Recalculate menu item is visible for ediProd users", item.Visible);
			}

			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.DHL))
			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var initializer = new RecalculatePatternsInitializer(org);
				initializer.CreateRecalculateMenuItem(((IFileMenuItemsProvider)dummyForm).ActionsMenuItem, null);
				dummyForm.Show();
				MenuItem actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				MenuItem item = actionsMenuItem.MenuItems.FindByText("Recalculate Pattern Tables");
				AssertNotNull("Check Recalculate Pattern Tables menu item exists", item);
				Assert("Recalculate menu item is invisible for other clients", !item.Visible);
			}
		}

		public void TestMenuVisibleToSupport()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABCDE";
			org.OH_FullName = "ABC Transports";
			var currentUserName = Env.CurrentUser.LoginName;

			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				using (Env.SetTemporaryUserContext(User.ServiceUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					new RecalculatePatternsInitializer(org).CreateRecalculateMenuItem(((IFileMenuItemsProvider)dummyForm).ActionsMenuItem, null);
					dummyForm.Show();
					MenuItem actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
					MenuItem item = actionsMenuItem.MenuItems.FindByText("Recalculate Pattern Tables");
					AssertNotNull("Check Recalculate Pattern Tables menu item exists", item);
					Assert("Recalculate menu item is not visible ", !item.Visible);
				}
			}

			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				using (Env.SetTemporaryUserContext(User.SupportUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
				{
					new RecalculatePatternsInitializer(org).CreateRecalculateMenuItem(((IFileMenuItemsProvider)dummyForm).ActionsMenuItem, null);
					dummyForm.Show();
					MenuItem actionsMenuItem = dummyForm.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
					MenuItem item = actionsMenuItem.MenuItems.FindByText("Recalculate Pattern Tables");
					AssertNotNull("Check Recalculate Pattern Tables menu item exists", item);
					Assert("Recalculate menu item is visible", item.Visible);
				}
			}
			using (Env.SetTemporaryUserContext(currentUserName, GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateMenuOnNullParentFormArgument()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			new RecalculatePatternsInitializer(org).CreateRecalculateMenuItem(null, null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateMenuOnNullOrgHeaderArgument()
		{
			using (ZForm dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				new RecalculatePatternsInitializer(null);
			}
		}

		#endregion

		#region Implementation

		class RecalculatePatternsInitializer_ForTest : RecalculatePatternsInitializer
		{
			public RecalculatePatternsInitializer_ForTest(IDeduplicatable sourceBizo) : base(sourceBizo)
			{
			}

			public List<ZMenuItem> MenuItems_ExposedForTest => menuItems;
		}
		#endregion
	}
}
