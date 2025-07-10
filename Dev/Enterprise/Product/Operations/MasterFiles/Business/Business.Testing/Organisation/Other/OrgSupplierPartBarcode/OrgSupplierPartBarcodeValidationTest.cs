using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	class OrgSupplierPartBarcodeValidationTest : BusinessObjectValidationTestCase
	{
		#region TestCheckPH_Barcode

		public void TestCheckPH_Barcode()
		{
			PartBarcode.PH_Barcode = ZString.Empty;
			AssertHasError(PartBarcode.PH_BarcodeInfo, "Please enter a " + PartBarcode.PH_BarcodeInfo.Description + ".");
			AssertNoWarnings(PartBarcode.PH_BarcodeInfo);

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "TEST PRODUCT";
			part2.PartBarcodes.AddNew();
			part2.PartBarcodes[0].PH_Barcode = "TEST";

			PartBarcode.PH_Barcode = "TEST";
			AssertNoErrors(PartBarcode.PH_BarcodeInfo);
			AssertNoWarnings(PartBarcode.PH_BarcodeInfo);

			PartBarcode.PH_Barcode = "NEW TEST";
			AssertNoErrors(PartBarcode.PH_BarcodeInfo);
			AssertNoWarnings(PartBarcode.PH_BarcodeInfo);

			PartBarcode.PH_Barcode = "   NEW TEST";
			AssertHasError(PartBarcode.PH_BarcodeInfo, OrgSupplierPartBarcodeValidation.ValueHasToBeTrimmed);
			AssertNoWarnings(PartBarcode.PH_BarcodeInfo);

			PartBarcode.PH_Barcode = "					              ";
			AssertHasError(PartBarcode.PH_BarcodeInfo, "Please enter a " + PartBarcode.PH_BarcodeInfo.Description + ".");
			AssertNoWarnings(PartBarcode.PH_BarcodeInfo);
		}

		#endregion

		#region TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner

		public void TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client0 = helper.CreateClient("ABC0");
			var client1 = helper.CreateClient("ABC1");
			var client2 = helper.CreateClient("ABC2");

			var part0 = (OrgSupplierPart)helper.CreateProduct(client0, "PRODUCT0");
			var barcode0 = part0.PartBarcodes.AddNew();
			barcode0.PH_Barcode = "TestCode";
			barcode0.PH_F3_NKPackType = "TCE";

			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var relatedOrg1 = part1.RelatedOrganisations.AddNew();
			relatedOrg1.OU_OH = client2;
			relatedOrg1.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TCE";
			Factory.Save();

			var part2 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT2");
			var relatedOrg2 = part2.RelatedOrganisations.AddNew();
			relatedOrg2.OU_OH = client0;
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "TCE";
			AssertHasError(barcode2.PH_BarcodeInfo, "Barcode 'TestCode' is the same as the Product Code or is already being used by Product 'PRODUCT0, PRODUCT1' from the same owner.");

			var part3 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT3");
			part3.RelatedOrganisations.RemoveAndDeleteAll();
			var relatedOrg3 = part3.RelatedOrganisations.AddNew();
			relatedOrg3.OU_OH = client1;
			relatedOrg3.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var barcode3 = part3.PartBarcodes.AddNew();
			barcode3.PH_Barcode = "TestCode";
			barcode3.PH_F3_NKPackType = "TCE";
			AssertHasError(barcode3.PH_BarcodeInfo, "Barcode 'TestCode' is the same as the Product Code or is already being used by Product 'PRODUCT1, PRODUCT2' from the same owner.");

			var part4 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT4");
			part4.RelatedOrganisations.RemoveAndDeleteAll();
			var relatedOrg4 = part4.RelatedOrganisations.AddNew();
			relatedOrg4.OU_OH = client1;
			relatedOrg4.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			var barcode4 = part4.PartBarcodes.AddNew();
			barcode4.PH_Barcode = "TestCode";
			barcode4.PH_F3_NKPackType = "TCE";
			AssertHasError(barcode4.PH_BarcodeInfo, "Barcode 'TestCode' is the same as the Product Code or is already being used by Product 'PRODUCT1, PRODUCT2, PRODUCT3' from the same owner.");

			var part5 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT5");
			part5.RelatedOrganisations.RemoveAndDeleteAll();
			var relatedOrg5 = part5.RelatedOrganisations.AddNew();
			relatedOrg5.OU_OH = client1;
			relatedOrg5.OU_Relationship = OrgPartRelation.RelationshipTypes.Both;
			var barcode5 = part5.PartBarcodes.AddNew();
			barcode5.PH_Barcode = "TestCode";
			barcode5.PH_F3_NKPackType = "TCE";
			AssertHasError(barcode5.PH_BarcodeInfo, "Barcode 'TestCode' is the same as the Product Code or is already being used by Product 'PRODUCT1, PRODUCT2, PRODUCT3, PRODUCT4' from the same owner.");

			part1.OP_IsActive = false;
			barcode5.RunPreSaveValidation();
			AssertHasError(barcode5.PH_BarcodeInfo, "Barcode 'TestCode' is the same as the Product Code or is already being used by Product 'PRODUCT2, PRODUCT3, PRODUCT4' from the same owner.");
		}

		#endregion

		#region TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_Active

		public void TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_Product1IsActiveAndProduct2IsActive()
		{
			TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_ActiveCore(true, true, expectedHasError: true);
		}

		public void TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_Product1IsInactiveAndProduct2IsActive()
		{
			TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_ActiveCore(false, true);
		}

		public void TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_Product1IsActiveAndProduct2IsAInactive()
		{
			TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_ActiveCore(true, false);
		}

		public void TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_Product1IsInactiveAndProduct2IsInactive()
		{
			TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_ActiveCore(false, false);
		}

		void TestCheckPH_Barcode_SameBarcodeOfProductWithSameOwner_ActiveCore(bool active1, bool active2, bool expectedHasError = false)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");

			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			part1.OP_IsActive = active1;
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			part2.OP_IsActive = active2;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode1";

			AssertNoErrors(barcode2.PH_BarcodeInfo);

			barcode2.PH_Barcode = "TestCode";
			if (expectedHasError)
			{
				AssertHasError(barcode2.PH_BarcodeInfo, "Barcode 'TestCode' is the same as the Product Code or is already being used by Product 'PRODUCT1' from the same owner.");
			}
			else
			{
				AssertNoErrors("One of products is in-active, should not has error", barcode2.PH_BarcodeInfo);
			}
		}

		#endregion

		#region TestCheckPH_Barcode_SameBarcodeOfProductWithDifferentOwner

		public void TestCheckPH_Barcode_SameBarcodeOfProductWithDifferentOwner()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC1");
			var client2 = helper.CreateClient("ABC2");

			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TCE";
			Factory.Save();

			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "TCE";

			AssertNoErrors(barcode1.PH_BarcodeInfo);
			AssertNoErrors(barcode2.PH_BarcodeInfo);
		}

		#endregion

		#region TestCheckPH_Barcode_SameBarcodeOfProductWithoutOwner

		public void TestCheckPH_Barcode_SameBarcodeOfProductWithoutOwner()
		{
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PRODUCT1";
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "BarCode";
			barcode1.PH_F3_NKPackType = "BAR";
			Factory.Save();

			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PRODUCT2";
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "BarCode";
			barcode2.PH_F3_NKPackType = "BAR";

			AssertNoErrors(barcode1.PH_BarcodeInfo);
			AssertNoErrors(barcode2.PH_BarcodeInfo);
		}

		#endregion

		#region TestCheckPH_Barcode_TwoSameBarcodes

		public void TestCheckPH_Barcode_TwoSameBarcodes_ProductIsActive()
		{
			TestCheckPH_Barcode_TwoSameBarcodesCore(true, true);
		}

		public void TestCheckPH_Barcode_TwoSameBarcodes_ProductIsInactive()
		{
			TestCheckPH_Barcode_TwoSameBarcodesCore(false, false);
		}

		void TestCheckPH_Barcode_TwoSameBarcodesCore(bool isActive, bool expectedHasError)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			var barcode1 = part.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "BAR";
			part.OP_IsActive = isActive;
			Factory.Save();

			var barcode2 = part.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "BAR";

			if (expectedHasError)
			{
				AssertHasError(barcode2.PH_BarcodeInfo, "Barcode 'TestCode' has already existed in this product.");
			}
			else
			{
				AssertNoErrors(barcode2.PH_BarcodeInfo);
			}
		}

		#endregion

		#region TestCheckPH_UseForDocument

		public void TestCheckPH_UseForDocuments_PackTypeIsNotStockUnitFlagCanNotBeSet()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "KG";
			var barcode = part.PartBarcodes.AddNew();
			barcode.PH_Barcode = "TestCode";
			barcode.PH_F3_NKPackType = "BAR";
			barcode.PH_UseForDocuments = true;

			AssertHasError(barcode.PH_UseForDocumentsInfo, OrgSupplierPartBarcodeValidation.PackTypeIsNotStockUnitFlagCanNotBeSet);

			barcode.PH_UseForDocuments = false;

			AssertNoErrors(barcode.PH_UseForDocumentsInfo);
		}

		public void TestCheckPH_UseForDocuments_PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOne()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "KG";
			var barcode = part.PartBarcodes.AddNew();
			barcode.PH_Barcode = "TestCode";
			barcode.PH_F3_NKPackType = "KG";
			barcode.PH_UseForDocuments = false;

			AssertHasError(barcode.PH_UseForDocumentsInfo, OrgSupplierPartBarcodeValidation.PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOneAndOnlyOne);

			barcode.PH_UseForDocuments = true;

			AssertNoErrors(barcode.PH_UseForDocumentsInfo);
		}

		public void TestCheckPH_UseForDocuments_PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOneAndOnlyOne()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "KG";
			var barcode1 = part.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode1";
			barcode1.PH_F3_NKPackType = "KG";
			barcode1.PH_UseForDocuments = true;

			Factory.Save();

			var barcode2 = part.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";
			barcode2.PH_F3_NKPackType = "KG";
			barcode2.PH_UseForDocuments = true;

			AssertHasError(barcode2.PH_UseForDocumentsInfo, OrgSupplierPartBarcodeValidation.PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOneAndOnlyOne);

			barcode2.PH_UseForDocuments = false;

			AssertNoErrors(barcode2.PH_UseForDocumentsInfo);
		}

		#endregion

		// already have test for empty and duplicate barcodes for one product

		#region TestCheckPH_Barcode_UniquePerProduct

		public void TestCheckPH_Barcode_UniquePerProduct()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("CL1");

			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "B001";
			barcode1.PH_F3_NKPackType = "UNT";

			AssertNoErrors(barcode1.PH_BarcodeInfo);

			var barcode2 = part1.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "B001";
			barcode2.PH_F3_NKPackType = "UNT";

			AssertHasError(barcode2.PH_BarcodeInfo, "Barcode 'B001' has already existed in this product.");

			var client2 = helper.CreateClient("CL2");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			var barcode3 = part2.PartBarcodes.AddNew();
			barcode3.PH_Barcode = "B001";
			barcode3.PH_F3_NKPackType = "UNT";

			AssertNoErrors(barcode3.PH_BarcodeInfo);

			barcode2.PH_Barcode = "B002";
			AssertNoErrors(barcode2.PH_BarcodeInfo);
		}

		#endregion

		#region TestCheckPH_Barcode_TwoBarcodesOfOneProductExchanged

		public void TestCheckPH_Barcode_TwoBarcodesOfOneProductExchanged()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			var barcode1 = part.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "BarCode1";
			barcode1.PH_F3_NKPackType = "BA1";
			var barcode2 = part.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "BarCode2";
			barcode2.PH_F3_NKPackType = "BA2";
			Factory.Save();

			barcode1.PH_Barcode = "BarCode2";
			barcode2.PH_Barcode = "BarCode1";
			barcode1.RunPreSaveValidation();
			barcode2.RunPreSaveValidation();

			AssertNoErrors(barcode1.PH_BarcodeInfo);
			AssertNoErrors(barcode2.PH_BarcodeInfo);

			AssertNoExceptionThrown("Barcodes exchanged and saved successfully.", Factory.Save);
		}

		#endregion

		#region TestCheckPH_Barcode_ValidateForTwoEmptyBarcodes_OneProduct

		public void TestCheckPH_Barcode_ValidateForTwoEmptyBarcodes_OneProduct()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");

			var barcode1 = part.PartBarcodes.AddNew();
			var barcode2 = part.PartBarcodes.AddNew();
			Assert("Precondition: Barcode1 is empty.", barcode1.PH_Barcode.IsEmpty);
			Assert("Precondition: Barcode2 is empty.", barcode2.PH_Barcode.IsEmpty);

			barcode2.RunPreSaveValidation();
			AssertEquals("Only has 1 error.", 1, barcode2.PH_BarcodeInfo.GetErrors().Count());
			AssertHasError("Barcode is not enterd.", barcode2.PH_BarcodeInfo, "Please enter a Barcode.");
		}

		#endregion

		#region TestCheckPH_Barcode_ValidateForTwoEmptyBarcodes_TwoProducts

		public void TestCheckPH_Barcode_ValidateForTwoEmptyBarcodes_TwoProducts()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");

			var barcode1 = part1.PartBarcodes.AddNew();
			var barcode2 = part2.PartBarcodes.AddNew();
			Assert("Precondition: Barcode1 is empty.", barcode1.PH_Barcode.IsEmpty);
			Assert("Precondition: Barcode2 is empty.", barcode2.PH_Barcode.IsEmpty);

			barcode2.RunPreSaveValidation();
			AssertEquals("Only has 1 error.", 1, barcode2.PH_BarcodeInfo.GetErrors().Count());
			AssertHasError("Barcode is not enterd.", barcode2.PH_BarcodeInfo, "Please enter a Barcode.");
		}

		#endregion

		#region TestCheckPH_Barcode_BarcodeEqualToProductcode

		public void TestCheckPH_Barcode_BarcodeEqualToProductcode_ProductIsActive()
		{
			TestCheckPH_Barcode_BarcodeEqualToProductcodeCore(true, true);
		}

		public void TestCheckPH_Barcode_BarcodeEqualToProductcode_ProductIsInactive()
		{
			TestCheckPH_Barcode_BarcodeEqualToProductcodeCore(false, false);
		}

		void TestCheckPH_Barcode_BarcodeEqualToProductcodeCore(bool active, bool expectedHasError)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			part.OP_IsActive = active;
			var barcode = part.PartBarcodes.AddNew();

			AssertNoErrors(barcode.PH_BarcodeInfo);

			barcode.PH_Barcode = "PRODUCT";
			if (expectedHasError)
			{
				AssertHasError(barcode.PH_BarcodeInfo, "Barcode cannot be the same as the Product Code.");
			}
			else
			{
				AssertNoErrors("Product is in-active, should not have error", barcode.PH_BarcodeInfo);
			}
		}

		#endregion

		#region TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProducts

		public void TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProducts_Product1IsActiveAndProduct2IsActive()
		{
			TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProductsCore(true, true, expectedHasError: true);
		}

		public void TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProducts_Product1IsInactiveAndProduct2IsActive()
		{
			TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProductsCore(false, true);
		}

		public void TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProducts_Product1IsActiveAndProduct2IsInactive()
		{
			TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProductsCore(true, false);
		}

		public void TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProducts_Product1IsInactiveAndProduct2IsInactive()
		{
			TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProductsCore(false, false);
		}

		public void TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProductsCore(bool active1, bool active2, bool expectedHasError = false)
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			part1.OP_IsActive = active1;
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			part2.OP_IsActive = active2;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";

			AssertNoErrors(barcode2.PH_BarcodeInfo);

			barcode2.PH_Barcode = "PRODUCT";
			if (expectedHasError)
			{
				AssertHasError(barcode2.PH_BarcodeInfo, "Barcode 'PRODUCT' is the same as the Product Code or is already being used by Product 'PRODUCT' from the same owner.");
			}
			else
			{
				AssertNoErrors("One of Products is in-active, should not have error", barcode2.PH_BarcodeInfo);
			}
		}

		#endregion

		#region TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProducts_RelationshipIsSupplier

		public void TestCheckPH_Barcode_BarcodeEqualToProductcode_TwoProducts_RelationshipIsSupplier()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			var relatedOrg2 = (OrgPartRelation)part2.RelatedOrganisations.Single();
			relatedOrg2.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";

			AssertNoErrors(barcode2.PH_BarcodeInfo);
			barcode2.PH_Barcode = "PRODUCT";
			AssertNoErrors(barcode2.PH_BarcodeInfo);
		}

		#endregion

		#region TestCheckPH_Barcode_BarcodeEqualToProductcode_DifferentOwners

		public void TestCheckPH_Barcode_BarcodeEqualToProductcode_DifferentOwners()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";

			var client2 = helper.CreateClient("ABC2");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "TestCode");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";

			AssertNoErrors(barcode2.PH_BarcodeInfo);

			barcode2.PH_Barcode = "PRODUCT";
			AssertNoErrors(barcode2.PH_BarcodeInfo);
		}

		#endregion

		#region TestCheckPH_F3_NKPackType

		public void TestCheckPH_F3_NKPackType()
		{
			SetupUnitConversions();
			PartBarcode.PH_F3_NKPackType = ZString.Empty;
			AssertHasError(PartBarcode.PH_F3_NKPackTypeInfo, "Please enter a " + PartBarcode.PH_F3_NKPackTypeInfo.Description + ".");
			AssertNoError(PartBarcode.PH_F3_NKPackTypeInfo, "Enter a valid " + PartBarcode.PH_F3_NKPackTypeInfo.Description + ".");
			AssertNoWarnings(PartBarcode.PH_F3_NKPackTypeInfo);

			var invalidCode = "XXX";
			AssertEquals("Product UQ List shoud not contain XXX", false, PartBarcode.Lookups.ProductUQList.ContainsCode(invalidCode));
			PartBarcode.PH_F3_NKPackType = invalidCode;
			AssertHasError(PartBarcode.PH_F3_NKPackTypeInfo, "Enter a valid " + PartBarcode.PH_F3_NKPackTypeInfo.Description + ".");
			AssertHasError(PartBarcode.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.NoUnitConversionWarningMessagePrefix + "XXX");

			PartBarcode.PH_F3_NKPackType = "UNT";
			AssertNoErrors(PartBarcode.PH_F3_NKPackTypeInfo);
			AssertNoWarnings(PartBarcode.PH_F3_NKPackTypeInfo);

			PartBarcode.PH_F3_NKPackType = "BAG";
			AssertHasError(PartBarcode.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.PackTypeIsNotStockUnitFlagCanNotBeSet);
			AssertNoWarnings(PartBarcode.PH_F3_NKPackTypeInfo);

			PartBarcode.PH_F3_NKPackType = "PLT";
			AssertHasError(PartBarcode.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.PackTypeIsNotStockUnitFlagCanNotBeSet);
			AssertNoWarnings(PartBarcode.PH_F3_NKPackTypeInfo);

			PartBarcode.PH_F3_NKPackType = "CAS";
			AssertNoWarnings(PartBarcode.PH_F3_NKPackTypeInfo);
			AssertHasError(PartBarcode.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.NoUnitConversionWarningMessagePrefix + "CAS");

			var product = Factory.New<OrgSupplierPart>();
			var barcode1 = product.PartBarcodes.AddNew();
			barcode1.PH_F3_NKPackType = "UNT";

			var barcode2 = product.PartBarcodes.AddNew();
			AssertNoErrors("Precondition", barcode2.PH_F3_NKPackTypeInfo);
			AssertNoWarnings("Precondition", barcode2.PH_F3_NKPackTypeInfo);

			barcode2.PH_F3_NKPackType = "UNT";
			AssertNoWarnings(barcode2.PH_F3_NKPackTypeInfo);

			barcode2.PH_F3_NKPackType = "PLT";
			AssertNoWarnings(barcode2.PH_F3_NKPackTypeInfo);
			AssertHasError(barcode2.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.NoUnitConversionWarningMessagePrefix + "PLT");

			PartBarcode.PH_F3_NKPackType = "   ";
			AssertHasError(PartBarcode.PH_F3_NKPackTypeInfo, "Please enter a " + PartBarcode.PH_F3_NKPackTypeInfo.Description + ".");
			AssertNoError(PartBarcode.PH_F3_NKPackTypeInfo, "Enter a valid " + PartBarcode.PH_F3_NKPackTypeInfo.Description + ".");
			AssertNoWarnings(PartBarcode.PH_F3_NKPackTypeInfo);
		}

		public void TestCheckPH_F3_NKPackType_PackTypeIsNotStockUnitFlagCanNotBeSet()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "KG";
			var barcode1 = part.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode1";
			barcode1.PH_F3_NKPackType = "KG";

			Factory.Save();

			barcode1.PH_F3_NKPackType = "UNT";
			AssertHasError("If PackType is no-SKU and the flag is true, should error.", barcode1.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.PackTypeIsNotStockUnitFlagCanNotBeSet);

			barcode1.PH_F3_NKPackType = "KG";
			AssertNoErrors("If PackType Changed to SKU and the flag is true, should remove error.", barcode1.PH_F3_NKPackTypeInfo);
		}

		public void TestCheckPH_F3_NKPackType_PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOne()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "KG";
			var barcode = part.PartBarcodes.AddNew();
			barcode.PH_Barcode = "TestCode";
			barcode.PH_F3_NKPackType = "KG";
			barcode.PH_UseForDocuments = false;

			AssertHasError(barcode.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOneAndOnlyOne);

			barcode.PH_UseForDocuments = true;

			AssertNoErrors(barcode.PH_F3_NKPackTypeInfo);
		}

		public void TestCheckPH_F3_NKPackType_PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOneAndOnlyOne()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT");
			part.OP_StockKeepingUnit = "KG";
			var barcode1 = part.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode1";
			barcode1.PH_F3_NKPackType = "KG";
			barcode1.PH_UseForDocuments = true;

			Factory.Save();

			var barcode2 = part.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";
			barcode2.PH_UseForDocuments = true;
			barcode2.PH_F3_NKPackType = "KG";

			AssertHasError(barcode2.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.PackTypeIsStockUnitExistsUseForDocumentsMustBeSetOnOneAndOnlyOne);

			barcode2.PH_UseForDocuments = false;

			AssertNoErrors(barcode2.PH_F3_NKPackTypeInfo);
		}

		#endregion

		#region TestCheckPH_F3_NKPackType_ForWeightsAndVolume

		public void TestCheckPH_F3_NKPackType_ForWeightsAndVolume()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "UNT";
			CreatePartUnit(part, "UNT", Constants.Weight.Kilograms, 1);
			CreatePartUnit(part, "UNT", Constants.Volume.CubicMetres, 2);
			CreatePartUnit(part, "UNT", "PLT", 3);

			var barcode = part.PartBarcodes.AddNew();
			barcode.PH_F3_NKPackType = Constants.Weight.Kilograms;
			AssertNoWarnings(barcode.PH_F3_NKPackTypeInfo);
			AssertHasError(barcode.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.NoWeightsOrVolumesSelectedUnlessStockKeepingUnitIsTheSame);

			barcode.PH_F3_NKPackType = Constants.Volume.CubicMetres;
			AssertNoWarnings(barcode.PH_F3_NKPackTypeInfo);
			AssertHasError(barcode.PH_F3_NKPackTypeInfo, OrgSupplierPartBarcodeValidation.NoWeightsOrVolumesSelectedUnlessStockKeepingUnitIsTheSame);

			barcode.PH_F3_NKPackType = "PLT";
			AssertNoWarnings(barcode.PH_F3_NKPackTypeInfo);
			AssertNoErrors(barcode.PH_F3_NKPackTypeInfo);

			part.OP_StockKeepingUnit = Constants.Weight.Kilograms;
			barcode.PH_F3_NKPackType = Constants.Weight.Kilograms;
			AssertNoWarnings(barcode.PH_F3_NKPackTypeInfo);
			AssertNoErrors(barcode.PH_F3_NKPackTypeInfo);

			part.OP_StockKeepingUnit = Constants.Volume.CubicMetres;
			barcode.PH_F3_NKPackType = Constants.Volume.CubicMetres;
			AssertNoWarnings(barcode.PH_F3_NKPackTypeInfo);
			AssertNoErrors(barcode.PH_F3_NKPackTypeInfo);
		}

		#endregion

		#region Implementation

		void SetupUnitConversions()
		{
			PartBarcode.SupplierPart.OP_StockKeepingUnit = "UNT";
			CreatePartUnit(PartBarcode.SupplierPart, "BAG", "UNT", 10);
			CreatePartUnit(PartBarcode.SupplierPart, "PLT", "BAG", 0.2m);
			CreatePartUnit(PartBarcode.SupplierPart, "CNT", "CAS", 5);
		}

		static OrgPartUnit CreatePartUnit(OrgSupplierPart part, string parentPackType, string childPackType, decimal childQuantityInParent)
		{
			var partUnit = part.PartUnits.AddNew();
			partUnit.OF_ParentPackType = parentPackType;
			partUnit.OF_PackType = childPackType;
			partUnit.OF_QuantityInParent = childQuantityInParent;
			return partUnit;
		}

		protected OrgSupplierPartBarcode PartBarcode
		{
			get { return fPartBarcode ?? (fPartBarcode = GetPartBarcode()); }
			set { fPartBarcode = value; }
		}

		protected virtual OrgSupplierPartBarcode GetPartBarcode()
		{
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			return part.PartBarcodes.AddNew();
		}

		OrgSupplierPartBarcode fPartBarcode;
		#endregion
	}
}
