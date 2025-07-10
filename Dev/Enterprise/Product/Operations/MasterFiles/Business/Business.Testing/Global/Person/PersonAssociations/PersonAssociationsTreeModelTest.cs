using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Recruiter;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(PersonAssociationsTreeModel))]
	sealed class PersonAssociationsTreeModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBuildTree()
		{
			ZString organizationCountryCode1 = "AUSYD";
			ZString organizationCountryCode2 = "USLAX";
			ZString fullName = "John Smith";

			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = fullName;

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = fullName;
			jobApplicant.HA_PER = person.PK;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_City = "Sydney";
			staff1.GS_FullName = fullName;
			staff1.GS_PER = person.PK;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_City = "Sydney";
			staff2.GS_IsActive = false;
			staff2.GS_FullName = fullName;
			staff2.GS_PER = person.PK;

			var organizationParent1 = Factory.NewWithValidTestData<OrgHeader>();
			organizationParent1.OH_FullName = "Test Organization Parent 1";
			organizationParent1.OH_Code = "TSTORGPNT1";
			organizationParent1.OH_IsActive = false;
			organizationParent1.OH_RL_NKClosestPort = organizationCountryCode1;

			var organizationParent2 = Factory.NewWithValidTestData<OrgHeader>();
			organizationParent2.OH_FullName = "Test Organization Parent 2";
			organizationParent2.OH_Code = "TSTORGPNT2";
			organizationParent2.OH_RL_NKClosestPort = organizationCountryCode2;

			var organization1 = Factory.NewWithValidTestData<OrgHeader>();
			organization1.OH_FullName = "Test Organization 1";
			organization1.OH_Code = "TSTORG1";
			organization1.OH_RL_NKClosestPort = organizationCountryCode1;

			var organization2 = Factory.NewWithValidTestData<OrgHeader>();
			organization2.OH_FullName = "Test Organization 2";
			organization2.OH_Code = "TSTORG2";
			organization2.OH_IsActive = false;
			organization2.OH_RL_NKClosestPort = organizationCountryCode1;

			var organization3 = Factory.NewWithValidTestData<OrgHeader>();
			organization3.OH_FullName = "Test Organization 3";
			organization3.OH_Code = "TSTORG3";
			organization3.OH_RL_NKClosestPort = organizationCountryCode2;

			var orgContact1 = Factory.NewWithValidTestData<OrgContact>();
			orgContact1.OC_ContactName = fullName;
			orgContact1.OC_IsActive = false;
			orgContact1.OC_OH = organizationParent1.PK;
			orgContact1.OC_PER = person.PK;

			var orgContact2 = Factory.NewWithValidTestData<OrgContact>();
			orgContact2.OC_ContactName = fullName;
			orgContact2.OC_OH = organizationParent2.PK;
			orgContact2.OC_PER = person.PK;

			var orgContact3 = Factory.NewWithValidTestData<OrgContact>();
			orgContact3.OC_ContactName = fullName;
			orgContact3.OC_OH = organization1.PK;
			orgContact3.OC_PER = person.PK;

			var orgContact4 = Factory.NewWithValidTestData<OrgContact>();
			orgContact4.OC_ContactName = fullName;
			orgContact4.OC_OH = organization2.PK;
			orgContact4.OC_PER = person.PK;

			var orgContact5 = Factory.NewWithValidTestData<OrgContact>();
			orgContact5.OC_ContactName = fullName;
			orgContact5.OC_OH = organization3.PK;
			orgContact5.OC_PER = person.PK;

			var orgContact6 = Factory.NewWithValidTestData<OrgContact>();
			orgContact6.OC_ContactName = fullName;
			orgContact6.OC_OH = organization3.PK;
			orgContact6.OC_PER = person.PK;

			var orgRelatedParty1 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty1.PR_OH_Parent = organization1.PK;
			orgRelatedParty1.PR_OH_RelatedParty = organizationParent1.PK;
			orgRelatedParty1.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			var orgRelatedParty2 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty2.PR_OH_Parent = organization2.PK;
			orgRelatedParty2.PR_OH_RelatedParty = organizationParent1.PK;
			orgRelatedParty2.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			var orgRelatedParty3 = Factory.NewWithValidTestData<OrgRelatedParty>();
			orgRelatedParty3.PR_OH_Parent = organization3.PK;
			orgRelatedParty3.PR_OH_RelatedParty = organizationParent1.PK;
			orgRelatedParty3.PR_PartyType = RelatedPartyTypeList.Codes.ManagementGrouping;

			var model = new PersonAssociationsTreeModel(person);
			model.BuildTree(true);

			var rootNodes = model.RootNodes;
			AssertEquals(5, rootNodes.Count);

			var organizationParent1Node = rootNodes[0];
			AssertEquals("Management Group", organizationParent1Node.BizObjForBinding.Grouping);

			var orgNodes = organizationParent1Node.ChildNodes;
			AssertEquals(5, orgNodes.Count());

			var organisationAU1 = orgNodes.ElementAt(0);
			AssertEquals(organizationParent1.OH_Code, organisationAU1.BizObjForBinding.Grouping);

			var organisationAU2 = orgNodes.ElementAt(1);
			AssertEquals(organization1.OH_Code, organisationAU2.BizObjForBinding.Grouping);

			var organisationAU3 = orgNodes.ElementAt(2);
			AssertEquals(organization2.OH_Code, organisationAU3.BizObjForBinding.Grouping);

			var organisationUSContact1 = orgNodes.ElementAt(3);
			AssertEquals(organization3.OH_Code, organisationUSContact1.BizObjForBinding.Grouping);

			var organisationUSContact2 = orgNodes.ElementAt(4);
			AssertEquals(organization3.OH_Code, organisationUSContact2.BizObjForBinding.Grouping);

			var organizationParent2Node = rootNodes[1];
			AssertEquals("Management Group", organizationParent2Node.BizObjForBinding.Grouping);

			orgNodes = organizationParent2Node.ChildNodes;
			AssertEquals(1, orgNodes.Count());

			organisationUSContact1 = orgNodes.ElementAt(0);
			AssertEquals(organizationParent2.OH_Code, organisationUSContact1.BizObjForBinding.Grouping);

			var staffNode1 = rootNodes[2];
			AssertEquals("Staff", staffNode1.BizObjForBinding.Grouping);

			var staffNode2 = rootNodes[3];
			AssertEquals("Staff", staffNode2.BizObjForBinding.Grouping);

			var jobApplicantNode = rootNodes[4];
			AssertEquals("Applicant", jobApplicantNode.BizObjForBinding.Grouping);

			model = new PersonAssociationsTreeModel(person);
			model.BuildTree(false);
			rootNodes = model.RootNodes;
			AssertEquals(4, rootNodes.Count);
			organizationParent1Node = rootNodes[0];
			orgNodes = organizationParent1Node.ChildNodes;
			AssertEquals(4, orgNodes.Count());

			var staffNode = rootNodes[2];
			AssertEquals("Staff", staffNode.BizObjForBinding.Grouping);
		}

		public void TestBuildTree_NeedRefreshCollection()
		{
			var fullName = "John Smith";

			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_FullName = fullName;

			var jobApplicant = Factory.New<IHRJobApplicant>();
			jobApplicant.HA_FullName = fullName;
			jobApplicant.HA_PER = person.PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_City = "Sydney";
			staff.GS_FullName = fullName;
			staff.GS_PER = person.PK;

			var model = new PersonAssociationsTreeModel(person);
			AssertEquals(2, model.RootNodes.Count);

			person.ApplicantCollection.Remove(jobApplicant.PK);
			model.BuildTree(true, false);
			AssertEquals("JobApplicant had been removed", 1, model.RootNodes.Count);

			model.BuildTree(true);
			AssertEquals("ApplicantCollection had been reloaded", 2, model.RootNodes.Count);
		}

		#region Overrides

		protected override BusinessObject GetNewBusinessObject()
		{
			var person = Factory.New<GlbPerson>();
			return new PersonAssociationsTreeModel(person);
		}

		#endregion
	}
}
