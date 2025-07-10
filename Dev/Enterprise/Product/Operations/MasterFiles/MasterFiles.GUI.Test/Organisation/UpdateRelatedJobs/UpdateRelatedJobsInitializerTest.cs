using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class UpdateRelatedJobsInitializerTest : TestCaseWithFactory
	{
		#region organisation specific update test

		public void TestMenuItemsOnClick_Disabled()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "NEWCOD";
			Factory.Save();

			using (OrgFormForTest form = new OrgFormForTest(orgHeader))
			{
				form.Show();
				var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var item = actionsMenuItem.MenuItems.FindByText("Update Related Job Screening Status");

				orgHeader.Contacts.AddNew().OC_ContactName = "x";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				item.PerformClick();

				AssertEquals("Please save the form before running this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemsOnClick_Enabled()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "NEWCOD";
			orgHeader.OH_FullName = "some name";
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();
			using (OrgFormForTest form = new OrgFormForTest(orgHeader))
			{
				form.Show();
				var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var item = actionsMenuItem.MenuItems.FindByText("Update Related Job Screening Status");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				item.PerformClick();

				AssertNotEquals("Please save the form before running this function.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateUpdateRelatedJobsMenuForOrgEnabledMenuItem()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader[OrgHeaderSchema.OH_Code] = "NEWCOD";
			Factory.Save();

			using (OrgFormForTest form = new OrgFormForTest(orgHeader))
			{
				form.Show();
				var actionsMenuItem = form.Menu.MenuItems[ZFormMenuStrategy.ActionsMenuItemName];
				var item = actionsMenuItem.MenuItems.FindByText("Update Related Job Screening Status");

				CombineAssertions(() =>
				{
					AssertNotNull("Check Update Related Job Screening Status menu item exists", item);
					AssertEquals("Update Related Job Screening Status menu visible", true, item.Visible);
					AssertEquals("Update Related Job Screening Status menu enabled", true, item.Enabled);
				});
			}
		}

		#endregion

		#region generic tests

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateMenuOnNullParentFormArgument()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			new UpdateRelatedJobsInitializer(org).CreateUpdateRelatedJobsMenuItem(null, null);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestCreateMenuOnNullOrgHeaderArgument()
		{
			new UpdateRelatedJobsInitializer(null);
		}
		#endregion
	}
}
