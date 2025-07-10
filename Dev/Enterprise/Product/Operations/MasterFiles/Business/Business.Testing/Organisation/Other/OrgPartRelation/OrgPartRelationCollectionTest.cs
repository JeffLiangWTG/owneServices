using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgPartRelationCollection))]
	sealed class OrgPartRelationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestFindByOH_CodeAndRelationship()
		{
			OrgSupplierPart product = Factory.NewWithValidTestData<OrgSupplierPart>();

			OrgHeader organisation1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader organisation2 = Factory.NewWithValidTestData<OrgHeader>();

			OrgPartRelation relation1 = product.RelatedOrganisations[0];
			relation1.OU_OH = organisation1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			OrgPartRelation relation2 = product.RelatedOrganisations.AddNew();
			relation2.OU_OH = organisation2.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			AssertEquals("Find by Code and relationship", relation1, product.RelatedOrganisations.FindByOH_CodeAndRelationship(organisation1.OH_Code, OrgPartRelation.RelationshipTypes.Owner));
			AssertNull("Find by Code and relationship", product.RelatedOrganisations.FindByOH_CodeAndRelationship(organisation1.OH_Code, OrgPartRelation.RelationshipTypes.Supplier));

			AssertEquals("Find by Code and relationship", relation2, product.RelatedOrganisations.FindByOH_CodeAndRelationship(organisation2.OH_Code, OrgPartRelation.RelationshipTypes.Owner));
			AssertEquals("Find by Code and relationship", relation2, product.RelatedOrganisations.FindByOH_CodeAndRelationship(organisation2.OH_Code, OrgPartRelation.RelationshipTypes.Supplier));

			AssertNull("Find by Code and relationship", product.RelatedOrganisations.FindByOH_CodeAndRelationship("", OrgPartRelation.RelationshipTypes.Supplier));
		}

		public void TestBuyerRelations()
		{
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			AssertEquals("One relation", 1, part.RelatedOrganisations.BuyerRelations.Length);
			AssertEquals("One relation", relation2, part.RelatedOrganisations.BuyerRelations[0]);

			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			AssertEquals("two relations", 2, part.RelatedOrganisations.BuyerRelations.Length);

			OrgPartRelation relation3 = part.RelatedOrganisations.AddNew();
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			AssertEquals("three relations", 3, part.RelatedOrganisations.BuyerRelations.Length);
		}

		public void TestGetMarginPercentages()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			OrgHeader orgHeader2 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();

			relation1.OU_OH = orgHeader1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_OH = orgHeader2.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			relation2.OU_LandedCostMarginPercent1 = 0.2m;
			relation2.OU_LandedCostMarginPercent2 = 0.3m;
			relation2.OU_LandedCostMarginPercent3 = 0.4m;

			LCMarginPercentages margin1 = part.RelatedOrganisations.GetLCMarginPercentagesForFallBack(orgHeader1);
			AssertEquals("There are no values", false, margin1.HasValues);

			LCMarginPercentages margin2 = part.RelatedOrganisations.GetLCMarginPercentagesForFallBack(orgHeader2);
			AssertEquals("Values are there", true, margin2.HasValues);
			AssertEquals("LCMarginPercentage1", 0.2m, margin2.LCMarginPercentage1);
			AssertEquals("LCMarginPercentage2", 0.3m, margin2.LCMarginPercentage2);
			AssertEquals("LCMarginPercentage3", 0.4m, margin2.LCMarginPercentage3);

			LCMarginPercentages margin3 = part.RelatedOrganisations.GetLCMarginPercentagesForFallBack(null);
			AssertEquals("Has no value", false, margin3.HasValues);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			return new OrgPartRelationCollection(part, Factory);
		}

		[ExpectNoExceptions()]
		public void TestCreation()
		{
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelationCollection collection = new OrgPartRelationCollection(part, Factory);
		}

		public void TestCheckForDuplicates()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			OrgHeader orgHeader2 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();

			relation1.OU_OH = orgHeader1.PK;
			relation2.OU_OH = orgHeader2.PK;

			Assert(!relation1.HasNotifications());
			Assert(!relation2.HasNotifications());

			relation1.OU_OH = orgHeader2.PK;

			Assert(relation1.HasNotifications());
			Assert(!relation2.HasNotifications());
		}

		public void TestSameOrganisationDifferentRelationshipIsNotError()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();

			relation1.OU_OH = orgHeader1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			relation2.OU_OH = orgHeader1.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			relation1.RunPreSaveValidation();
			relation2.RunPreSaveValidation();

			Assert(!relation1.HasNotifications());
			Assert(relation2.HasNotifications());
		}

		public void TestFindByOrganisationPKAndRelationship()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			OrgHeader orgHeader2 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_OH = orgHeader1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();
			relation2.OU_OH = orgHeader2.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			OrgPartRelation relationFound1 = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationFound2 = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(orgHeader2.PK, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationNotFound = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(ZGuid.NewZGuid(), OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals("Relation 1 found", relation1.PK, relationFound1.PK);
			AssertEquals("Relation 2 found", relation2.PK, relationFound2.PK);
			AssertNull("Relation not found", relationNotFound);
		}

		public void TestFindByOrganisationPKAndExactRelationship()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			OrgHeader orgHeader2 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_OH = orgHeader1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();
			relation2.OU_OH = orgHeader2.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			OrgPartRelation relationFound1 = part.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationFound2 = part.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(orgHeader2.PK, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationNotFound = part.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(ZGuid.NewZGuid(), OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals("Relation 1 found", relation1.PK, relationFound1.PK);
			AssertEquals("Relation 2 found", relation2.PK, relationFound2.PK);
			AssertNull("Relation not found", relationNotFound);
		}

		public void TestFindByOrganisationPKAndRelationshipWithBoth1()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_OH = orgHeader1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();
			relation2.OU_OH = orgHeader1.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			OrgPartRelation relation3 = part.RelatedOrganisations.AddNew();
			relation3.OU_OH = orgHeader1.PK;
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			OrgPartRelation relationFound = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Both);
			AssertEquals("Relation found", relation3.PK, relationFound.PK);
		}

		public void TestFindByOrganisationPKAndExactRelationshipWithBoth1()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_OH = orgHeader1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();
			relation2.OU_OH = orgHeader1.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			OrgPartRelation relation3 = part.RelatedOrganisations.AddNew();
			relation3.OU_OH = orgHeader1.PK;
			relation3.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			OrgPartRelation relationFound = part.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Both);
			AssertEquals("Relation found", relation3.PK, relationFound.PK);
		}

		public void TestFindByOrganisationPKAndRelationshipWithBoth2()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_OH = orgHeader1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			OrgPartRelation relationFound1 = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationFound2 = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			OrgPartRelation relationFound3 = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Both);
			OrgPartRelation relationNotFound = part.RelatedOrganisations.FindByOrganisationPKAndRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.WarehouseConsignee);
			AssertEquals("Relation found", relation1.PK, relationFound1.PK);
			AssertEquals("Relation found", relation1.PK, relationFound2.PK);
			AssertEquals("Relation found", relation1.PK, relationFound3.PK);
			AssertNull("Relation not found", relationNotFound);
		}

		public void TestFindByOrganisationPKAndExactRelationshipWithBoth2()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = orgHeader1.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;

			OrgPartRelation relation1 = part.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relation2 = part.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			OrgPartRelation relation3 = part.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Both);
			OrgPartRelation relation4 = part.RelatedOrganisations.FindByOrganisationPKAndExactRelationship(orgHeader1.PK, OrgPartRelation.RelationshipTypes.WarehouseConsignee);
			AssertNull("Relation not found", relation1);
			AssertNull("Relation not found", relation2);
			AssertEquals("Relation found", relation.PK, relation3.PK);
			AssertNull("Relation not found", relation4);
		}

		public void TestFindByOrganisationAndRelationship()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			OrgHeader orgHeader2 = Factory.New<OrgHeader>();

			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			OrgPartRelation relation1 = part.RelatedOrganisations.AddNew();
			relation1.OU_OH = orgHeader1.PK;
			relation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			OrgPartRelation relation2 = part.RelatedOrganisations.AddNew();
			relation2.OU_OH = orgHeader2.PK;
			relation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			OrgPartRelation relationFound1 = part.RelatedOrganisations.FindByOrganisationAndRelationship(orgHeader1, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationFound2 = part.RelatedOrganisations.FindByOrganisationAndRelationship(orgHeader2, OrgPartRelation.RelationshipTypes.Owner);
			OrgPartRelation relationNotFound = part.RelatedOrganisations.FindByOrganisationAndRelationship(null, OrgPartRelation.RelationshipTypes.Owner);

			AssertEquals("Relation 1 found", relation1.PK, relationFound1.PK);
			AssertEquals("Relation 2 found", relation2.PK, relationFound2.PK);
			AssertNull("Relation not found", relationNotFound);
		}

		public void TestAddDeleteOrganisations()
		{
			OrgHeader orgHeader1 = Factory.New<OrgHeader>();
			OrgHeader orgHeader2 = Factory.New<OrgHeader>();
			OrgSupplierPart part = OrgSupplierPart.New(Factory);
			AssertEquals("No relationships initially", part.RelatedOrganisations.Count, 0);

			part.RelatedOrganisations.AddOrganisationIfNotExist(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Owner);
			AssertEquals("Relationship added, should be 1", 1, part.RelatedOrganisations.Count);
			part.RelatedOrganisations.AddOrganisationIfNotExist(orgHeader2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			AssertEquals("Relationship added, should be 2", 2, part.RelatedOrganisations.Count);

			part.RelatedOrganisations.AddOrganisationIfNotExist(orgHeader2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			AssertEquals("Duplicate relationship added, should be 2", 2, part.RelatedOrganisations.Count);
			part.RelatedOrganisations.AddOrganisationIfNotExist(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Owner);
			AssertEquals("Duplicate relationship added again, should be 2", 2, part.RelatedOrganisations.Count);

			part.RelatedOrganisations.DeleteOrganisationIfExists(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Supplier);
			AssertEquals("Non-existant relationship deleted", 2, part.RelatedOrganisations.Count);
			part.RelatedOrganisations.DeleteOrganisationIfExists(orgHeader2.PK, OrgPartRelation.RelationshipTypes.Owner);
			AssertEquals("Non-existant relationship deleted", 2, part.RelatedOrganisations.Count);

			part.RelatedOrganisations.DeleteOrganisationIfExists(orgHeader1.PK, OrgPartRelation.RelationshipTypes.Owner);
			AssertEquals("Relationship deleted, should be 1", 1, part.RelatedOrganisations.Count);
			part.RelatedOrganisations.DeleteOrganisationIfExists(orgHeader2.PK, OrgPartRelation.RelationshipTypes.Supplier);
			AssertEquals("Relationship deleted, should be 0", 0, part.RelatedOrganisations.Count);
		}

		//public void TestContainsRelationType()
		//{
		//  BusinessObjectFactory Factory = new BusinessObjectFactory();
		//  OrgSupplierPart Part = OrgSupplierPart.New(Factory);

		//  OrgHeader Org1 = Factory.New<OrgHeader>();
		//  OrgHeader Org2 = Factory.New<OrgHeader>();
		//  Part.RelatedOrganisations.AddOrganisationIfNotExist(Org1.PK, OrgPartRelation.RelationshipTypes.Owner);
		//  Part.RelatedOrganisations.AddOrganisationIfNotExist(Org2.PK, OrgPartRelation.RelationshipTypes.Owner);
		//  AssertEquals(true, Part.RelatedOrganisations.ContainsRelationType(OrgPartRelation.RelationshipTypes.Owner));
		//  AssertEquals(false, Part.RelatedOrganisations.ContainsRelationType(OrgPartRelation.RelationshipTypes.Supplier));

		//  Part.RelatedOrganisations.AddOrganisationIfNotExist(Org2.PK, OrgPartRelation.RelationshipTypes.Supplier);
		//  AssertEquals(true, Part.RelatedOrganisations.ContainsRelationType(OrgPartRelation.RelationshipTypes.Supplier));
		//}
	}
}
