using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallProcessTaskCollection))]
	sealed class OrgSalesCallProcessTaskCollectionTest : ProcessTaskCollectionTest<OrgSalesCallProcessTaskCollection>
	{
		public void TestSupportsContactAndAddress()
		{
			var org = Factory.New<OrgHeader>();
			var salesCall = org.SalesCalls.AddNew();
			AssertEquals("OrgEnquiry Task SupportsContactAndAddress", false, salesCall.WorkflowItems.SupportsContactAndAddress);
		}

		public void TestCreateTasksFromTemplate()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = OrgSalesCallWorkflowDescriptor.WorkflowTypeCode;
			template1.P0_SubType1 = "111";
			var task1A = template1.WorkflowItems.AddNew();
			var task1B = template1.WorkflowItems.AddNew();
			task1A.P9_GS_NKAssignedStaffMember = "ZZ";
			task1B.P9_GS_NKAssignedStaffMember = "PM";

			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = OrgSalesCallWorkflowDescriptor.WorkflowTypeCode;
			template2.P0_SubType1 = "222";
			var task2A = template2.WorkflowItems.AddNew();
			var task2B = template2.WorkflowItems.AddNew();
			task2A.P9_GG_AssignedGroup = Core.Constants.Groups.PostMastersGroupPK;
			task2B.P9_GG_AssignedGroup = Core.Constants.Groups.AllPK;

			ProcessTaskTemplate template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = OrgSalesCallWorkflowDescriptor.WorkflowTypeCode;
			template3.P0_SubType1 = "333";
			template3.WorkflowItems.AddNew();
			template3.WorkflowItems.AddNew();

			Factory.Save();

			var org = Factory.New<OrgHeader>();
			var salesCall = org.SalesCalls.AddNew();
			AssertEquals("No tasks", 0, salesCall.WorkflowItems.Count);
			salesCall.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("No tasks loaded as opp type was empty", 0, salesCall.WorkflowItems.Count);

			salesCall.OQ_TypeOfCall = "111";
			salesCall.WorkflowItems.RemoveAndDeleteAll();
			salesCall.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("2 templated tasks loaded", 2, salesCall.WorkflowItems.Count);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template when template has value", "ZZ", salesCall.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "PM", salesCall.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			salesCall.OQ_TypeOfCall = "222";
			salesCall.WorkflowItems.RemoveAndDeleteAll();
			salesCall.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("2 templated tasks loaded", 2, salesCall.WorkflowItems.Count);
			AssertEquals("P9_GS_NKAssignedStaffMember is current user when no Sales Rep and template has no user", GlbStaff.CurrentUser.GS_Code, salesCall.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "", salesCall.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			salesCall.OQ_TypeOfCall = "333";
			salesCall.WorkflowItems.RemoveAndDeleteAll();
			salesCall.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("2 templated tasks loaded", 2, salesCall.WorkflowItems.Count);
			AssertEquals("P9_GS_NKAssignedStaffMember is current user when no Sales Rep and template has no user", GlbStaff.CurrentUser.GS_Code, salesCall.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "", salesCall.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			salesCall.WorkflowItems.RemoveAndDeleteAll();
			salesCall.OQ_TypeOfCall = "111";
			salesCall.OQ_GS_NKSalesRep = "X";
			salesCall.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("P9_GS_NKAssignedStaffMember is from template when template has value", "ZZ", salesCall.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "PM", salesCall.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			salesCall.WorkflowItems.RemoveAndDeleteAll();
			salesCall.OQ_TypeOfCall = "222";
			salesCall.OQ_GS_NKSalesRep = "X";
			salesCall.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("P9_GS_NKAssignedStaffMember is Sales Rep when template has no user", "X", salesCall.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "", salesCall.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			salesCall.WorkflowItems.RemoveAndDeleteAll();
			salesCall.OQ_TypeOfCall = "333";
			salesCall.OQ_GS_NKSalesRep = "X";
			salesCall.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("P9_GS_NKAssignedStaffMember is Sales Rep when template has no user", "X", salesCall.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "", salesCall.WorkflowItems[1].P9_GS_NKAssignedStaffMember);
		}

		public void TestDefaultsOnChild()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgAddress address1 = org.MainAddress;
			OrgAddress address2 = org.Addresses.AddNew();

			var salesCall1 = org.SalesCalls.AddNew();
			salesCall1.OQ_GS_NKSalesRep = GlbStaff.CurrentUser.GS_Code;
			salesCall1.OQ_OH = org.PK;

			ProcessTask task1 = salesCall1.WorkflowItems.AddNew();
			AssertEquals("Task Org is Call Org", salesCall1.OQ_OH, task1.OrganisationPK);
			AssertEquals("Task Address is Org Address", org.MainAddress.PK, task1.P9_OA);
			AssertEquals("Task Assigned To is blank", "", task1.P9_GS_NKAssignedStaffMember);

			var salesCall2 = org.SalesCalls.AddNew();
			salesCall2.OQ_GS_NKSalesRep = GlbStaff.CurrentUser.GS_Code;
			salesCall2.OQ_OH = org.PK;
			salesCall2.OQ_OA_LocationAddress = address2.PK;

			ProcessTask task2 = salesCall2.WorkflowItems.AddNew();
			AssertEquals("Task Address is sales call Address", address2.PK, task2.P9_OA);
		}

		public void TestProcessTaskTypeDecider()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesCall = org.SalesCalls.AddNew();
			salesCall.OQ_TypeOfCall = "111";
			var task1A = salesCall.WorkflowItems.AddNew();
			Factory.Save();

			var factory2 = new BusinessObjectFactory() { RefreshEnabled = false };
			var asProcessTask = factory2.Load<ProcessTask>(task1A.PK);
			AssertType(typeof(OrgSalesCallProcessTask), asProcessTask);
		}

		protected override OrgSalesCallProcessTaskCollection GetCollectionToTestCore()
		{
			OrgSalesCall salesCall = Factory.New<OrgSalesCall>();
			return new OrgSalesCallProcessTaskCollection(salesCall);
		}
	}
}
