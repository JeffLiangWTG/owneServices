using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesEnquiryProcessTaskCollection))]
	sealed class SalesEnquiryProcessTaskCollectionTest : ProcessTaskCollectionTest<SalesEnquiryProcessTaskCollection>
	{
		public void TestSupportsContactAndAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			SalesEnquiry enquiry = CreateEnquiry(org);
			AssertEquals("OrgEnquiry Task SupportsContactAndAddress", false, enquiry.WorkflowItems.SupportsContactAndAddress);
		}

		public void TestApplicableCountry()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "NZ";
			SalesEnquiry enquiry = CreateEnquiry(org);
			AssertEquals("Should be NZ", "NZ", enquiry.WorkflowItems.OriginCountry);
			AssertEquals("Should be NZ", "NZ", enquiry.WorkflowItems.DestinationCountry);

			org.MainAddress.OA_RN_NKCountryCode = "";
			enquiry = CreateEnquiry(org);
			AssertEquals("Should be empty", ZString.Empty, enquiry.WorkflowItems.OriginCountry);
			AssertEquals("Should be empty", ZString.Empty, enquiry.WorkflowItems.DestinationCountry);
		}

		public void TestCreateTasksFromTemplate()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = SalesEnquiryWorkflowDescriptor.WorkflowTypeCode;
			template.P0_SubType2 = "111";
			ProcessTask taskA = template.WorkflowItems.AddNew();
			ProcessTask taskB = template.WorkflowItems.AddNew();
			taskA.P9_GS_NKAssignedStaffMember = "ZZ";
			taskB.P9_GS_NKAssignedStaffMember = "PM";

			template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = SalesEnquiryWorkflowDescriptor.WorkflowTypeCode;
			template.P0_SubType2 = "222";
			taskA = template.WorkflowItems.AddNew();
			taskB = template.WorkflowItems.AddNew();
			taskA.P9_GG_AssignedGroup = Core.Constants.Groups.PostMastersGroupPK;
			taskB.P9_GG_AssignedGroup = Core.Constants.Groups.AllPK;

			template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = SalesEnquiryWorkflowDescriptor.WorkflowTypeCode;
			template.P0_SubType2 = "333";
			template.WorkflowItems.AddNew();
			template.WorkflowItems.AddNew();

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			SalesEnquiry enquiry = CreateEnquiry(org);
			AssertEquals("No tasks", 0, enquiry.WorkflowItems.Count);
			enquiry.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("No tasks loaded as opp type was empty", 0, enquiry.WorkflowItems.Count);

			enquiry.O1_LeadSource = "111";
			enquiry.WorkflowItems.RemoveAndDeleteAll();
			enquiry.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("2 templated tasks loaded", 2, enquiry.WorkflowItems.Count);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template when template has value", "ZZ", enquiry.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "PM", enquiry.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			enquiry.O1_LeadSource = "222";
			enquiry.WorkflowItems.RemoveAndDeleteAll();
			enquiry.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("2 templated tasks loaded", 2, enquiry.WorkflowItems.Count);
			AssertEquals("P9_GS_NKAssignedStaffMember is empty because group is assigned", "", enquiry.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "", enquiry.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			enquiry.O1_LeadSource = "333";
			enquiry.WorkflowItems.RemoveAndDeleteAll();
			enquiry.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("2 templated tasks loaded", 2, enquiry.WorkflowItems.Count);
			AssertEquals("P9_GS_NKAssignedStaffMember is current user when no Sales Rep and template has no user", GlbStaff.CurrentUser.GS_Code, enquiry.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "", enquiry.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			enquiry.WorkflowItems.RemoveAndDeleteAll();
			enquiry.O1_LeadSource = "111";
			enquiry.O1_GS_NKRepAssigned = "X";
			enquiry.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("P9_GS_NKAssignedStaffMember is from template when template has value", "ZZ", enquiry.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "PM", enquiry.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			enquiry.WorkflowItems.RemoveAndDeleteAll();
			enquiry.O1_LeadSource = "222";
			enquiry.O1_GS_NKRepAssigned = "X";
			enquiry.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("P9_GS_NKAssignedStaffMember is Sales Rep when template has no user", "X", enquiry.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "", enquiry.WorkflowItems[1].P9_GS_NKAssignedStaffMember);

			enquiry.WorkflowItems.RemoveAndDeleteAll();
			enquiry.O1_LeadSource = "333";
			enquiry.O1_GS_NKRepAssigned = "X";
			enquiry.WorkflowItems.Tasks.CreateItemsFromTemplate();
			AssertEquals("P9_GS_NKAssignedStaffMember is Sales Rep when template has no user", "X", enquiry.WorkflowItems[0].P9_GS_NKAssignedStaffMember);
			AssertEquals("P9_GS_NKAssignedStaffMember is from template for second task", "", enquiry.WorkflowItems[1].P9_GS_NKAssignedStaffMember);
		}

		public void TestDefaultsOnChild()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgAddress address = org.MainAddress;

			SalesEnquiry enquiry = CreateEnquiry(org);
			enquiry.O1_GS_NKRepAssigned = GlbStaff.CurrentUser.GS_Code;
			enquiry.OrgPk = org.PK;

			ProcessTask task = enquiry.WorkflowItems.AddNew();
			AssertEquals("Task Org is Enquiry Org", enquiry.OrgPk, task.OrganisationPK);
			AssertEquals("Task Address is Org Address", org.MainAddress.PK, task.P9_OA);
			AssertEquals("Task Assigned To is blank", "", task.P9_GS_NKAssignedStaffMember);
		}

		SalesEnquiry CreateEnquiry(OrgHeader org)
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			enquiry.OrgPk = org.PK;
			return enquiry;
		}

		protected override SalesEnquiryProcessTaskCollection GetCollectionToTestCore()
		{
			SalesEnquiry enquiry = Factory.New<SalesEnquiry>();
			return new SalesEnquiryProcessTaskCollection(enquiry);
		}
	}
}
