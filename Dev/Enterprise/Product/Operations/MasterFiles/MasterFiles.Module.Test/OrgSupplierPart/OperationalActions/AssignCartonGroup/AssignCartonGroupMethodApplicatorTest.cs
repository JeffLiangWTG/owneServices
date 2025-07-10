using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AssignCartonGroupMethodApplicator))]
	sealed class AssignCartonGroupMethodApplicatorTest : OperationalActionMethodApplicatorTest
	{
		#region TestAction

		public void TestAction()
		{
			var organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			var organisation2 = Factory.NewWithValidTestData<OrgHeader>();

			var cartonGroup1 = Factory.New<IWhsCartonGroup>();
			cartonGroup1.WCG_Code = "G1";
			cartonGroup1.WCG_Description = "Group";

			var cartonGroup2 = Factory.New<IWhsCartonGroup>();
			cartonGroup2.WCG_Code = "G2";
			cartonGroup2.WCG_Description = "Group";

			var product1 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var product2 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var product3 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var product4 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var product5 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var product6 = Factory.NewWithValidTestData<OrgSupplierPart>();
			var product7 = Factory.NewWithValidTestData<OrgSupplierPart>();
			product1.OP_PartNum = "P1";
			product2.OP_PartNum = "P2";
			product3.OP_PartNum = "P3";
			product4.OP_PartNum = "P4";
			product5.OP_PartNum = "P5";
			product6.OP_PartNum = "P6";
			product7.OP_PartNum = "P7";

			var products = new[] { product1, product2, product3, product4, product5, product6 };

			// product 1 is blank
			product2.RelatedOrganisations.AddOwner(organisation2); // Different organisation is owner
			product3.RelatedOrganisations.AddOwner(organisation1); // Same organisation is owner, blank carton group
			product4.RelatedOrganisations.AddOwner(organisation1).OU_WCG_CartonGroup = cartonGroup2.PK; // Same organisation is owner, attached to another carton group
			product5.RelatedOrganisations.AddOrganisationIfNotExist(organisation1.PK, OrgPartRelation.RelationshipTypes.Both); // Same org is both
			product6.RelatedOrganisations.AddSupplier(organisation1);

			Applicator.OrganisationPK = organisation1.PK;
			Applicator.CartonGroupPK = cartonGroup1.PK;
			ApplyApplicator(products,
@"INFO: Assigned Carton Group to P1.
INFO: Assigned Carton Group to P2.
INFO: Assigned Carton Group to P3.
INFO: Assigned Carton Group to P4.
INFO: Assigned Carton Group to P5.
INFO: Assigned Carton Group to P6.");

			AssertEquals("Should have changed Org1's relationship",
				cartonGroup1.PK,
				product1.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup);

			AssertEquals("Should not have changed Org2's relationship",
				ZGuid.Empty,
				product2.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation2.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup);

			AssertEquals("Should have changed Org1's relationship",
				cartonGroup1.PK,
				product3.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup);

			AssertEquals("Should have changed Org1's relationship",
				cartonGroup1.PK,
				product4.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup);

			AssertEquals("Should have changed Org1's relationship",
				cartonGroup1.PK,
				product5.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation1.PK, OrgPartRelation.RelationshipTypes.Both).OU_WCG_CartonGroup);

			AssertNull("Shouldnt have created an 'Owner' relationship",
				product5.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation1.PK, OrgPartRelation.RelationshipTypes.Owner));

			AssertEquals("Should have changed Org1's relationship",
				cartonGroup1.PK,
				product6.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup);

			AssertEquals("Should not have changed the 'Supplier' relationship",
				ZGuid.Empty,
				product6.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation1.PK, OrgPartRelation.RelationshipTypes.Supplier).OU_WCG_CartonGroup);

			// Test With Blank CartonGroup
			Applicator.CartonGroupPK = ZGuid.Empty;
			ApplyApplicator(new[] { product1, product7 },
@"INFO: Unassigned Carton Group from P1.
INFO: No Relationship to remove Carton Group from for P7.");

			AssertEquals("Should have changed Org1's relationship",
				ZGuid.Empty,
				product1.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation1.PK, OrgPartRelation.RelationshipTypes.Owner).OU_WCG_CartonGroup);

			AssertNull("Should not have created relationship for unassigning",
				product7.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(organisation1.PK, OrgPartRelation.RelationshipTypes.Owner));
		}

		#endregion

		#region TestAction_ErrorIfNothingEntered

		public void TestAction_ErrorIfNothingEntered()
		{
			var product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_PartNum = "P1";

			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var cartonGroup = Factory.New<IWhsCartonGroup>();
			cartonGroup.WCG_Code = "G1";
			cartonGroup.WCG_Description = "Group";

			ApplyApplicator(new[] { product }, "ERROR: Enter an Organization.");

			Applicator.OrganisationPK = organisation.PK;
			Applicator.CartonGroupPK = cartonGroup.PK;
			ApplyApplicator(new[] { product }, "INFO: Assigned Carton Group to P1.");
		}

		#endregion

		#region TestOrganisationPK

		public void TestOrganisationPK()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(AssignCartonGroupMethodApplicator), "OrganisationPK", false,
				la => la.ListDataSourceMember == "Lookups.Organisations");
		}

		#endregion

		#region TestCartonGroupPK

		public void TestCartonGroupPK()
		{
			AssertHasCustomAttribute<ListAttribute>(
				typeof(AssignCartonGroupMethodApplicator), "CartonGroupPK", false,
				la => la.ListDataSourceMember == "Lookups.CartonGroups");
		}

		#endregion

		#region GetNewBusinessObject

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AssignCartonGroupMethodApplicator("test", Factory);
		}

		#endregion

		new AssignCartonGroupMethodApplicator Applicator => (AssignCartonGroupMethodApplicator)base.Applicator;
	}
}
