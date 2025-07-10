using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class BulkUpdatePreviewPageTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageInfo()
		{
			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var page = wizard.Pages.First((p) => p is BulkUpdatePreviewPage);
				AssertNotNull("Prequisite", page);
				page.Visible = true;

				page.NotifyActivated(wizard);

				AssertEquals("Preview", wizard.PageHeaderTitle);
				AssertEquals("Please note only the first 50 applicable rates and their resulting changes will be displayed.", wizard.PageHeaderDescription);
			}
		}

		[TestDate(2015, 11, 10)]
		public void TestPageWarning_AllRatesToBeUpdated()
		{
			Enumerable.Range(0, 5).ForEach(x => CreateEntryWithLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "FRT", ZDate.Today.AddDays(x)));
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var updater = wizard.Updater;
				updater.Module = "FWD";
				updater.Type = RatingConstants.RateCategory.AIR;
				updater.Mode = Core.Constants.RateMode.LSE;

				updater.LoadEntries();
				AssertEquals("Expected entries to be found", 5, wizard.Updater.Entries.Count);

				updater.ActionsLine.TL_AC = Helper.ChargeCodes["FRT"].PK;

				var page = wizard.Pages.First(p => p is BulkUpdatePreviewPage);
				page.Visible = true;
				page.NotifyActivated(wizard);

				AssertEquals(false, wizard.Updater.HasAdditionalBatchesToProcess);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2015, 11, 10)]
		public void TestPageWarning_NoRatesToChange_DeleteExisting()
		{
			Enumerable.Range(0, 4).ForEach(x => CreateEntryWithLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "FRT", ZDate.Today.AddDays(x)));
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();
				var updater = wizard.Updater;
				updater.Module = "FWD";
				updater.Type = RatingConstants.RateCategory.AIR;
				updater.Mode = Core.Constants.RateMode.LSE;
				updater.Action = BulkRateUpdater.Actions.DeleteCharge;

				updater.LoadEntries();
				updater.ActionsLine.TL_AC = Helper.ChargeCodes["FRT"].PK;

				updater.CreateNewEntry = true;
				updater.NewEntryStartDate = ZDate.Today.AddDays(-10);
				updater.NewEntryEndDate = ZDate.Today.AddDays(-5);

				var page = wizard.Pages.First(p => p is BulkUpdatePreviewPage);
				page.Visible = true;
				page.NotifyActivated(wizard);

				AssertEquals(false, wizard.Updater.HasAdditionalBatchesToProcess);
				AssertEquals("No changes will be made as this action is not applicable to any of the selected 4 rate trade lane(s).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2015, 11, 10)]
		public void TestPageWarning_NoRatesToChange_ReplaceExisting()
		{
			Enumerable.Range(0, 4).ForEach(x => CreateEntryWithLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "FRT", ZDate.Today.AddDays(x)));
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();
				var updater = wizard.Updater;
				updater.Module = "FWD";
				updater.Type = RatingConstants.RateCategory.AIR;
				updater.Mode = Core.Constants.RateMode.LSE;
				updater.Action = BulkRateUpdater.Actions.ReplaceCharge;

				updater.LoadEntries();
				updater.ActionsLine.TL_AC = Helper.ChargeCodes["FRT"].PK;

				updater.CreateNewEntry = true;
				updater.NewEntryStartDate = ZDate.Today.AddDays(-10);
				updater.NewEntryEndDate = ZDate.Today.AddDays(-5);

				var page = wizard.Pages.First(p => p is BulkUpdatePreviewPage);
				page.Visible = true;
				page.NotifyActivated(wizard);

				var expected = "No changes will be made as this action is not applicable to any of the selected 4 rate trade lane(s).";

				AssertEquals(false, wizard.Updater.HasAdditionalBatchesToProcess);
				AssertEquals(expected, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2015, 11, 10)]
		public void TestPageWarning_SomeRatesToBeUpdated()
		{
			Enumerable.Range(0, 51).ForEach(x => CreateEntryWithLine(RatingConstants.RateCategory.AIR, Core.Constants.RateMode.LSE, "FRT", ZDate.Today.AddDays(x)));
			Factory.Save();

			UnitTestUserNotification.Instance.ClearMessages();

			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var updater = wizard.Updater;
				updater.Module = "FWD";
				updater.Type = RatingConstants.RateCategory.AIR;
				updater.Mode = Core.Constants.RateMode.LSE;

				updater.LoadEntries();

				updater.ActionsLine.TL_AC = Helper.ChargeCodes["FRT"].PK;

				var page = wizard.Pages.First(p => p is BulkUpdatePreviewPage);
				page.Visible = true;
				page.NotifyActivated(wizard);

				AssertEquals(true, wizard.Updater.HasAdditionalBatchesToProcess);
				AssertEquals("Only 50 of 51 results will be shown for performance reasons.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region Implementation

		RateEntry CreateEntryWithLine(ZString category, ZString mode, string chargeCode, ZDate startDate)
		{
			var clientRate = Helper.NewClientRate(Helper.NewOrgHeader());
			var rateEntry = clientRate.AddRateEntry(category, mode, "CN", "AU");
			rateEntry.TI_RateStartDate = startDate;
			rateEntry.IncludeInUpdate = true;

			var rateLine = rateEntry.RateLines[0];
			rateLine.TL_AC = Helper.ChargeCodes[chargeCode].PK;
			rateLine.TL_RateCalculator = FlatCalculator.Code;
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 10m;

			return rateEntry;
		}

		TestHelper helper;
		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}

		#endregion
	}
}
