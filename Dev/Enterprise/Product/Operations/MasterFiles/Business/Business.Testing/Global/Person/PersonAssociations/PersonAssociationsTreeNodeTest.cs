using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PersonAssociationsTreeNodeTest : TestCaseWithFactory
	{
		const string OrganizationCountryCode1 = "AUSYD";
		const string OrganizationCountryCode2 = "USLAX";
		const string FullName = "John Smith";

		GlbPerson person;
		GlbStaff staff;
		IHRJobApplicant jobApplicant;
		OrgHeader organizationParent1;
		OrgHeader organizationParent2;
		OrgHeader organization1;
		OrgHeader organization2;
		OrgHeader organization3;
		OrgContact orgContact1;
		OrgContact orgContact2;
		OrgContact orgContact3;
		OrgContact orgContact4;
		OrgContact orgContact5;
		OrgRelatedParty orgRelatedParty1;
		OrgRelatedParty orgRelatedParty2;
		OrgRelatedParty orgRelatedParty3;
		PersonAssociationsTreeModel treeModel;

		void SetupTestData()
		{
			person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = FullName;

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_OH_OrgProxy = Factory.NewWithValidTestData<OrgHeader>().PK;
			branch.GB_RL_NKHomePort = "AUSYD";
			branch.GB_City = "Sydney";
			branch.GB_State = "WSN";

			staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = FullName;
			staff.GS_PER = person.PK;
			staff.GS_GB_HomeBranch = branch.PK;

			jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = FullName;
			jobApplicant.HA_PER = person.PK;

			organizationParent1 = Factory.NewWithValidTestData<OrgHeader>();
			organizationParent1.OH_Code = "TSTORGPNT1";
			organizationParent1.OH_FullName = "Test Organization Parent 1";
			organizationParent1.OH_RL_NKClosestPort = OrganizationCountryCode1;

			organizationParent2 = Factory.NewWithValidTestData<OrgHeader>();
			organizationParent2.OH_Code = "TSTORGPNT2";
			organizationParent2.OH_FullName = "Test Organization Parent 2";
			organizationParent2.OH_RL_NKClosestPort = OrganizationCountryCode2;

			organization1 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_Code = "TSTORG1";
			organization1.OH_FullName = "Test Organization 1";
			organization1.OH_RL_NKClosestPort = OrganizationCountryCode1;

			organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization2.OH_Code = "TSTORG2";
			organization2.OH_FullName = "Test Organization 2";
			organization2.OH_RL_NKClosestPort = OrganizationCountryCode1;

			organization3 = Factory.NewWithValidTestData<OrgHeader>();
			organization3.OH_Code = "TSTORG3";
			organization3.OH_FullName = "Test Organization 3";
			organization3.OH_RL_NKClosestPort = OrganizationCountryCode2;

			orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			orgContact1.OC_ContactName = FullName;
			orgContact1.OC_OH = organizationParent1.PK;
			orgContact1.OC_PER = person.PK;

			orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			orgContact2.OC_ContactName = FullName;
			orgContact2.OC_OH = organizationParent2.PK;
			orgContact2.OC_PER = person.PK;

			orgContact3 = Factory.NewWithValidTestData<OrgContact>();
			orgContact3.OC_ContactName = FullName;
			orgContact3.OC_OH = organization1.PK;
			orgContact3.OC_PER = person.PK;

			orgContact4 = Factory.NewWithValidTestData<OrgContact>();
			orgContact4.OC_ContactName = FullName;
			orgContact4.OC_OH = organization2.PK;
			orgContact4.OC_PER = person.PK;

			orgContact5 = Factory.NewWithValidTestData<OrgContact>();
			orgContact5.OC_ContactName = FullName;
			orgContact5.OC_OH = organization3.PK;
			orgContact5.OC_PER = person.PK;

			orgRelatedParty1 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty1.PR_OH_Parent = organization1.PK;
			orgRelatedParty1.PR_OH_RelatedParty = organizationParent1.PK;
			orgRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			orgRelatedParty2 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty2.PR_OH_Parent = organization2.PK;
			orgRelatedParty2.PR_OH_RelatedParty = organizationParent1.PK;
			orgRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			orgRelatedParty3 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty3.PR_OH_Parent = organization3.PK;
			orgRelatedParty3.PR_OH_RelatedParty = organizationParent1.PK;
			orgRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			treeModel = new PersonAssociationsTreeModel(person);
			treeModel.BuildTree(true);
		}

		public void TestActive()
		{
			SetupTestData();

			var orgParent1Node = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsOrgGroupWrapper &&
				((PersonAssociationsOrgGroupWrapper)x.BizObj).Organization == organizationParent1);
			AssertEquals(organizationParent1.OH_IsActive, orgParent1Node.Active);

			var orgParent2Node = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsOrgGroupWrapper &&
				((PersonAssociationsOrgGroupWrapper)x.BizObj).Organization == organizationParent2);
			AssertEquals(organizationParent2.OH_IsActive, orgParent2Node.Active);

			var staffNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsStaffWrapper && ((PersonAssociationsStaffWrapper)x.BizObj).Staff == staff);
			AssertEquals(staff.GS_IsActive, staffNode.Active);

			var jobApplicantNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsJobApplicantWrapper &&
				((PersonAssociationsJobApplicantWrapper)x.BizObj).JobApplicant == jobApplicant);
			AssertEquals(true, jobApplicantNode.Active);

			var organization1Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization1);
			AssertEquals(organization3.OH_IsActive, organization1Node.Active);

			var organization2Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization2);
			AssertEquals(organization2.OH_IsActive, organization2Node.Active);

			var organization3Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization3);
			AssertEquals(organization3.OH_IsActive, organization3Node.Active);
		}

		public void TestCity()
		{
			SetupTestData();

			var orgParent1Node = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsOrgGroupWrapper &&
				((PersonAssociationsOrgGroupWrapper)x.BizObj).Organization == organizationParent1);
			AssertEquals(ZString.Empty, orgParent1Node.City);

			var orgParent2Node = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsOrgGroupWrapper &&
				((PersonAssociationsOrgGroupWrapper)x.BizObj).Organization == organizationParent2);
			AssertEquals(ZString.Empty, orgParent2Node.City);

			var staffNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsStaffWrapper && ((PersonAssociationsStaffWrapper)x.BizObj).Staff == staff);
			AssertEquals("Sydney", staffNode.City);

			var jobApplicantNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsJobApplicantWrapper &&
				((PersonAssociationsJobApplicantWrapper)x.BizObj).JobApplicant == jobApplicant);
			AssertEquals(jobApplicant.HA_City, jobApplicantNode.City);

			var organization1Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization1);
			AssertEquals(ZString.Empty, organization1Node.City);

			var organization2Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization2);
			AssertEquals(ZString.Empty, organization2Node.City);

			var organization3Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization3);
			AssertEquals(ZString.Empty, organization3Node.City);
		}

		public void TestDescription()
		{
			SetupTestData();

			var orgParent1Node = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsOrgGroupWrapper &&
				((PersonAssociationsOrgGroupWrapper)x.BizObj).Organization == organizationParent1);

			var staffNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsStaffWrapper && ((PersonAssociationsStaffWrapper)x.BizObj).Staff == staff);
			AssertEquals($"{staff.GS_FullName}", staffNode.Description);

			var jobApplicantNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsJobApplicantWrapper &&
				((PersonAssociationsJobApplicantWrapper)x.BizObj).JobApplicant == jobApplicant);
			AssertEquals(jobApplicant.HA_FullName, jobApplicantNode.Description);

			var organization1Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization1);
			AssertEquals(organization1.OH_FullName, organization1Node.Description);

			var organization2Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization2);
			AssertEquals(organization2.OH_FullName, organization2Node.Description);

			var organization3Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization3);
			AssertEquals(organization3.OH_FullName, organization3Node.Description);
		}

		public void TestGrouping()
		{
			SetupTestData();

			var orgParent1Node = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsOrgGroupWrapper &&
				((PersonAssociationsOrgGroupWrapper)x.BizObj).Organization == organizationParent1);
			AssertEquals("Management Group", orgParent1Node.Grouping);

			var orgParent2Node = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsOrgGroupWrapper &&
				((PersonAssociationsOrgGroupWrapper)x.BizObj).Organization == organizationParent2);
			AssertEquals("Management Group", orgParent2Node.Grouping);

			var staffNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsStaffWrapper && ((PersonAssociationsStaffWrapper)x.BizObj).Staff == staff);
			AssertEquals("Staff", staffNode.Grouping);

			var jobApplicantNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsJobApplicantWrapper &&
				((PersonAssociationsJobApplicantWrapper)x.BizObj).JobApplicant == jobApplicant);
			AssertEquals("Applicant", jobApplicantNode.Grouping);

			var organization1Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization1);
			AssertEquals(organization1.OH_Code, organization1Node.Grouping);

			var organization2Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization2);
			AssertEquals(organization2.OH_Code, organization2Node.Grouping);

			var organization3Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization3);
			AssertEquals(organization3.OH_Code, organization3Node.Grouping);
		}

		public void TestState()
		{
			SetupTestData();

			var orgParent1Node = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsOrgGroupWrapper &&
				((PersonAssociationsOrgGroupWrapper)x.BizObj).Organization == organizationParent1);
			AssertEquals(ZString.Empty, orgParent1Node.State);

			var orgParent2Node = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsOrgGroupWrapper &&
				((PersonAssociationsOrgGroupWrapper)x.BizObj).Organization == organizationParent2);
			AssertEquals(ZString.Empty, orgParent2Node.State);

			var staffNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsStaffWrapper && ((PersonAssociationsStaffWrapper)x.BizObj).Staff == staff);
			AssertEquals("WSN", staffNode.State);

			var jobApplicantNode = (PersonAssociationsTreeNode)treeModel.RootNodes.Single(x =>
				x.BizObj is PersonAssociationsJobApplicantWrapper &&
				((PersonAssociationsJobApplicantWrapper)x.BizObj).JobApplicant == jobApplicant);
			AssertEquals(jobApplicant.HA_State, jobApplicantNode.State);

			var organization1Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization1);
			AssertEquals("NSW", organization1Node.State);

			var organization2Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization2);
			AssertEquals("NSW", organization2Node.State);

			var organization3Node = (PersonAssociationsTreeNode)orgParent1Node.ChildNodes.Single(x =>
				x.BizObj is PersonAssociationsOrganizationWrapper &&
				((PersonAssociationsOrganizationWrapper)x.BizObj).Organization == organization3);
			AssertEquals("CA", organization3Node.State);
		}
	}
}
