using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgSecondaryPartBOMValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOSB_OP_SecondaryProduct()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mainProduct1 = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			var secondaryPart1 = mainProduct1.SecondaryParts.AddNew();
			AssertNoErrors("Precondition", secondaryPart1.OSB_OP_SecondaryProductInfo);

			secondaryPart1.Validation.ValidateOSB_OP_SecondaryProduct();
			AssertHasError(secondaryPart1.OSB_OP_SecondaryProductInfo, "Please enter a Part.");

			secondaryPart1.OSB_OP_SecondaryProduct = mainProduct1.PK;
			AssertHasError(secondaryPart1.OSB_OP_SecondaryProductInfo, "Cannot Select the Main Product as a Secondary Product for BOM.");

			var bomComponentProduct = Factory.New<OrgSupplierPart>();
			CreateProductBOM(mainProduct1, bomComponentProduct, 1m, Constants.PkgUnit.Bottle);

			var product1 = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			secondaryPart1.OSB_OP_SecondaryProduct = product1.PK;
			AssertNoErrors(secondaryPart1.OSB_OP_SecondaryProductInfo);

			var secondaryPart2 = mainProduct1.SecondaryParts.AddNew();
			secondaryPart2.OSB_OP_SecondaryProduct = product1.PK;
			AssertHasError(secondaryPart2.OSB_OP_SecondaryProductInfo, "Cannot Select the same Secondary Product twice.");

			var product2 = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			secondaryPart2.OSB_OP_SecondaryProduct = product2.PK;
			AssertNoErrors(secondaryPart2.OSB_OP_SecondaryProductInfo);

			var mainProduct2 = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			var bomComponentProduct2 = Factory.New<OrgSupplierPart>();
			CreateProductBOM(mainProduct2, bomComponentProduct2, 1m, Constants.PkgUnit.Bottle);

			var secondaryPartOnMainProduct2 = mainProduct2.SecondaryParts.AddNew();
			secondaryPartOnMainProduct2.OSB_OP_SecondaryProduct = product1.PK;
			AssertNoErrors(secondaryPartOnMainProduct2.OSB_OP_SecondaryProductInfo);

			secondaryPartOnMainProduct2.OSB_OP_SecondaryProduct = product2.PK;
			AssertNoErrors(secondaryPartOnMainProduct2.OSB_OP_SecondaryProductInfo);
		}

		public void TestCheckOSB_OP_SecondaryProduct_DifferentOwnerWIthMasterProduct()
		{
			var orgHeader1 = Factory.New<OrgHeader>();
			var mainProduct = GetProductWithOrgPartRelationship(orgHeader1, OrgPartRelation.RelationshipTypes.Owner);
			var bomComponentProduct = GetProductWithOrgPartRelationship(orgHeader1, OrgPartRelation.RelationshipTypes.Owner);
			CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Bottle);

			var orgHeader2 = Factory.New<OrgHeader>();
			var secondaryProduct = GetProductWithOrgPartRelationship(orgHeader2, OrgPartRelation.RelationshipTypes.Owner);

			var secondaryPart = mainProduct.SecondaryParts.AddNew();
			secondaryPart.OSB_OP_SecondaryProduct = secondaryProduct.PK;
			AssertHasError(secondaryPart.OSB_OP_SecondaryProductInfo, "Both the Main Product and Secondary Part should have at least one owner and one common owner.");
		}

		public void TestCheckOSB_OP_SecondaryProduct_MasterProductWithNoOwner()
		{
			var mainProduct = Factory.New<OrgSupplierPart>();
			var bomComponentProduct = Factory.New<OrgSupplierPart>();
			CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Bottle);

			var secondaryProduct = GetProductWithOrgPartRelationship(Factory.New<OrgHeader>(), OrgPartRelation.RelationshipTypes.Owner);
			var secondaryPart = mainProduct.SecondaryParts.AddNew();
			secondaryPart.OSB_OP_SecondaryProduct = secondaryProduct.PK;
			AssertHasError(secondaryPart.OSB_OP_SecondaryProductInfo, "Both the Main Product and Secondary Part should have at least one owner and one common owner.");
		}

		public void TestCheckOSB_OP_SecondaryProduct_SecondaryProductWithNoOwner()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mainProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);

			var bomComponentProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Bottle);

			var secondaryProduct = Factory.New<OrgSupplierPart>();
			var secondaryPart = mainProduct.SecondaryParts.AddNew();
			secondaryPart.OSB_OP_SecondaryProduct = secondaryProduct.PK;
			AssertHasError(secondaryPart.OSB_OP_SecondaryProductInfo, "Both the Main Product and Secondary Part should have at least one owner and one common owner.");
		}

		public void TestCheckOSB_OP_SecondaryProduct_ProductsWithSupplierOnly()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mainProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Supplier);

			var bomComponentProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Supplier);
			CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Bottle);

			var secondaryProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Supplier);
			var secondaryPart = mainProduct.SecondaryParts.AddNew();
			secondaryPart.OSB_OP_SecondaryProduct = secondaryProduct.PK;
			AssertHasError(secondaryPart.OSB_OP_SecondaryProductInfo, "Both the Main Product and Secondary Part should have at least one owner and one common owner.");
		}

		public void TestCheckOSB_OP_SecondaryProduct_PickWithoutWorkOrderMasterProduct()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mainProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;

			var bomComponentProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Bottle);

			var secondaryProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			var secondaryPart = mainProduct.SecondaryParts.AddNew();
			secondaryPart.OSB_OP_SecondaryProduct = secondaryProduct.PK;
			AssertHasError(secondaryPart.OSB_OP_SecondaryProductInfo, "Secondary part can not be added to this product, because the main product is set to 'Can Pick without Work Order'.");
		}

		public void TestCheckOSB_OP_SecondaryProduct_PickWithoutWorkOrderParentMasterProduct()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mainProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			mainProduct.OP_IsComponentPickedOnSalesOrder = true;

			var bomComponentProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Bottle);

			var secondaryProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			var secondaryPart = bomComponentProduct.SecondaryParts.AddNew();
			secondaryPart.OSB_OP_SecondaryProduct = secondaryProduct.PK;
			AssertHasError(secondaryPart.OSB_OP_SecondaryProductInfo, "Secondary part can not be added to this product, because the main product is set to 'Can Pick without Work Order'.");
		}

		public void TestCheckOSB_OP_SecondaryProduct_PickWithoutWorkOrder()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mainProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);

			var bomComponentProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			CreateProductBOM(mainProduct, bomComponentProduct, 1m, Constants.PkgUnit.Bottle);

			var secondaryProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			secondaryProduct.OP_IsComponentPickedOnSalesOrder = true;

			var secondaryPart = mainProduct.SecondaryParts.AddNew();
			secondaryPart.OSB_OP_SecondaryProduct = secondaryProduct.PK;
			AssertHasError(secondaryPart.OSB_OP_SecondaryProductInfo, "Secondary part can not be added to this product if it is set to 'Can Pick without Work Order'.");
		}

		public void TestCheckOSB_OP_SecondaryProduct_PickWithoutWorkOrderComponent()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mainProduct1 = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			mainProduct1.OP_IsComponentPickedOnSalesOrder = true;

			var bomComponentProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			CreateProductBOM(mainProduct1, bomComponentProduct, 1m, Constants.PkgUnit.Bottle);

			var mainProduct2 = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);

			var bomComponentProduct2 = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			CreateProductBOM(mainProduct2, bomComponentProduct2, 1m, Constants.PkgUnit.Bottle);

			var secondaryPart = mainProduct2.SecondaryParts.AddNew();
			secondaryPart.OSB_OP_SecondaryProduct = bomComponentProduct.PK;
			AssertHasError(secondaryPart.OSB_OP_SecondaryProductInfo, "Secondary part can not be added to this product if it is set to 'Can Pick without Work Order'.");
		}

		public void TestCheckOSB_OP_SecondaryProduct_NoBillOfMaterials()
		{
			var orgHeader = Factory.New<OrgHeader>();
			var mainProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);

			var secondaryProduct = GetProductWithOrgPartRelationship(orgHeader, OrgPartRelation.RelationshipTypes.Owner);
			secondaryProduct.OP_IsComponentPickedOnSalesOrder = true;

			var secondaryPart = mainProduct.SecondaryParts.AddNew();
			secondaryPart.OSB_OP_SecondaryProduct = secondaryProduct.PK;
			AssertHasError(secondaryPart.OSB_OP_SecondaryProductInfo, "You cannot add Secondary Parts if Main Product has no Bill of Materials.");
		}

		public void TestCheckOSB_ProductQuantity()
		{
			var part = Factory.New<OrgSupplierPart>();
			var secondaryPart = part.SecondaryParts.AddNew();
			AssertNoErrors("Precondition", secondaryPart.OSB_ProductQuantityInfo);

			secondaryPart.Validation.ValidateOSB_ProductQuantity();
			AssertHasError(secondaryPart.OSB_ProductQuantityInfo, "Quantity cannot be zero.");

			secondaryPart.OSB_ProductQuantity = -1;
			AssertHasError(secondaryPart.OSB_ProductQuantityInfo, "Quantity cannot be negative.");

			secondaryPart.OSB_ProductQuantity = 1;
			AssertNoErrors(secondaryPart.OSB_ProductQuantityInfo);
		}

		OrgPartBOM CreateProductBOM(OrgSupplierPart part, OrgSupplierPart subPart, ZDecimal componentQty, ZString componentPack)
		{
			var bom = Factory.New<OrgPartBOM>();
			bom.OE_OP_MainProduct = part.PK;
			bom.OE_OP_Component = subPart.PK;
			bom.OE_ComponentQty = componentQty;
			bom.OE_F3_NKPackType = componentPack;
			return bom;
		}

		OrgSupplierPart GetProductWithOrgPartRelationship(OrgHeader relatedOrg, string relationshipType)
		{
			var product = Factory.New<OrgSupplierPart>();
			var productOrgRelation = product.RelatedOrganisations.AddNew();
			productOrgRelation.OU_Relationship = relationshipType;
			productOrgRelation.OU_OH = relatedOrg.PK;

			return product;
		}
	}
}
