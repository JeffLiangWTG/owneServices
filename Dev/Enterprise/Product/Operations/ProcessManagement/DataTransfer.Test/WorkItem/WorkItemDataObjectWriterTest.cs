using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	class WorkItemDataObjectWriterTest : ActivityDataObjectWriterTestCase<WorkItem>
	{
		protected override WorkItem GetBusinessObject()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(BizoFactory);
			workItem.WKI_Details = ZBlob.FromUTF8("Get your shit together.");
			workItem.WKI_WorkItemType = "AAA";
			workItem.WKI_WorkItemArea = "BBB";
			workItem.WKI_ActivityType = "CCC";
			workItem.WKI_ActivitySubtype = "DDD";
			workItem.WKI_Priority = "EEE";

			workItem.WKI_GE_AssignedDepartment = CreateDepartment().PK;
			workItem.WKI_GC_AssignedCompany = CreateCompany().PK;
			workItem.WKI_PortOrCountry = "CAYHZ";

			workItem.Cancel(); // so that the status is something interesting, not just the default.

			return workItem;
		}

		protected override IEnumerable<IWorkTaskRelatedItem> GetRelatedItems()
		{
			var project = ProcessMgmtTestHelper.CreateProject(BizoFactory, "Yeah, I'd like to order one large phone with extra phones please.");
			project.WKP_ProjectNumber = "PROJECTNUM";
			yield return project;

			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(BizoFactory, "Cellphone. No, no, no, no. Rotary. And pay phone on half.");
			ticket.WKR_RequestNumber = "TICKETNUM";
			yield return ticket;
		}

		protected override IWorkTaskRelatedItem GetSecondLevelRelatedItem()
		{
			return ProcessMgmtTestHelper.CreateWorkItem(BizoFactory);
		}

		protected override string ExpectedSummary => "I hope our prices aren't too low!";
		protected override string ExpectedDescription => "Get your shit together.";
		protected override string ExpectedStatus => "CAN";
		protected override string ExpectedSelectionCriterion1 => "AAA";
		protected override string ExpectedSelectionCriterion2 => "BBB";
		protected override string ExpectedSelectionCriterion3 => "CCC";
		protected override string ExpectedSelectionCriterion4 => "DDD";
		protected override string ExpectedSelectionCriterion5 => "EEE";
		protected override string ExpectedBranchCode => null;
		protected override string ExpectedDepartmentCode => "DEP";
		protected override string ExpectedLocation => "CAYHZ";
		protected override string ExpectedCompanyCode => "COM";
		protected override string ExpectedProjectManagerCode => null;
		protected override string ExpectedClient1Name => null;
		protected override string ExpectedClient1Address1 => null;
		protected override string ExpectedClient2Name => null;
		protected override string ExpectedClient2Address1 => null;
		protected override IEnumerable<string> ExpectedRelatedItems => new[]
		{
			"Project: PROJECTNUM - Yeah, I'd like to order one large phone with extra phones please.",
			"CustomerServiceTicket: TICKETNUM - Cellphone. No, no, no, no. Rotary. And pay phone on half.",
		};

		public void TestPopulateDataObject_NullDepartment()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(BizoFactory);
			AssertNull(workItem.AssignedDepartment);

			var activity = ActivityTestHelper.WriteActivity(workItem);
			AssertNull(activity.Department);
		}

		public void TestPopulateDataObject_EmptyLocation()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(BizoFactory);
			AssertEquals(ZString.Empty, workItem.WKI_PortOrCountry);

			var activity = ActivityTestHelper.WriteActivity(workItem);
			AssertEquals(ZString.Empty, activity.Location.Code);
		}

		public void TestPopulateDataObject_NullCompany()
		{
			var workItem = ProcessMgmtTestHelper.CreateWorkItem(BizoFactory);
			AssertNull(workItem.AssignedCompany);

			var activity = ActivityTestHelper.WriteActivity(workItem);
			AssertNull(activity.Company);
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.WorkItem;
	}
}
