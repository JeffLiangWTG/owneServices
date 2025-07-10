using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.Testing
{
	public class BulkUpdateFilterPageTest : TestCaseWithFactory
	{
		public void TestContainerTypesGrid_DisableImportDataMenuItemShouldBeTrue()
		{
			using (var bulkUpdateWizard = new BulkUpdateWizard())
			{
				bulkUpdateWizard.Show();

				var bulkUpdateFilterPage = (BulkUpdateFilterPage)bulkUpdateWizard.Pages.Single((page) => page is BulkUpdateFilterPage);
				var containerTypesGrid = (ZGrid)bulkUpdateFilterPage.Controls.Find("ContainerTypesGrid", searchAllChildren: true).Single();
				AssertEquals("DisableImportDataMenuItem", expected: true, containerTypesGrid.DisableImportDataMenuItem);
			}
		}

		[ExpectNoExceptions]
		public void TestPageInfo()
		{
			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var page = wizard.Pages.First((p) => p is BulkUpdateFilterPage);
				AssertNotNull("Prequisite", page);
				page.Visible = true;

				page.NotifyActivated(wizard);

				AssertEquals("Filter", wizard.PageHeaderTitle);
				AssertEquals("Enter the criteria for the rates that you wish to update", wizard.PageHeaderDescription);
			}
		}

		public void TestValidationContainerTypes_GivenInvalidContainerType_WhenLeavingPage_ThenShouldShowError()
		{
			using (var bulkUpdateWizard = new BulkUpdateWizard())
			{
				bulkUpdateWizard.Show();

				var bulkUpdateFilterPage = bulkUpdateWizard.Pages.Single((page) => page is BulkUpdateFilterPage);
				bulkUpdateFilterPage.Visible = true;

				var bulkRateUpdater = bulkUpdateWizard.Updater;

				bulkRateUpdater.Module = "FWD";
				bulkRateUpdater.Type = RatingConstants.RateCategory.FCL;
				bulkRateUpdater.Mode = RateMode.SEA;

				bulkRateUpdater.ContainerTypes.AddNew();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				bulkUpdateFilterPage.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("Validation Error", "You have provided incorrect filter criteria", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestValidation()
		{
			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var page = wizard.Pages.First((p) => p is BulkUpdateFilterPage);
				AssertNotNull("Prequisite", page);
				AssertNotNull("Prequisite", wizard.Updater);
				page.Visible = true;

				BulkRateUpdater updater = wizard.Updater;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("You have provided incorrect filter criteria", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Back));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				updater.Module = "XXX";
				updater.Type = "AIR";
				updater.Mode = "LSE";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("You have provided incorrect filter criteria", UnitTestUserNotification.Instance.LastMessage.Text);

				updater.Module = "FWD";
				updater.Type = "AIR";
				updater.Mode = "LSE";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				updater.GatewayAgentType = "ABC";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("You have provided incorrect filter criteria", UnitTestUserNotification.Instance.LastMessage.Text);

				updater.GatewayAgentType = "RAG";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				updater.ShowIntercompanyTariffs = true;
				updater.ShowClientRates = true;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("You have provided incorrect filter criteria", UnitTestUserNotification.Instance.LastMessage.Text);

				updater.ShowIntercompanyTariffs = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				updater.ShowIntercompanyTariffs = true;
				updater.ShowClientRates = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);

				updater.ShipmentConsolidationStatus = "SHP";

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				page.NotifyLeaving(new WizardSteppingEventArgs(WizardSteppingEventArgs.Direction.Forward));
				AssertEquals("You have provided incorrect filter criteria", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestGUIControls()
		{
			using (var wizard = new BulkUpdateWizard())
			{
				wizard.Show();

				var page = wizard.Pages.First((p) => p is BulkUpdateFilterPage);

				AssertNotNull("Prequisite", page);
				Assert("Page should have GatewayAgentTypeDropEdit control", page.Controls.Find("GatewayAgentTypeDropEdit", true).Length == 1);
				Assert("Page should have IntercompanyTariffsCheckBox control", page.Controls.Find("IntercompanyTariffsCheckBox", true).Length == 1);
			}
		}
	}
}
