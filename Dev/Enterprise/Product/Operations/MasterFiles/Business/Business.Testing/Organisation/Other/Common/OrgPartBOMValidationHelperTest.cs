using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgPartBOMValidationHelperTest : TestCaseWithFactory
	{
		public void TestDoesProductHaveOwner_Owner()
		{
			TestDoesProductHaveOwnerCore(OrgPartRelation.RelationshipTypes.Owner, expectedDoesProductHaveOwner: true);
		}

		public void TestDoesProductHaveOwner_Supplier()
		{
			TestDoesProductHaveOwnerCore(OrgPartRelation.RelationshipTypes.Supplier, expectedDoesProductHaveOwner: false);
		}

		public void TestDoesProductHaveOwner_Both()
		{
			TestDoesProductHaveOwnerCore(OrgPartRelation.RelationshipTypes.Both, expectedDoesProductHaveOwner: true);
		}

		void TestDoesProductHaveOwnerCore(string relationshipType, bool expectedDoesProductHaveOwner)
		{
			var product = Factory.New<OrgSupplierPart>();
			var orgHeader = Factory.New<OrgHeader>();
			var productOrgRelation = product.RelatedOrganisations.AddNew();
			productOrgRelation.OU_Relationship = relationshipType;
			productOrgRelation.OU_OH = orgHeader.PK;

			AssertEquals(expectedDoesProductHaveOwner, OrgPartBOMValidationHelper.DoesProductHaveOwner(product));
		}

		public void TestDoesProductHaveOwner_NoRelationship()
		{
			AssertEquals(false, OrgPartBOMValidationHelper.DoesProductHaveOwner(Factory.New<OrgSupplierPart>()));
		}

		public void TestDoesProductHaveOwner_NullProduct()
		{
			AssertEquals(false, OrgPartBOMValidationHelper.DoesProductHaveOwner(null));
		}

		public void TestDoCommonOwnersExist()
		{
			var mainProduct = Factory.New<OrgSupplierPart>();
			var orgHeader = Factory.New<OrgHeader>();
			var mainProductOrgRelation = mainProduct.RelatedOrganisations.AddNew();
			mainProductOrgRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			mainProductOrgRelation.OU_OH = orgHeader.PK;

			var secondaryProduct = Factory.New<OrgSupplierPart>();
			var secondaryProductOrgRelation = secondaryProduct.RelatedOrganisations.AddNew();
			secondaryProductOrgRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			secondaryProductOrgRelation.OU_OH = orgHeader.PK;

			Assert(OrgPartBOMValidationHelper.DoCommonOwnersExist(mainProduct, secondaryProduct));
		}

		public void TestDoCommonOwnersExist_DifferentOwners()
		{
			var mainProduct = Factory.New<OrgSupplierPart>();
			var orgHeader1 = Factory.New<OrgHeader>();
			var mainProductOrgRelation = mainProduct.RelatedOrganisations.AddNew();
			mainProductOrgRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			mainProductOrgRelation.OU_OH = orgHeader1.PK;

			var secondaryProduct = Factory.New<OrgSupplierPart>();
			var orgHeader2 = Factory.New<OrgHeader>();
			var secondaryProductOrgRelation = secondaryProduct.RelatedOrganisations.AddNew();
			secondaryProductOrgRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			secondaryProductOrgRelation.OU_OH = orgHeader2.PK;

			AssertEquals(false, OrgPartBOMValidationHelper.DoCommonOwnersExist(mainProduct, secondaryProduct));
		}

		public void TestDoCommonOwnersExist_SupplierRelationship()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			var orgHeader = Factory.New<OrgHeader>();
			var mainProductOrgRelation = product1.RelatedOrganisations.AddNew();
			mainProductOrgRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			mainProductOrgRelation.OU_OH = orgHeader.PK;

			var product2 = Factory.New<OrgSupplierPart>();
			var secondaryProductOrgRelation = product2.RelatedOrganisations.AddNew();
			secondaryProductOrgRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			secondaryProductOrgRelation.OU_OH = orgHeader.PK;

			AssertEquals(false, OrgPartBOMValidationHelper.DoCommonOwnersExist(product1, product2));
			AssertEquals(false, OrgPartBOMValidationHelper.DoCommonOwnersExist(product2, product1));
		}

		public void TestDoCommonOwnersExist_NoRelationship()
		{
			var product1 = Factory.New<OrgSupplierPart>();
			var orgHeader = Factory.New<OrgHeader>();
			var mainProductOrgRelation = product1.RelatedOrganisations.AddNew();
			mainProductOrgRelation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			mainProductOrgRelation.OU_OH = orgHeader.PK;

			var product2 = Factory.New<OrgSupplierPart>();

			AssertEquals(false, OrgPartBOMValidationHelper.DoCommonOwnersExist(product1, product2));
			AssertEquals(false, OrgPartBOMValidationHelper.DoCommonOwnersExist(product2, product1));
		}

		public void TestDoCommonOwnersExist_NullProducts()
		{
			AssertEquals(false, OrgPartBOMValidationHelper.DoCommonOwnersExist(null, Factory.New<OrgSupplierPart>()));
			AssertEquals(false, OrgPartBOMValidationHelper.DoCommonOwnersExist(Factory.New<OrgSupplierPart>(), null));
		}

		public void TestIsProductHasAnyParentMainProductWithIsPickOnOrder()
		{
			var product = Factory.New<OrgSupplierPart>();
			product.OP_IsComponentPickedOnSalesOrder = true;
			AssertEquals(false, OrgPartBOMValidationHelper.IsProductComponentOnMainProductWithIsPickOnOrder(product));

			var bomComponentProduct = Factory.New<OrgSupplierPart>();
			var bom = Factory.New<OrgPartBOM>();
			bom.OE_OP_MainProduct = product.PK;
			bom.OE_OP_Component = bomComponentProduct.PK;
			bom.OE_ComponentQty = 1m;
			bom.OE_F3_NKPackType = Constants.PkgUnit.Bottle;
			AssertEquals(true, OrgPartBOMValidationHelper.IsProductComponentOnMainProductWithIsPickOnOrder(bomComponentProduct));
		}

		public void TestIsProductHasAnyParentMainProductWithIsPickOnOrder_NullProduct()
		{
			AssertEquals(false, OrgPartBOMValidationHelper.IsProductComponentOnMainProductWithIsPickOnOrder(null));
		}
	}
}
