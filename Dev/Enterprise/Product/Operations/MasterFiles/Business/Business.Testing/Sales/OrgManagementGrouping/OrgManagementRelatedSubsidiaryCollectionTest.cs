using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgManagementRelatedSubsidiaryCollection))]
	sealed class OrgManagementRelatedSubsidiaryCollectionTest : OrgManagementRelatedCollectionTestCase<OrgManagementRelatedSubsidiaryCollection>
	{
		public override void TestOrganisations()
		{
			var differentCompany = Factory.NewWithValidTestData<GlbCompany>();

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var subOrgA = Factory.NewWithValidTestData<OrgHeader>();
			var subOrgB = Factory.NewWithValidTestData<OrgHeader>();
			var subOrgC = Factory.NewWithValidTestData<OrgHeader>();

			OrgManagementRelatedPartyTestHelper.Create(Factory, org1, subOrgA);

			OrgManagementRelatedPartyTestHelper.Create(Factory, org1, subOrgB, GlbCompany.CurrentCompany);
			OrgManagementRelatedPartyTestHelper.Create(Factory, org2, subOrgB);

			OrgManagementRelatedPartyTestHelper.Create(Factory, org1, subOrgC, differentCompany);
			OrgManagementRelatedPartyTestHelper.Create(Factory, org2, subOrgC, GlbCompany.CurrentCompany);
			OrgManagementRelatedPartyTestHelper.Create(Factory, org3, subOrgC);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, new[] { subOrgA, subOrgB }, org1.RelatedManagementSubsidiaryRelations.Organisations);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, new[] { subOrgB, subOrgC }, org2.RelatedManagementSubsidiaryRelations.Organisations);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, new[] { subOrgC }, org3.RelatedManagementSubsidiaryRelations.Organisations);
		}

		public override void TestCheckIsValidOrganisation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "Org";
			var subOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			subOrg1.OH_Code = "SubOrg1";
			var subOrg1SubOrg = Factory.NewWithValidTestData<OrgHeader>();
			subOrg1SubOrg.OH_Code = "SubOrg1SubOr";
			var orgSubsidiaries = org.RelatedManagementSubsidiaryRelations;

			var validationResult = orgSubsidiaries.CheckIsValidOrganisation(org, true);
			AssertValidationResult(validationResult, false, "Can not make Organization (Org) a subsidiary of itself.");

			validationResult = orgSubsidiaries.CheckIsValidOrganisation(subOrg1, false);
			AssertValidationResult(validationResult, true);

			validationResult = orgSubsidiaries.CheckIsValidOrganisation(subOrg1, true);
			AssertValidationResult(validationResult, false, "Organization (SubOrg1) must be saved before it can be a subsidiary of another organization.");

			Factory.Save();
			validationResult = orgSubsidiaries.CheckIsValidOrganisation(subOrg1, true);
			AssertValidationResult(validationResult, true);

			org.RelatedManagementSubsidiaryRelations.AddOrganisation(subOrg1);
			subOrg1.RelatedManagementSubsidiaryRelations.AddOrganisation(subOrg1SubOrg);
			Factory.Save();
			validationResult = orgSubsidiaries.CheckIsValidOrganisation(subOrg1SubOrg, true);
			AssertValidationResult(validationResult, false, "Organization (SubOrg1) is already the parent of Organization (SubOrg1SubOr). Organization (SubOrg1SubOr) can only have one parent.");
		}

		public override void TestAddOrganisation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "org1";
			var org1SubsidiaryRelations = org1.RelatedManagementSubsidiaryRelations;
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "org2";
			var org2SubsidiaryRelations = org2.RelatedManagementSubsidiaryRelations;

			var addSubsidiaryResult1 = org1SubsidiaryRelations.AddOrganisation(org2);
			AssertEquals("Should not be able to add relation to unsaved subsidiary, if subsidiary ends up not being saved, the relation will be invalid!", false, addSubsidiaryResult1.Success);
			AssertEquals("Organization (org2) must be saved before it can be a subsidiary of another organization.", addSubsidiaryResult1.Reason);
			AssertCollectionNotContains(org2, org1SubsidiaryRelations.Organisations);

			var addSubsidiaryResult2 = org2SubsidiaryRelations.AddOrganisation(org1);
			AssertEquals("It is ok for an unsaved organisation to add a relation to a saved subsidiary - the relation will only remain if the organisation is saved.", true, addSubsidiaryResult2.Success);
			AssertCollectionContains(org1, org2SubsidiaryRelations.Organisations);
		}

		public override void TestRemoveOrganisation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "org1";
			var org1SubsidiaryRelations = org1.RelatedManagementSubsidiaryRelations;
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "org2";

			var removeSubsidiaryResult1 = org1SubsidiaryRelations.RemoveOrganisation(org2);
			AssertEquals(false, removeSubsidiaryResult1.Success);
			AssertEquals("Organization (org2) is not a subsidiary of Organization (org1).", removeSubsidiaryResult1.Reason);

			var org3 = Factory.NewWithValidTestData<OrgHeader>();
			org3.OH_Code = "org3";
			var org4 = Factory.NewWithValidTestData<OrgHeader>();
			org4.OH_Code = "org4";

			OrgManagementRelatedPartyTestHelper.Create(Factory, org1, org2);
			OrgManagementRelatedPartyTestHelper.Create(Factory, org1, org3, GlbCompany.CurrentCompany);
			OrgManagementRelatedPartyTestHelper.Create(Factory, org1, org4, Factory.NewWithValidTestData<GlbCompany>());
			Factory.Save();
			AssertCollectionContains("Pre-condition", org2, org1SubsidiaryRelations.Organisations);
			var removeSubsidiaryResult2 = org1SubsidiaryRelations.RemoveOrganisation(org2);
			AssertEquals(true, removeSubsidiaryResult2.Success);
			AssertCollectionNotContains(org2, org1SubsidiaryRelations.Organisations);
		}

		#region Implementation

		protected override OrgManagementRelatedSubsidiaryCollection GetCollectionToTest()
		{
			return new OrgManagementRelatedSubsidiaryCollection(ParentOrg);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var subsidiaryOrg = Factory.NewWithValidTestData<OrgHeader>();
			return OrgManagementRelatedPartyTestHelper.Create(Factory, ParentOrg, subsidiaryOrg);
		}

		OrgHeader parentOrg;
		OrgHeader ParentOrg
		{
			get { return parentOrg ?? (parentOrg = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		#endregion
	}
}
