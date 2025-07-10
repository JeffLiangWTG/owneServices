using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbPersonModule))]
	internal class GlbPersonModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbPerson;
		}

		#region Implementation

		[RequiresSTA]
		public void TestClickMergeWithNoSelectedPersons_ShouldDisplayNotification()
		{
			using (var module = new GlbPersonModule())
			{
				var actionsMenu = module.FormActionMenu.FindByText("Actions");
				var mergeMenu = actionsMenu.MenuItems.FindByText("Merge");

				using (ZForm form = new ZForm())
				{
					form.Controls.Add(module.EmbeddedControl);
					form.Show();

					UnitTestUserNotification.Instance.ClearMessages();
					mergeMenu.PerformClick();
					AssertEquals("Please select at least 2 or more Persons.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		[RequiresSTA]
		public void TestMergeMenuItem_Security()
		{
			Factory.Save();

			Env.Security.PersonIntelligenceEdit.IsAllowed = false;

			using (var module = (GlbPersonModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var embeddedControl = module.EmbeddedControl;
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Merge");

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals(Environment.Env.Security.PersonIntelligenceEdit.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();

			Env.Security.PersonIntelligenceEdit.IsAllowed = true;
			using (var module = (GlbPersonModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var embeddedControl = module.EmbeddedControl;
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Merge");

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals("Please select at least 2 or more Persons.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestBulkSyncMenuItem_Security()
		{
			Factory.Save();

			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = false;

			using (var module = (GlbPersonModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var embeddedControl = module.EmbeddedControl;
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Bulk Create Accreditation Attempts");

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals(Environment.Env.Security.GlbAccreditationAttemptEdit.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}

			UnitTestUserNotification.Instance.ClearMessages();

			Env.Security.GlbAccreditationAttemptEdit.IsAllowed = true;
			using (var module = (GlbPersonModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var embeddedControl = module.EmbeddedControl;
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Bulk Create Accreditation Attempts");

				AssertNotNull(menuItem);
				menuItem.PerformClick();

				AssertEquals("Please select a Person.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestBulkSyncMenuItem()
		{
			using (var module = (GlbPersonModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var x = module.EmbeddedControl; // sets up menu
				var menuItem = module.ActionsMenuItem.MenuItems.FindByText("Bulk Create Accreditation Attempts");
				AssertNotNull(menuItem);
			}
		}

		protected void TestSupportsWorkflow()
		{
			using (var module = (GlbPersonModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals("Should support Worflow", true, module.SupportsWorkflow);
			}
		}

		public void TestBusinessContexts()
		{
			using (var module = (GlbPersonModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("GlbPerson business context should be returned", BusinessContext.GlbPerson, module.BusinessContexts[0]);
			}
		}

		protected override BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			var glbPerson = factory.NewWithValidTestData<GlbPerson>();
			glbPerson.PER_FullName = filterStripHelperTestPersonName;

			return glbPerson;
		}

		protected override void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
			base.CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);

			var filter = (ModuleTextFilter)filterBusinessObject["Full Name"];
			filter.IsActive = true;
			filter.Property = filterStripHelperTestPersonName;
		}

		const string filterStripHelperTestPersonName = "MODULE BASHER TEST";

		#endregion
	}
}
