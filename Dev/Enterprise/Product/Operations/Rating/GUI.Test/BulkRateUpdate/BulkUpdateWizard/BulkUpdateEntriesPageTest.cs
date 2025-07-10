using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class BulkUpdateEntriesPageTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageInfo()
		{
			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var page = wizard.Pages.First((p) => p is BulkUpdateEntriesPage);
				AssertNotNull("Prequisite", page);
				page.Visible = true;

				page.NotifyActivated(wizard);

				AssertEquals("Results", wizard.PageHeaderTitle);
				AssertEquals("Review results matching the selected criteria", wizard.PageHeaderDescription);
			}
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());
			rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX");
			Factory.Save();

			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var page = wizard.Pages.First((p) => p is BulkUpdateEntriesPage);
				AssertNotNull("Prequisite", page);
				AssertNotNull("Prequisite", wizard.Updater);
				page.Visible = true;

				BulkRateUpdater updater = wizard.Updater;
				updater.Module = "FWD";
				updater.Type = "AIR";
				updater.Mode = "LSE";

				updater.LoadEntries();

				AssertEquals("Prequisite", 1, updater.Entries.Count);
				AssertEquals("Prequisite", 1, CountSelected(updater));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				updater.Entries[0].IncludeInUpdate = false;
				AssertEquals("Prequisite", 0, CountSelected(updater));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("You must select at least one entry for update", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Back));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		int CountSelected(BulkRateUpdater updater)
		{
			return updater.Entries.Cast<RateEntry>().Count((e) => e.IncludeInUpdate);
		}

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}

		TestHelper helper;
	}
}
