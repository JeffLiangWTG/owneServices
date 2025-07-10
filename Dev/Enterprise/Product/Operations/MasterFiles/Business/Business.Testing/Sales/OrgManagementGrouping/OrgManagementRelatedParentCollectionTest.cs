using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgManagementRelatedParentCollection))]
	sealed class OrgManagementRelatedParentCollectionTest : OrgManagementRelatedCollectionTestCase<OrgManagementRelatedParentCollection>
	{
		public override void TestOrganisations()
		{
			var differentCompany = Factory.NewWithValidTestData<GlbCompany>();
			var orgA = Factory.NewWithValidTestData<OrgHeader>();
			var parentEntOrgA = Factory.NewWithValidTestData<OrgHeader>();
			OrgManagementRelatedPartyTestHelper.Create(Factory, parentEntOrgA, orgA);

			var orgB = Factory.NewWithValidTestData<OrgHeader>();
			var parentEntOrgB = Factory.NewWithValidTestData<OrgHeader>();
			var parentComOrgBForDifferentCompany = Factory.NewWithValidTestData<OrgHeader>();
			OrgManagementRelatedPartyTestHelper.Create(Factory, parentEntOrgB, orgB);
			OrgManagementRelatedPartyTestHelper.Create(Factory, parentComOrgBForDifferentCompany, orgB, differentCompany);

			var orgC = Factory.NewWithValidTestData<OrgHeader>();
			var parentEntOrgC = Factory.NewWithValidTestData<OrgHeader>();
			var parentComOrgC = Factory.NewWithValidTestData<OrgHeader>();
			var parentComOrgCForDifferentCompany = Factory.NewWithValidTestData<OrgHeader>();
			OrgManagementRelatedPartyTestHelper.Create(Factory, parentEntOrgC, orgC);
			OrgManagementRelatedPartyTestHelper.Create(Factory, parentComOrgC, orgC, GlbCompany.CurrentCompany);
			OrgManagementRelatedPartyTestHelper.Create(Factory, parentComOrgCForDifferentCompany, orgC, differentCompany);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, new[] { parentEntOrgA }, orgA.RelatedManagementParentRelations.Organisations);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, new[] { parentEntOrgB }, orgB.RelatedManagementParentRelations.Organisations);
			AssertContainsExactElementsInAnyOrder(BusinessObjectEqualityComparer<OrgHeader>.PKOnlyComparer, new[] { parentEntOrgC, parentComOrgC }, orgC.RelatedManagementParentRelations.Organisations);
		}

		public override void TestCheckIsValidOrganisation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "Org";
			var subOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			subOrg1.OH_Code = "SubOrg1";
			var subOrg1SubOrg = Factory.NewWithValidTestData<OrgHeader>();
			subOrg1SubOrg.OH_Code = "SubOrg1SubOr";
			var orgParents = org.RelatedManagementParentRelations;

			var validationResult = orgParents.CheckIsValidOrganisation(org, true);
			AssertValidationResult(validationResult, false, "Can not make Organization (Org) the parent of itself.");

			validationResult = orgParents.CheckIsValidOrganisation(subOrg1, false);
			AssertValidationResult(validationResult, true);

			validationResult = orgParents.CheckIsValidOrganisation(subOrg1, true);
			AssertValidationResult(validationResult, false, "Organization (SubOrg1) must be saved before it can be a parent of another organization.");

			Factory.Save();
			validationResult = orgParents.CheckIsValidOrganisation(subOrg1, true);
			AssertValidationResult(validationResult, true);

			subOrg1.RelatedManagementParentRelations.AddOrganisation(org);
			subOrg1SubOrg.RelatedManagementParentRelations.AddOrganisation(subOrg1);
			Factory.Save();
			validationResult = orgParents.CheckIsValidOrganisation(subOrg1SubOrg, true);
			AssertValidationResult(validationResult, false,
@"Organization (SubOrg1SubOr) is already a related descendant of Organization (Org).
Making Organization (SubOrg1SubOr) the parent of Organization (Org) would create an illegal cycle.

Conflicting relationship sequence: Organization (Org) > Organization (SubOrg1) > Organization (SubOrg1SubOr)");

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "Org2";
			Factory.Save();

			validationResult = subOrg1.RelatedManagementParentRelations.CheckIsValidOrganisation(org2, true);
			AssertValidationResult(validationResult, false,
@"Organization (Org) is already the parent of Organization (SubOrg1). Organization (SubOrg1) can only have one parent.");
		}

		public override void TestAddOrganisation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "org1";
			var org1ParentRelations = org1.RelatedManagementParentRelations;
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "org2";
			var org2ParentRelations = org2.RelatedManagementParentRelations;

			var addParentResult1 = org1ParentRelations.AddOrganisation(org2);
			AssertEquals("Should not be able to add relation to unsaved parent, if parent ends up not being saved, the relation will be invalid!", false, addParentResult1.Success);
			AssertEquals("Organization (org2) must be saved before it can be a parent of another organization.", addParentResult1.Reason);
			AssertCollectionNotContains(org2, org1ParentRelations.Organisations);

			var addParentResult2 = org2ParentRelations.AddOrganisation(org1);
			AssertEquals("It is ok for an unsaved organisation to add a relation to a saved parent - the relation will only remain if the organisation is saved.", true, addParentResult2.Success);
			AssertCollectionContains(org1, org2ParentRelations.Organisations);
		}

		public override void TestRemoveOrganisation()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "org1";
			var org1ParentRelations = org1.RelatedManagementParentRelations;
			Factory.Save();

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "org2";

			var removeParentResult1 = org1ParentRelations.RemoveOrganisation(org2);
			AssertEquals(false, removeParentResult1.Success);
			AssertEquals("Organization (org2) is not a parent of Organization (org1).", removeParentResult1.Reason);

			OrgManagementRelatedPartyTestHelper.Create(Factory, org2, org1);
			Factory.Save();
			AssertCollectionContains("Pre-condition", org2, org1ParentRelations.Organisations);
			var removeParentResult2 = org1ParentRelations.RemoveOrganisation(org2);
			AssertEquals(true, removeParentResult2.Success);
			AssertCollectionNotContains(org2, org1ParentRelations.Organisations);
		}

		#region Implementation

		protected override OrgManagementRelatedParentCollection GetCollectionToTest()
		{
			return new OrgManagementRelatedParentCollection(SubsidiaryOrg);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var parentOrg = Factory.NewWithValidTestData<OrgHeader>();
			return OrgManagementRelatedPartyTestHelper.Create(Factory, parentOrg, SubsidiaryOrg);
		}

		OrgHeader subsidiaryOrg;
		OrgHeader SubsidiaryOrg
		{
			get { return subsidiaryOrg ?? (subsidiaryOrg = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		#endregion
	}
}
