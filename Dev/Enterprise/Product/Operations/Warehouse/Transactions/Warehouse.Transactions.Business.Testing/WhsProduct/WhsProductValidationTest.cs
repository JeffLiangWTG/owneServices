using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class WhsProductValidationTest : WhsBusinessObjectValidationTestCase
	{
		#region TestProductStylePK

		public void TestProductStylePK()
		{
			var part = Factory.New<OrgSupplierPart>();
			var product = WhsProduct.GetWhsProduct(part);
			AssertNoErrors("Precondition", product.ProductStylePKInfo);

			product.ProductStylePK = ZGuid.Empty;
			AssertNoErrors(product.ProductStylePKInfo);

			product.ProductStylePK = ZGuid.Invalid;
			AssertHasError(product.ProductStylePKInfo, "Enter a valid selection.");
		}

		public void TestProductStylePK_NoMoreThanOneOwnerForProduct()
		{
			var org1 = Helper.CreateClient("O1");
			var org2 = Helper.CreateClient("O2");

			var styleForOrg1 = Helper.CreateProductStyle("Shoe", "Shoe Style1", org1.PK);
			styleForOrg1.Colours.AddNew();
			styleForOrg1.Sizes.AddNew();

			var styleForOrg2 = Helper.CreateProductStyle("Shirt", "Shirt Style1", org2.PK);
			styleForOrg2.Colours.AddNew();
			styleForOrg2.Sizes.AddNew();

			var partWithOneOwner = Helper.CreateProduct(org1, "P1", OrgPartRelation.RelationshipTypes.Owner);
			var partWithOneSupplier = Helper.CreateProduct(org1, "P2", OrgPartRelation.RelationshipTypes.Supplier);
			var partWithOneBoth = Helper.CreateProduct(org1, "P3", OrgPartRelation.RelationshipTypes.Both);

			var partWithTwoOwners = Helper.CreateProduct(org1, "P4", OrgPartRelation.RelationshipTypes.Owner);
			Helper.CreateProductClientRelationShip(org2, partWithTwoOwners, OrgPartRelation.RelationshipTypes.Owner);

			var partWithOneOwnerAndSupplier = Helper.CreateProduct(org1, "P5", OrgPartRelation.RelationshipTypes.Owner);
			Helper.CreateProductClientRelationShip(org2, partWithOneOwnerAndSupplier, OrgPartRelation.RelationshipTypes.Supplier);

			var partWithTwoSuppliers = Helper.CreateProduct(org1, "P6", OrgPartRelation.RelationshipTypes.Supplier);
			Helper.CreateProductClientRelationShip(org2, partWithTwoSuppliers, OrgPartRelation.RelationshipTypes.Supplier);

			var partWithOneOwnerAndBoth = Helper.CreateProduct(org1, "P7", OrgPartRelation.RelationshipTypes.Owner);
			Helper.CreateProductClientRelationShip(org2, partWithOneOwnerAndBoth, OrgPartRelation.RelationshipTypes.Both);

			var partWithTwoBoths = Helper.CreateProduct(org1, "P8", OrgPartRelation.RelationshipTypes.Both);
			Helper.CreateProductClientRelationShip(org2, partWithTwoBoths, OrgPartRelation.RelationshipTypes.Both);

			var productWithOneOwner = WhsProduct.GetWhsProduct(partWithOneOwner);
			var productwithOneSupplier = WhsProduct.GetWhsProduct(partWithOneSupplier);
			var productWithBothRelationship = WhsProduct.GetWhsProduct(partWithOneBoth);
			var productWithTwoOwners = WhsProduct.GetWhsProduct(partWithTwoOwners);
			var productWithOneOwnerAndSupplier = WhsProduct.GetWhsProduct(partWithOneOwnerAndSupplier);
			var productWithTwoSuppliers = WhsProduct.GetWhsProduct(partWithTwoSuppliers);
			var productWithOneOwnerAndBoth = WhsProduct.GetWhsProduct(partWithOneOwnerAndBoth);
			var productWithTwoBothRelationships = WhsProduct.GetWhsProduct(partWithTwoBoths);

			AssertProductStylePKForOneAndOnlyOwner(productWithOneOwner, styleForOrg1, "Since Product has only one owner, no errors should be shown.");
			AssertProductStylePKForOneAndOnlyOwner(productWithOneOwner, styleForOrg2, "Since Product has only one owner, no errors should be shown.", "Product Style owner must be the owner of the product.");

			AssertProductStylePKForOneAndOnlyOwner(productwithOneSupplier, styleForOrg1, "Since product doesn't have an owner there should be an error.", "Product must have an owner to use Styles.");
			AssertProductStylePKForOneAndOnlyOwner(productwithOneSupplier, styleForOrg2, "Since product doesn't have an owner there should be an error.", "Product must have an owner to use Styles.");

			AssertProductStylePKForOneAndOnlyOwner(productWithBothRelationship, styleForOrg1, "Since Product has only one owner, no errors should be shown.");
			AssertProductStylePKForOneAndOnlyOwner(productWithBothRelationship, styleForOrg2, "Since Product has only one owner, no errors should be shown.", "Product Style owner must be the owner of the product.");

			AssertProductStylePKForOneAndOnlyOwner(productWithTwoOwners, styleForOrg1, "Since product has two owners product can't have a product style.", "Product must have only one Owner.");
			AssertProductStylePKForOneAndOnlyOwner(productWithTwoOwners, styleForOrg2, "Since product has two owners product can't have a product style.", "Product must have only one Owner.");

			AssertProductStylePKForOneAndOnlyOwner(productWithOneOwnerAndSupplier, styleForOrg1, "Since product has one owner there should not be an error.");
			AssertProductStylePKForOneAndOnlyOwner(productWithOneOwnerAndSupplier, styleForOrg2, "Since product has one owner there should not be an error.", "Product Style owner must be the owner of the product.");

			AssertProductStylePKForOneAndOnlyOwner(productWithTwoSuppliers, styleForOrg1, "Since product doesn't have an owner there should be an error.", "Product must have an owner to use Styles.");
			AssertProductStylePKForOneAndOnlyOwner(productWithTwoSuppliers, styleForOrg2, "Since product doesn't have an owner there should be an error.", "Product must have an owner to use Styles.");

			AssertProductStylePKForOneAndOnlyOwner(productWithOneOwnerAndBoth, styleForOrg1, "Since product has two owners product can't have a product style.", "Product must have only one Owner.");
			AssertProductStylePKForOneAndOnlyOwner(productWithOneOwnerAndBoth, styleForOrg2, "Since product has two owners product can't have a product style.", "Product must have only one Owner.");

			AssertProductStylePKForOneAndOnlyOwner(productWithTwoBothRelationships, styleForOrg1, "Since product has two owners product can't have a product style.", "Product must have only one Owner.");
			AssertProductStylePKForOneAndOnlyOwner(productWithTwoBothRelationships, styleForOrg2, "Since product has two owners product can't have a product style.", "Product must have only one Owner.");
		}

		static void AssertProductStylePKForOneAndOnlyOwner(WhsProduct product, WhsProductStyle style, string message, string expectedErrorMessage = "")
		{
			product.ProductStylePK = style.PK;
			if (string.IsNullOrEmpty(expectedErrorMessage))
			{
				AssertNoErrors(message, product.ProductStylePKInfo);
			}
			else
			{
				AssertHasError(message, product.ProductStylePKInfo, expectedErrorMessage);
			}
		}

		#endregion

		#region TestProductStyleColourPK

		public void TestProductStyleColourPK()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "P1";
			var product = WhsProduct.GetWhsProduct(part);
			AssertNoErrors("Precondition", product.ProductStyleColourPKInfo);

			product.ProductStylePK = ZGuid.Empty;
			product.Validation.ValidateProductStyleColourPK();
			AssertNoErrors(product.ProductStyleColourPKInfo);

			product.ProductStylePK = ZGuid.Invalid;
			product.Validation.ValidateProductStyleColourPK();
			AssertNoErrors(product.ProductStyleColourPKInfo);

			var style = Helper.CreateProductStyle("AAA", "Code", Helper.CreateClient().PK);
			var colour = style.Colours.AddNew();
			colour.WSC_Code = "RED";
			product.ProductStylePK = style.PK;
			product.Validation.ValidateProductStyleColourPK();
			AssertHasError(product.ProductStyleColourPKInfo, "Please enter a value.");

			var size = style.Sizes.AddNew();
			size.WSZ_Size = "1";
			size.WSZ_Sequence = 1;

			product.ProductStylePK = style.PK;
			product.ProductStyleColourPK = colour.PK;
			product.ProductStyleSizePK = size.PK;
			Factory.Save();

			var part2 = Factory.New<OrgSupplierPart>();
			var product2 = WhsProduct.GetWhsProduct(part2);
			product2.ProductStylePK = style.PK;
			product2.ProductStyleSizePK = size.PK;
			product2.ProductStyleColourPK = colour.PK;
			AssertHasError(product2.ProductStyleColourPKInfo, "The active Product 'P1' is already using the Style 'AAA', Color 'RED', Size '1' and Classification ''.");
		}

		#endregion

		#region TestProductStyleClassificationPK

		public void TestProductStyleClassificationPK()
		{
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "P1";
			var product1 = WhsProduct.GetWhsProduct(part1);
			AssertNoErrors("Precondition", product1.ProductStyleClassificationPKInfo);

			product1.ProductStylePK = ZGuid.Empty;
			product1.Validation.ValidateProductStyleClassificationPK();
			AssertNoErrors(product1.ProductStyleClassificationPKInfo);

			product1.ProductStylePK = ZGuid.Invalid;
			product1.Validation.ValidateProductStyleClassificationPK();
			AssertNoErrors(product1.ProductStyleClassificationPKInfo);

			var style1 = Helper.CreateProductStyle("AAA", "Code1", Helper.CreateClient().PK);
			var colour1 = style1.Colours.AddNew();
			colour1.WSC_Code = "RED";
			var size1 = style1.Sizes.AddNew();
			size1.WSZ_Size = "1";
			size1.WSZ_Sequence = 1;
			var classification1 = style1.Classifications.AddNew();
			classification1.WSS_Code = "M";
			classification1.WSS_Description = "Male";

			product1.ProductStylePK = style1.PK;
			product1.ProductStyleColourPK = colour1.PK;
			product1.ProductStyleSizePK = size1.PK;
			product1.Validation.ValidateProductStyleClassificationPK();
			AssertHasError(product1.ProductStyleClassificationPKInfo, "Please enter a value.");

			product1.ProductStyleClassificationPK = classification1.PK;
			AssertNoErrors(product1.ProductStyleClassificationPKInfo);
			Factory.Save();

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "P2";
			var product2 = WhsProduct.GetWhsProduct(part2);
			AssertNoErrors("Precondition", product2.ProductStyleClassificationPKInfo);

			var style2 = Helper.CreateProductStyle("BBB", "Code2", Helper.CreateClient().PK);
			var colour2 = style2.Colours.AddNew();
			colour2.WSC_Code = "RED";
			var size2 = style2.Sizes.AddNew();
			size2.WSZ_Size = "1";
			size2.WSZ_Sequence = 1;

			product2.ProductStylePK = style2.PK;
			product2.ProductStyleColourPK = colour2.PK;
			product2.ProductStyleSizePK = size2.PK;
			product2.Validation.ValidateProductStyleClassificationPK();
			AssertNoErrors(product2.ProductStyleClassificationPKInfo);

			var part3 = Factory.New<OrgSupplierPart>();
			var product3 = WhsProduct.GetWhsProduct(part3);
			product3.ProductStylePK = style1.PK;
			product3.ProductStyleSizePK = size1.PK;
			product3.ProductStyleColourPK = colour1.PK;
			product3.ProductStyleClassificationPK = classification1.PK;
			AssertHasError(product3.ProductStyleClassificationPKInfo, "The active Product 'P1' is already using the Style 'AAA', Color 'RED', Size '1' and Classification 'M'.");
		}

		#endregion

		#region TestProductStyleSizePK

		public void TestProductStyleSizePK()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "P1";
			var product = WhsProduct.GetWhsProduct(part);
			AssertNoErrors("Precondition", product.ProductStyleSizePKInfo);

			product.ProductStylePK = ZGuid.Empty;
			product.Validation.ValidateProductStyleSizePK();
			AssertNoErrors(product.ProductStyleSizePKInfo);

			product.ProductStylePK = ZGuid.Invalid;
			product.Validation.ValidateProductStyleSizePK();
			AssertNoErrors(product.ProductStyleSizePKInfo);

			var style = Helper.CreateProductStyle("AAA", "Code", Helper.CreateClient().PK);
			var size = style.Sizes.AddNew();
			size.WSZ_Size = "1";
			size.WSZ_Sequence = 1;
			product.ProductStylePK = style.PK;
			product.Validation.ValidateProductStyleSizePK();
			AssertHasError(product.ProductStyleSizePKInfo, "Please enter a value.");

			var colour = style.Colours.AddNew();
			colour.WSC_Code = "RED";
			product.ProductStylePK = style.PK;
			product.ProductStyleColourPK = colour.PK;
			product.ProductStyleSizePK = size.PK;
			Factory.Save();

			var part2 = Factory.New<OrgSupplierPart>();
			var product2 = WhsProduct.GetWhsProduct(part2);
			product2.ProductStylePK = style.PK;
			product2.ProductStyleColourPK = colour.PK;
			product2.ProductStyleSizePK = size.PK;
			AssertHasError(product2.ProductStyleSizePKInfo, "The active Product 'P1' is already using the Style 'AAA', Color 'RED', Size '1' and Classification ''.");
		}

		#endregion
	}
}
