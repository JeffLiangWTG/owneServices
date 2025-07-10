using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ProcessManagement.Integration;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.ProcessManagement.DataTransfer.Test
{
	class CustomerServiceTicketDataObjectWriterTest : ActivityDataObjectWriterTestCase<WorkRequest>
	{
		protected override WorkRequest GetBusinessObject()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(BizoFactory, "Celebrity Baby Plastic Surgery Disasters", "AAA", "BBB", "CCC", "DDD", "EEE", CreateBranch(), CreateDepartment(), "LB");
			ticket.WKR_Description = "Lonely Gal Margarita Mix for One";
			ticket.WKR_Status = "CAN";
			ticket.WKR_OC_Client = CreateContact().PK;

			var otherContact = ticket.Client.Header.Contacts.AddNew();
			otherContact.OC_ContactName = "C Dubya";
			otherContact.OC_Email = "cdubs@hunrath.com";

			return ticket;
		}

		protected override IEnumerable<IWorkTaskRelatedItem> GetRelatedItems()
		{
			var workitem1 = ProcessMgmtTestHelper.CreateWorkItem(BizoFactory, "Yeah, I'd like to order one large phone with extra phones please.");
			var workitem2 = ProcessMgmtTestHelper.CreateWorkItem(BizoFactory, "Cellphone. No, no, no, no. Rotary. And pay phone on half.");
			var workitem3 = ProcessMgmtTestHelper.CreateWorkItem(BizoFactory, "This one shant be related.");

			workitem1.WKI_WorkItemNumber = "WKI 1";
			workitem2.WKI_WorkItemNumber = "WKI 2";
			workitem3.WKI_WorkItemNumber = "WKI 3";

			yield return workitem1;
			yield return workitem2;
		}

		protected override IWorkTaskRelatedItem GetSecondLevelRelatedItem()
		{
			return ProcessMgmtTestHelper.CreateProject(BizoFactory, client: CreateContact());
		}

		protected override IEnumerable<string> GetExpectedExtraMessagesForORP()
		{
			return new[] { "Jan Michael Vincent (VANINDSYD) has been added to the conversation., System, internal" };
		}

		protected override string ExpectedSummary => "Celebrity Baby Plastic Surgery Disasters";
		protected override string ExpectedDescription => "Lonely Gal Margarita Mix for One";
		protected override string ExpectedStatus => "CAN";
		protected override string ExpectedSelectionCriterion1 => "AAA";
		protected override string ExpectedSelectionCriterion2 => "BBB";
		protected override string ExpectedSelectionCriterion3 => "CCC";
		protected override string ExpectedSelectionCriterion4 => "DDD";
		protected override string ExpectedSelectionCriterion5 => "EEE";
		protected override string ExpectedLocation => "LB";
		protected override string ExpectedBranchCode => "BRA";
		protected override string ExpectedDepartmentCode => "DEP";
		protected override string ExpectedCompanyCode => null;
		protected override string ExpectedProjectManagerCode => null;
		protected override string ExpectedClient1Name => "Jan Michael Vincent";
		protected override string ExpectedClient1Address1 => "123 Canada Street";
		protected override string ExpectedClient2Name => null;
		protected override string ExpectedClient2Address1 => null;
		protected override IEnumerable<string> ExpectedRelatedItems => new[]
		{
			"WorkItem: WKI 1 - Yeah, I'd like to order one large phone with extra phones please.",
			"WorkItem: WKI 2 - Cellphone. No, no, no, no. Rotary. And pay phone on half."
		};

		public void TestPopulateDataObject_NullBranch()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(BizoFactory);
			var activity = ActivityTestHelper.WriteActivity(ticket);

			AssertNull(activity.Branch);
		}

		public void TestPopulateDataObject_NullDepartment()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(BizoFactory);
			var activity = ActivityTestHelper.WriteActivity(ticket);

			AssertNull(activity.Department);
		}

		public void TestPopulateDataObject_NullCountry()
		{
			var ticket = ProcessMgmtTestHelper.CreateWorkRequest(BizoFactory);
			var activity = ActivityTestHelper.WriteActivity(ticket);

			AssertEquals(ZString.Empty, activity.Location?.Code);
		}

		protected override DataContextType ExpectedDataContextType => DataContextType.CustomerServiceTicket;
	}
}
