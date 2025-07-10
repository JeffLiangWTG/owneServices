using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.ComplianceRisk.GUI.Test;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DeniedPartyScreeningActionsProviderTest : TestCaseWithFactory
	{
		public void TestAddSeparatorFormMenutItemIfNeeded()
		{
			var dummyBizO = Factory.New<DummyBusinessObject>();

			using (var form = new ZForm(dummyBizO))
			{
				ZFormMenuStrategy.AddActionsMenuItem(form, new ZMenuItem(ZMenuItem.Separator));

				var actions = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var count = actions.MenuItems.Count;
				var provider = new DeniedPartyScreeningActionsProvider(form);
				provider.AddinSeparatorActionsMenuItemIfNeeded();

				Assert("Should not add Separator", count == actions.MenuItems.Count);

				ZFormMenuStrategy.AddActionsMenuItem(form, new ZMenuItem("WiseTech Global"));

				count = actions.MenuItems.Count;

				provider.AddinSeparatorActionsMenuItemIfNeeded();

				Assert("Should add Separator", actions.MenuItems.Count > count);
			}
		}

		public void TestAddSeparatorModuleMenuItemIfNeeded()
		{
			var menuItem = new List<MenuItem>()
			{
				new ZMenuItem(ZMenuItem.Separator)
			};
			using (var moduleFilterGrid = new DummyFilterGridModule())
			{
				var provider = new DeniedPartyScreeningActionsProvider(moduleFilterGrid, menuItem.ToList());
				provider.AddinSeparatorActionsMenuItemIfNeeded();

				Assert("Should not add Separator", menuItem.Count == 1);

				menuItem.Add(new ZMenuItem("WiseTech Global"));

				Assert("Should  add Separator", menuItem.Count > 1);
			}
		}

		public void TestIsFormActionsMenuItem()
		{
			using (var form = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				CombineAssertions(() =>
				{
					var provider = new DeniedPartyScreeningActionsProvider(form);
					AssertEquals(typeof(DeniedPartyScreeningActionsProvider), provider.GetType());
					AssertEquals("Should be valid type Form", true, provider.IsFormActionsMenuItem);
					AssertExceptionThrown<ArgumentNullException>(() => new DeniedPartyScreeningActionsProvider(null));
				});
			}
		}

		public void TestIsModuleActionsMenuItem()
		{
			using (var module = new DummyFilterGridModule())
			{
				CombineAssertions(() =>
				{
					var menuItem = new List<MenuItem>();

					AssertExceptionThrown<ArgumentNullException>(() => new DeniedPartyScreeningActionsProvider(module, null));
					AssertExceptionThrown<ArgumentNullException>(() => new DeniedPartyScreeningActionsProvider(null, menuItem.ToList()));

					var provider = new DeniedPartyScreeningActionsProvider(module, menuItem.ToList());
					AssertEquals("Should be valid type Filter Grid and Menu Item", true, provider.IsModuleActionsMenuItem);
				});
			}
		}

		public void TestModuleHasSelectedBusinessObjectsWithShowMessage()
		{
			using (var moduleFilterGrid1 = new ComplianceRiskHelperTest.BaseFilterGridModuleForTest(typeof(DeniedPartyScreeningFilterModuleStrategyTest.DummyWithBothScreeningPartyProviderAndComplianceRiskStatusProvider)))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(true)))
			using (Env.Registry.RawRegistry.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var menuItems = new List<MenuItem>();
				new DeniedPartyScreeningActionsProvider(moduleFilterGrid1, menuItems).AddJobsMenuItem();
				menuItems.FindByText("Resynchronize Compliance Risk Status").PerformClick();

				AssertEquals("Please select at least 1 row to Process.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				menuItems.FindByText("View Party Risk").PerformClick();

				AssertEquals("Please select at least 1 row to Process.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}
	}
}
