using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSupplierPartBarcode))]
	public class OrgSupplierPartBarcodeTest : EnterpriseBusinessObjectTestCase
	{
		#region Test Cases

		public void TestFindBarcode()
		{
			OrgSupplierPart part1 = Factory.New<OrgSupplierPart>();
			OrgSupplierPart part2 = Factory.New<OrgSupplierPart>();
			OrgSupplierPart part3 = Factory.New<OrgSupplierPart>();
			OrgSupplierPart part4 = Factory.New<OrgSupplierPart>();
			OrgSupplierPart part5 = Factory.New<OrgSupplierPart>();

			CreatePartBarcode(part1, "TEST11");
			CreatePartBarcode(part1, "TEST12");
			CreatePartBarcode(part2, "TEST2");
			CreatePartBarcode(part3, "TEST3");
			CreatePartBarcode(part4, "TEST4");
			CreatePartBarcode(part5, "TEST5");

			AssertEquals("TEST11", OrgSupplierPartBarcode.FindBarcode("TEST11", Factory).PH_Barcode);
			AssertEquals(part1, OrgSupplierPartBarcode.FindBarcode("TEST11", Factory).SupplierPart);

			AssertEquals("TEST12", OrgSupplierPartBarcode.FindBarcode("TEST12", Factory).PH_Barcode);
			AssertEquals(part1, OrgSupplierPartBarcode.FindBarcode("TEST12", Factory).SupplierPart);

			AssertEquals("TEST2", OrgSupplierPartBarcode.FindBarcode("TEST2", Factory).PH_Barcode);
			AssertEquals(part2, OrgSupplierPartBarcode.FindBarcode("TEST2", Factory).SupplierPart);

			AssertEquals("TEST3", OrgSupplierPartBarcode.FindBarcode("TEST3", Factory).PH_Barcode);
			AssertEquals(part3, OrgSupplierPartBarcode.FindBarcode("TEST3", Factory).SupplierPart);

			AssertEquals("TEST4", OrgSupplierPartBarcode.FindBarcode("TEST4", Factory).PH_Barcode);
			AssertEquals(part4, OrgSupplierPartBarcode.FindBarcode("TEST4", Factory).SupplierPart);

			AssertEquals("TEST5", OrgSupplierPartBarcode.FindBarcode("TEST5", Factory).PH_Barcode);
			AssertEquals(part5, OrgSupplierPartBarcode.FindBarcode("TEST5", Factory).SupplierPart);
		}

		#endregion

		#region TestTriggers

		[ExpectNoExceptions]
		public void TestTG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TPT";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			Factory.Save();

			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "TPT";

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product with same owner have the same barcode.");
		}

		[ExpectNoExceptions]
		public void TestTG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique_AddNewBarcodes()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TPT";
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode";
			barcode2.PH_F3_NKPackType = "TPT";

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product with same owner have the same barcode.");
		}

		[ExpectNoExceptions]
		public void TestTG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique_AddNewBarcodesWithAnotherProductChanged()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "123";
			barcode1.PH_F3_NKPackType = "123";
			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "456";
			barcode2.PH_F3_NKPackType = "456";
			var part3 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT3");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newPart3 = newFactory.Load<OrgSupplierPart>(part3.PK);
			var newBarcode3 = newPart3.PartBarcodes.AddNew();
			newBarcode3.PH_Barcode = "123";
			newBarcode3.PH_F3_NKPackType = "123";
			var newBarcode2 = newFactory.Load<OrgSupplierPartBarcode>(barcode2.PK);
			newBarcode2.PH_Barcode = "789";
			newBarcode2.PH_F3_NKPackType = "789";

			NUnit.Framework.Assert.That(newFactory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product with same barcode have the same owner.");
		}

		public void TestTG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique_ExchangeTwoBarcodes()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barode1 = part.PartBarcodes.AddNew();
			barode1.PH_Barcode = "TestCode1";
			barode1.PH_F3_NKPackType = "TC1";
			var barode2 = part.PartBarcodes.AddNew();
			barode2.PH_Barcode = "TestCode2";
			barode2.PH_F3_NKPackType = "TC2";
			Factory.Save();

			barode1.PH_Barcode = "TestCode2";
			barode2.PH_Barcode = "TestCode1";
			AssertNoExceptionThrown("Throw no excetion when exchanging barcodes.", Factory.Save);
		}

		[ExpectNoExceptions]
		public void TestTG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "PRODUCT1";
			barcode1.PH_F3_NKPackType = "PR1";

			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product code same with barcode.");
		}

		[ExpectNoExceptions]
		public void TestTG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_AnotherProduct()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode";
			barcode1.PH_F3_NKPackType = "TC1";

			var part2 = (OrgSupplierPart)helper.CreateProduct(client, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";
			barcode2.PH_F3_NKPackType = "TC2";

			Factory.Save();

			barcode2.PH_Barcode = "PRODUCT1";
			NUnit.Framework.Assert.That(Factory.Save, CustomConstraints.InnermostExceptionThrown(typeof(SqlException), OrgSupplierPart.OwnerAndBarcodeOfProductAreUnique), "Throw exception when product code same with barcode of another has same relationship.");
		}

		public void TestTG_OrgSupplierPartBarcode_EnsureBarcodeOfProductWithSameOwnerIsUnique_ProductCodeEqualToBarcode_DifferentRelationship()
		{
			var helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory);
			var client1 = helper.CreateClient("ABC");
			var part1 = (OrgSupplierPart)helper.CreateProduct(client1, "PRODUCT1");
			var barcode1 = part1.PartBarcodes.AddNew();
			barcode1.PH_Barcode = "TestCode1";
			barcode1.PH_F3_NKPackType = "TC1";

			var client2 = helper.CreateClient("DEF");
			var part2 = (OrgSupplierPart)helper.CreateProduct(client2, "PRODUCT2");
			var barcode2 = part2.PartBarcodes.AddNew();
			barcode2.PH_Barcode = "TestCode2";
			barcode2.PH_F3_NKPackType = "TC2";

			Factory.Save();

			barcode2.PH_Barcode = "PRODUCT1";

			AssertNoExceptionThrown("Throw no excetion when changing barcode as the product in another relationship.", Factory.Save);
		}

		public void Test_IsStockUintAndNoExistedStockUintBarcode_PH_UseForDocumentsShouldBeTrue()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "KG";
			var barcode = CreatePartBarcode(part, "TEST1");
			barcode.PH_F3_NKPackType = part.OP_StockKeepingUnit;

			Assert(barcode.PH_UseForDocuments);
		}

		public void Test_IsStockUintAndExistedStockUintBarcode_PH_UseForDocumentsShouldBeFalse()
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_StockKeepingUnit = "KG";
			var barcode1 = CreatePartBarcode(part, "TEST1");
			var barcode2 = CreatePartBarcode(part, "TEST2");
			barcode1.PH_F3_NKPackType = part.OP_StockKeepingUnit;
			barcode2.PH_F3_NKPackType = part.OP_StockKeepingUnit;

			Assert(barcode1.PH_UseForDocuments);
			Assert(!barcode2.PH_UseForDocuments);
		}

		#endregion

		#region DeferrableTriggers_OrgSupplierPartBarcodeTest class

		[TestedType(typeof(OrgSupplierPartBarcode))]
		class DeferrableTriggers_OrgSupplierPartBarcodeTest : DeferrableTriggerTestCase<OrgSupplierPartBarcode>
		{
			// Tested in OrgSupplierPartBarcodeTest
		}

		#endregion

		#region Implementation

		OrgSupplierPartBarcode CreatePartBarcode(OrgSupplierPart part, ZString barcode)
		{
			OrgSupplierPartBarcode partBarcode = part.PartBarcodes.AddNew();
			partBarcode.PH_Barcode = barcode;
			return partBarcode;
		}

		#endregion
	}
}
