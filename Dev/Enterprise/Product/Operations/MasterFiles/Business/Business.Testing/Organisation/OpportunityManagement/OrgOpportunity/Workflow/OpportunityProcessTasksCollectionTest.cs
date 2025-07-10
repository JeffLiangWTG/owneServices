using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OpportunityProcessTasksCollection))]
	sealed class OpportunityProcessTasksCollectionTest : ProcessTaskCollectionTest<OpportunityProcessTasksCollection>
	{
		public void TestSupportsContactAndAddress()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			AssertEquals("Opportunity Task DOES support contacts and addresses", true, opportunity.WorkflowItems.SupportsContactAndAddress);
		}

		public void TestApplicableCountry()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "NZAKL";
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			AssertEquals("Should be NZ", "NZ", opportunity.WorkflowItems.OriginCountry);
			AssertEquals("Should be NZ", "NZ", opportunity.WorkflowItems.DestinationCountry);

			org.OH_RL_NKClosestPort = "";
			opportunity = org.SalesOpportunities.AddNew();
			AssertEquals("Should be empty", ZString.Empty, opportunity.WorkflowItems.OriginCountry);
			AssertEquals("Should be empty", ZString.Empty, opportunity.WorkflowItems.DestinationCountry);
		}

		public void TestCreateTasksFromTemplate()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = new OpportunityWorkflowDescriptor().Code;
			template.P0_SubType1 = "XYZ";
			ProcessTask task1 = template.WorkflowItems.AddNew();
			ProcessTask task2 = template.WorkflowItems.AddNew();

			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			OrgOpportunity opportunity = org.SalesOpportunities.AddNew();
			AssertEquals("No tasks", 0, opportunity.WorkflowItems.Count);

			Factory.Save();
			AssertEquals("No tasks loaded as opp type was empty", 0, opportunity.WorkflowItems.Count);

			opportunity.P8_OpportunityType = "XYZ";
			opportunity.WorkflowItems.AddNew();
			Factory.Save();
			AssertEquals("No template tasks loaded as task already existed", 1, opportunity.WorkflowItems.Count);

			opportunity.P8_OpportunityDescription = "changed";
			opportunity.WorkflowItems.RemoveAndDeleteAll();
			Factory.Save();
			AssertEquals("2 templated tasks loaded", 2, opportunity.WorkflowItems.Count);
		}

		public void TestDefaultsOnChild()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgAddress address = org.MainAddress;

			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			opp.P8_OH = org.PK;
			opp.P8_OC = contact.PK;
			opp.P8_OA = address.PK;
			opp.P8_GS_NKPrimarySalesPerson = GlbStaff.CurrentUser.GS_Code;

			ProcessTask task = opp.WorkflowItems.AddNew();
			AssertEquals("Task Org is Org Opportunity Org", opp.P8_OH, task.OrganisationPK);
			AssertEquals("Task Contact is Org Opportunity Contact", opp.P8_OC, task.P9_OC);
			AssertEquals("Task Address is Org Opportunity Contact", opp.P8_OA, task.P9_OA);
			AssertEquals("Task Assigned To is Org Opportunity Primary Sales Person", opp.P8_GS_NKPrimarySalesPerson, task.P9_GS_NKAssignedStaffMember);
			AssertEquals("Task ParentID is set", opp.PK, task.P9_ParentID);
			AssertEquals("Task ParentTableCode is set", OrgOpportunitySchema.Constants.Prefix, task.P9_ParentTableCode);
		}

		public void TestDefaultsOnChild_FromTemplate()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			GlbCapability cap = Factory.New<GlbCapability>();
			cap.G4_Code = "GGG";
			ProcessTask task = template.WorkflowItems.AddNew();
			task.P9_GS_NKAssignedStaffMember = "";
			task.P9_G4_RequiredCapability = cap.PK;
			task.P9_Sequence = 100;

			ProcessTask task2 = template.WorkflowItems.AddNew();
			task2.P9_GS_NKAssignedStaffMember = "";
			task2.P9_G4_RequiredCapability = ZGuid.Empty;
			task2.P9_Sequence = 200;

			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "FO";
			ProcessTask task3 = template.WorkflowItems.AddNew();
			task3.P9_GS_NKAssignedStaffMember = staff.GS_Code;
			task3.P9_G4_RequiredCapability = ZGuid.Empty;
			task3.P9_Sequence = 300;

			template.P0_ProcessType = "OPP";

			Factory.Save();

			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "WISEGRID";
			OrgContact contact = org.Contacts.AddNew();
			OrgAddress address = org.MainAddress;

			OrgOpportunity opp = Factory.New<OrgOpportunity>();
			opp.P8_OH = org.PK;
			opp.P8_OC = contact.PK;
			opp.P8_OA = address.PK;
			opp.P8_GS_NKPrimarySalesPerson = staff.GS_Code;

			Factory.Save();

			ProcessTask task1 = opp.WorkflowItems.Tasks[0];
			AssertEquals("No staff is assigned to task since capability has the assignment", "", task1.P9_GS_NKAssignedStaffMember);
			AssertEquals("ASN is set", "ASN", task1.P9_Status);
			AssertEquals("Task ParentID is set", opp.PK, task1.P9_ParentID);
			AssertEquals("Task ParentTableCode is set", OrgOpportunitySchema.Constants.Prefix, task1.P9_ParentTableCode);

			ProcessTask task21 = opp.WorkflowItems.Tasks[1];
			AssertEquals("No staff assigned and no Capability assigned, therefore, should set Staff to 'Sales Person'", "FO", task21.P9_GS_NKAssignedStaffMember);
			AssertEquals("ASN is set", "ASN", task21.P9_Status);
			AssertEquals("Task ParentID is set", opp.PK, task21.P9_ParentID);
			AssertEquals("Task ParentTableCode is set", OrgOpportunitySchema.Constants.Prefix, task21.P9_ParentTableCode);

			ProcessTask task31 = opp.WorkflowItems.Tasks[2];
			AssertEquals("Staff is assigned to task", "FO", task31.P9_GS_NKAssignedStaffMember);
			AssertEquals("ASN is set", "ASN", task31.P9_Status);
			AssertEquals("Task ParentID is set", opp.PK, task31.P9_ParentID);
			AssertEquals("Task ParentTableCode is set", OrgOpportunitySchema.Constants.Prefix, task31.P9_ParentTableCode);
		}

		protected override OpportunityProcessTasksCollection GetCollectionToTestCore()
		{
			OrgOpportunity opportunity = Factory.New<OrgOpportunity>();
			return new OpportunityProcessTasksCollection(opportunity);
		}
	}
}
