using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.AMS.Business;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.AMS.GUI.Testing
{
	class StowPlanUserControlTest : TestCaseWithFactory
	{
		public void TestMessageStatusBarShowIssueMoreDetails()
		{
			var voyage = Factory.New<JobVoyage>();
			var sailingData = new StowPlanSailingData(voyage);
			sailingData.IssueCollection.AddNew(ZGuid.Empty, RefContainerStockSchema.Constants.Prefix, "Stock", "XXX", "More details", NotificationType.MessageError);
			sailingData.IssueCollectionView.Rebuild();
			using (var form = new StowPlanForm(sailingData))
			{
				form.Show();
				form.stowPlanUserControl.issuesGrid.CurrentRowIndex = 0;
				form.stowPlanUserControl.issuesGrid_MouseClick(null, null);
				AssertEquals("More details", form.MessageStatusBarPanel.Text);
			}
		}

		public void TestSendButtonClick()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			voyage.GenerateSailings();
			var sendingObj = new StowPlanSailingData(voyage);
			sendingObj.Arrival = ZString.Empty;
			using (var form = new StowPlanForm(sendingObj))
			{
				form.Show();
				form.stowPlanUserControl.SendButton_Click(null, null);
				AssertEquals("Please fix the errors before sending to US Customs.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDoubleClickOnIssueGrid()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUSYD";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "USLAX";
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.GenerateSailings();
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			bill.JS_JX = voyage.Sailings[0].PK;
			var container = bill.RealContainers.AddNew();
			var stock = Factory.NewWithValidTestData<RefContainerStock>();
			container.JC_ContainerNum = stock.R6_ContainerNum;
			Factory.Save();
			var sendingObj = new StowPlanSailingData(Factory.New<JobVoyage>());
			var vesselIssue = sendingObj.IssueCollection.AddNew(vessel.PK, RefVesselSchema.Constants.Prefix, "Vessel", "XXX", "XXX", NotificationType.Error);
			var billIssue = sendingObj.IssueCollection.AddNew(bill.PK, JobShipmentSchema.Constants.Prefix, "Bill", "XXX", "XXX", NotificationType.Error);
			var containerIssue = sendingObj.IssueCollection.AddNew(container.PK, JobContainerSchema.Constants.Prefix, "Container", "XXX", "XXX", NotificationType.Error);
			var stockIssue = sendingObj.IssueCollection.AddNew(stock.PK, RefContainerStockSchema.Constants.Prefix, "Stock", "XXX", "XXX", NotificationType.Error);
			var billIssueNoPK = sendingObj.IssueCollection.AddNew(ZGuid.Empty, JobShipmentSchema.Constants.Prefix, "Bill", "XXX", "XXX", NotificationType.Error);
			var containerIssueNoPK = sendingObj.IssueCollection.AddNew(ZGuid.Empty, JobContainerSchema.Constants.Prefix, "Container", "XXX", "XXX", NotificationType.Error);
			var stockIssueNoPK = sendingObj.IssueCollection.AddNew(ZGuid.Empty, RefContainerStockSchema.Constants.Prefix, "Stock", "XXX", "XXX", NotificationType.Error);
			using (var form = new StowPlanForm(new StowPlanSailingData(voyage)))
			{
				form.Show();
				using (var openForm = form.stowPlanUserControl.OpenIssue(vesselIssue))
				{
					AssertEquals(ControllerIDs.RefVessel, openForm.ControllerID);
				}

				using (var openForm = form.stowPlanUserControl.OpenIssue(billIssue))
				{
					AssertEquals(ControllerIDs.AgencyBillOfLading, openForm.ControllerID);
				}

				using (var openForm = form.stowPlanUserControl.OpenIssue(containerIssue))
				{
					AssertEquals(ControllerIDs.AgencyBillContainers, openForm.ControllerID);
				}

				using (var openForm = form.stowPlanUserControl.OpenIssue(stockIssue))
				{
					AssertEquals(ControllerIDs.AgencyContainerManager, openForm.ControllerID);
				}

				using (var openForm = form.stowPlanUserControl.OpenIssue(billIssueNoPK))
				{
					var filterModule = ((EmbeddedModulePopup)openForm).Module_ForTest;
					AssertEquals(ModuleIDs.AgencyBillOfLading, filterModule.ID);
				}

				using (var openForm = form.stowPlanUserControl.OpenIssue(containerIssueNoPK))
				{
					var filterModule = ((EmbeddedModulePopup)openForm).Module_ForTest;
					AssertEquals(ModuleIDs.AgencyBillContainers, filterModule.ID);
				}

				using (var openForm = form.stowPlanUserControl.OpenIssue(stockIssueNoPK))
				{
					var filterModule = ((EmbeddedModulePopup)openForm).Module_ForTest;
					AssertEquals(ModuleIDs.AgencyContainerManager, filterModule.ID);
				}
			}
		}
	}
}
