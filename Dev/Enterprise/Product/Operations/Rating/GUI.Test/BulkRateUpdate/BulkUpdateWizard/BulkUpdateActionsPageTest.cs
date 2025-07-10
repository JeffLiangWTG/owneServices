using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Rating.GUI.Testing
{
	public class BulkUpdateActionsPageTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestPageInfo()
		{
			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var page = wizard.Pages.First((p) => p is BulkUpdateActionsPage);
				AssertNotNull("Prequisite", page);
				page.Visible = true;

				page.NotifyActivated(wizard);

				AssertEquals("Actions", wizard.PageHeaderTitle);
				AssertEquals("State the charge code that will be added, replaced, increased/decreased or deleted", wizard.PageHeaderDescription);
			}
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			var rate = Helper.NewClientRate(Helper.NewOrgHeader());
			rate.AddRateEntry("AIR", "LSE", "AUSYD", "USLAX").AddRateLine("FRT", UnitCalculator.Code, QuantityUnit.KG).Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;
			Factory.Save();

			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var page = wizard.Pages.First((p) => p is BulkUpdateActionsPage);
				AssertNotNull("Prequisite", page);
				AssertNotNull("Prequisite", wizard.Updater);
				page.Visible = true;

				var updater = wizard.Updater;
				updater.Module = "FWD";
				updater.Type = "AIR";
				updater.Mode = "LSE";

				updater.LoadEntries();

				AssertEquals("Prequisite", 1, updater.Entries.Count);
				AssertEquals("Prequisite", 1, CountSelected(updater));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("Prequisite", true, updater.ActionsLine.HasErrors());
				AssertEquals("Unable to continue this action...", ZFormModaliser.LastFormShownDialogForTest.Text);
				ZFormModaliser.LastFormShownDialogForTest = null;

				updater.ActionsLine.TL_AC = Helper.ChargeCodes["FRT"].PK;
				updater.ActionsLine.TL_WeightVolume = "KG";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("Prequisite", false, updater.HasErrors() || updater.ActionsLine.HasErrors());
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);

				updater.CreateNewEntry = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("Prequisite", true, updater.HasErrors());
				AssertEquals("You have provided incorrect actions criteria", UnitTestUserNotification.Instance.LastMessage.Text);

				updater.NewEntryStartDate = ZDate.Today.AddDays(-100);
				updater.NewEntryEndDate = ZDate.Today.AddDays(100);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("Prequisite", false, updater.HasErrors() || updater.ActionsLine.HasErrors());
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);

				updater.ActionsLine.OverrideChargeDescription = true;
				updater.ActionsLine.TL_RateDesc = "";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("Prequisite", true, updater.ActionsLine.HasErrors());
				AssertEquals("Unable to continue this action...", ZFormModaliser.LastFormShownDialogForTest.Text);
				ZFormModaliser.LastFormShownDialogForTest = null;

				updater.ActionsLine.TL_WeightVolume = "";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				updater.DeleteCharge = true;
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("Prequisite", true, updater.ActionsLine.HasErrors());
				AssertNull("No error message for delete action", ZFormModaliser.LastFormShownDialogForTest);

				updater.ActionsLine.TL_AC = ZGuid.Empty;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("Prequisite", true, updater.ActionsLine.HasErrors());
				AssertEquals("Error message for delete action if no charge code", "Unable to continue this action...", ZFormModaliser.LastFormShownDialogForTest.Text);
			}
		}

		int CountSelected(BulkRateUpdater updater)
		{
			return updater.Entries.Cast<RateEntry>().Count((e) => e.IncludeInUpdate);
		}

		public void TestResetChargeCodesLookupListWhenActivate()
		{
			Helper.ChargeCodes.New("WWTEST", "Warehouse Test", "UNT", ChargeCodeGroupList.Codes.WHSStorage, "");

			ClientRate rate = Helper.NewClientRate(Helper.NewOrgHeader());
			rate.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX").AddRateLine("OCART", UnitCalculator.Code, QuantityUnit.KG).Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)50m;
			rate.AddRateEntry("WHS", "ALL", "", "").AddRateLine("WWTEST", UnitCalculator.Code, QuantityUnit.KG).Calculator[Calculator.Items.Operator.UNT] = (ZDecimal)100m;
			Factory.Save();

			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				BulkRateUpdater updater = wizard.Updater;
				updater.Module = "FWD";
				updater.Type = "ORG";
				updater.Mode = "ALL";
				updater.LoadEntries();
				AssertEquals("Prequisite", 1, updater.Entries.Count);
				AssertEquals("Prequisite", 1, CountSelected(updater));

				var page = wizard.Pages.First((p) => p is BulkUpdateActionsPage);
				page.Visible = true;

				page.NotifyActivated(wizard);
				updater.ActionsLine.TL_AC = Helper.ChargeCodes["OCART"].PK;
				updater.ActionsLine.RunPreSaveValidation();
				AssertEquals(false, updater.ActionsLine.TL_ACInfo.HasErrors());

				updater.Module = "WRH";
				updater.Type = "WHS";
				updater.Mode = "ALL";
				updater.LoadEntries();
				AssertEquals("Prequisite", 1, updater.Entries.Count);
				AssertEquals("Prequisite", 1, CountSelected(updater));

				page.NotifyActivated(wizard);
				updater.ActionsLine.TL_AC = Helper.ChargeCodes["WWTEST"].PK;
				updater.ActionsLine.RunPreSaveValidation();
				AssertEquals(false, updater.ActionsLine.TL_ACInfo.HasErrors());
			}
		}

		TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;
	}
}
