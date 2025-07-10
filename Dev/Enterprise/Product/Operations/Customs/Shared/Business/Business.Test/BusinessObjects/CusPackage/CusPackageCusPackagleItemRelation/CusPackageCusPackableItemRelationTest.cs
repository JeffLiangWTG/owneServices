using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackageCusPackableItemRelation))]
	sealed class CusPackageCusPackableItemRelationTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSequence()
		{
			AssertEquals((ZShort)1, relation.Sequence);
		}

		public void TestPackage()
		{
			AssertEquals(cusPackage1.PK, relation.Package.PK);
		}

		public void TestIsPacked()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Unpacked initally", false, relation.IsPacked);
				cusPackableItem.CUI_PackableQty = 5m;
				cusPackage1.CustomsPackItem(cusPackableItem, 3m);
				AssertEquals("Packed 3", true, relation.IsPacked);

				cusPackage1.CustomsUnpackItem(cusPackableItem, 1m);
				AssertEquals("Unpacked 1", true, relation.IsPacked);

				cusPackage1.CustomsUnpackItem(cusPackableItem, 3m);
				AssertEquals("Unpacked 3", true, relation.IsPacked);

				cusPackage1.CustomsPackItem(cusPackableItem, 3m);
				cusPackage1.CustomsUnpackItem(cusPackableItem, 2m);
				AssertEquals("Packed 3 and Unpacked 2", false, relation.IsPacked);
			});
		}

		public void TestPackableQuantity()
		{
			cusPackableItem.CUI_PackableQty = 5m;
			AssertEquals(5m, relation.PackableQuantity);

			relation.PackableQuantity = 6m;
			AssertEquals(6m, cusPackableItem.CUI_PackableQty);
		}

		public void TestPackableUQ()
		{
			cusPackableItem.CUI_PackableUQ = "BAG";
			AssertEquals("BAG", relation.PackableUQ);

			relation.PackableUQ = "BBG";
			AssertEquals("BBG", cusPackableItem.CUI_PackableUQ);
		}

		public void TestNetWeight()
		{
			CombineAssertions(() =>
			{
				relation.NetWeight = 5m;
				var packageItemDivot = cusPackage1.GetDivot(cusPackableItem);
				AssertNull("Not packed", packageItemDivot);
				AssertEquals("Not packed Net weight", 0m, relation.NetWeight);

				cusPackableItem.CUI_PackableQty = 5m;
				cusPackage1.CustomsPackItem(cusPackableItem, 3m);
				packageItemDivot = cusPackage1.GetDivot(cusPackableItem);

				relation.NetWeight = 6m;
				AssertEquals("Relation Net Weight", 6m, relation.NetWeight);
				AssertEquals("Package Net Weight", 6m, packageItemDivot.PkgNetWeight);
			});
		}

		public void TestNetWeightUQ()
		{
			relation.NetWeightUQ = "KG";
			var packageItemDivot = cusPackage1.GetDivot(cusPackableItem);
			AssertNull("Not packed", packageItemDivot);
			AssertNullOrEmpty("Not Packed Net Weight UQ", relation.NetWeightUQ);

			cusPackableItem.CUI_PackableQty = 5m;
			cusPackage1.CustomsPackItem(cusPackableItem, 3m);
			packageItemDivot = cusPackage1.GetDivot(cusPackableItem);

			relation.NetWeightUQ = "DT";
			AssertEquals("Relation Net Weight UQ", "DT", relation.NetWeightUQ);
			AssertEquals("Package Net Weight UQ", "DT", packageItemDivot.PkgNetWeightUQ);
		}

		public void TestPackedQty()
		{
			cusPackableItem.CUI_PackableQty = 5;
			AssertEquals(0m, relation.PackedQty);

			cusPackage1.CustomsPackItem(cusPackableItem, 3);
			AssertEquals(3m, relation.PackedQty);

			relation.PackedQty = 4;
			AssertEquals(4m, relation.PackedQty);

			relation.PackedQty = 0;
			AssertEquals(0m, relation.PackedQty);
			Assert(!relation.IsPacked);
		}

		public void TestInvoiceLineNetWeight()
		{
			CombineAssertions(() =>
			{
				cusPackableItem.CUI_NetWeight = 10m;
				AssertEquals("InvoiceLineNetWeight", 10m, relation.InvoiceLineNetWeight);

				relation.InvoiceLineNetWeight = 20m;
				AssertEquals("CUI_NetWeight", 20m, cusPackableItem.CUI_NetWeight);
			});
		}

		public void TestInvoiceLineNetWeightUQ()
		{
			cusPackableItem.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;
			AssertEquals("KG", relation.InvoiceLineNetWeightUQ);
		}

		public void TestNotPackedQty()
		{
			cusPackableItem.CUI_PackableQty = 10;
			AssertEquals(0m, relation.TotalPackedQty);
			AssertEquals(10m, relation.NotPackedQty);

			cusPackage1.CustomsPackItem(cusPackableItem, 3);
			AssertEquals(7m, relation.NotPackedQty);

			cusPackage2.CustomsPackItem(cusPackableItem, 5);
			AssertEquals(2m, relation.NotPackedQty);
		}

		public void TestTotalPackedQty()
		{
			cusPackableItem.CUI_PackableQty = 10;
			AssertEquals(0m, relation.TotalPackedQty);
			AssertEquals(10m, relation.NotPackedQty);

			cusPackage1.CustomsPackItem(cusPackableItem, 3);
			AssertEquals(3m, relation.TotalPackedQty);

			cusPackage2.CustomsPackItem(cusPackableItem, 5);
			AssertEquals(8m, relation.TotalPackedQty);
		}

		public void TestTotalPackedNetWeight()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Nothing packed", 0m, relation.TotalPackedNetWeight);

				cusPackableItem.CUI_PackableQty = 10m;
				cusPackableItem.CUI_NetWeight = 50m;
				cusPackableItem.CUI_NetWeightUQ = Core.Constants.Weight.Kilograms;

				cusPackage1.CustomsPackItem(cusPackableItem, 3m);
				AssertEquals("Packed 3 items", 15m, relation.TotalPackedNetWeight);

				cusPackage2.CustomsPackItem(cusPackableItem, 5m);
				AssertEquals("Packed 5 more items", 40m, relation.TotalPackedNetWeight);
			});
		}

		public void TestInvoiceNumber()
		{
			invoice.JZ_InvoiceNumber = "121230";
			AssertEquals("121230", relation.InvoiceNumber);
		}

		public void TestInvoiceLineNumber()
		{
			AssertEquals((short)1, relation.InvoiceLineNumber);
		}

		public void TestGoodsDescription()
		{
			cusPackableItem.CUI_GoodsDescription = "goods desc";
			AssertEquals("goods desc", relation.GoodsDescription);

			relation.GoodsDescription = "goods desc2";
			AssertEquals("goods desc2", cusPackableItem.CUI_GoodsDescription);
		}

		public void TestLookups()
		{
			AssertType(typeof(CusPackageCusPackableItemRelationLookups), relation.Lookups);
		}

		public void TestIsPackedReadOnly()
		{
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceForTest = decl.Invoices.AddNew();
			var line1 = invoiceForTest.InvoiceLines.AddNew();
			line1.JI_Description = "ITEM 1";
			line1.JI_InvoiceQuantity = 1;
			line1.JI_InvoiceUQ = "PCE";
			var line2 = invoiceForTest.InvoiceLines.AddNew();
			line2.JI_Description = "ITEM 2";
			line2.JI_InvoiceQuantity = 2;
			line2.JI_InvoiceUQ = "PCE";

			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			var package1 = packingList.PackageJob.Packages.AddNew();
			var package2 = packingList.PackageJob.Packages.AddNew();
			var cusPackage1Relations = package1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			var cusPackage2Relations = package2.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			var cusPackage1Relation1 = cusPackage1Relations.First();
			var cusPackage1Relation2 = cusPackage1Relations.ElementAt(1);
			var cusPackage2Relation1 = cusPackage2Relations.First();
			var cusPackage2Relation2 = cusPackage2Relations.ElementAt(1);
			var packableItem1 = packingList.PackableItems.First();
			var packableItem2 = packingList.PackableItems.ElementAt(1);
			package1.CustomsPackItem(packableItem2, 1);
			package2.CustomsPackItem(packableItem1, 1);
			package2.CustomsPackItem(packableItem2, 1);

			Assert(cusPackage1Relation1.IsPackedInfo.ReadOnly);
			Assert(!cusPackage1Relation2.IsPackedInfo.ReadOnly);
			Assert(!cusPackage2Relation1.IsPackedInfo.ReadOnly);
			Assert(!cusPackage2Relation2.IsPackedInfo.ReadOnly);
		}

		public void TestOriginalGoodsDescription()
		{
			invoiceLine.JI_Description = "XXX1";
			AssertEquals("XXX1", relation.OriginalGoodsDescription);
		}

		public void TestRelatedPacks()
		{
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceForTest = decl.Invoices.AddNew();
			var line1 = invoiceForTest.InvoiceLines.AddNew();
			line1.JI_Description = "ITEM 1";
			line1.JI_InvoiceQuantity = 2;
			line1.JI_InvoiceUQ = "PCE";
			var line2 = invoiceForTest.InvoiceLines.AddNew();
			line2.JI_Description = "ITEM 2";
			line2.JI_InvoiceQuantity = 3;
			line2.JI_InvoiceUQ = "PCE";

			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			var packageX = packingList.PackageJob.Packages.AddNew();
			packageX.KP_MarksAndNumbers = "PACK#X";
			var packageY = packingList.PackageJob.Packages.AddNew();
			packageY.KP_MarksAndNumbers = "PACK#Y";
			var cusPackageXRelations = packageX.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			var cusPackageYRelations = packageY.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			var cusPackageXRelation1 = cusPackageXRelations.First();
			var cusPackageXRelation2 = cusPackageXRelations.ElementAt(1);
			var cusPackageYRelation1 = cusPackageYRelations.First();
			var cusPackageYRelation2 = cusPackageYRelations.ElementAt(1);
			var packableItem1 = packingList.PackableItems.First();
			var packableItem2 = packingList.PackableItems.ElementAt(1);
			packageX.CustomsPackItem(packableItem2, 1);
			packageY.CustomsPackItem(packableItem1, 1);
			packageY.CustomsPackItem(packableItem2, 1);
			AssertEquals("PACK#Y", cusPackageXRelation1.RelatedPacks);
			AssertEquals("PACK#X,PACK#Y", cusPackageXRelation2.RelatedPacks);
			AssertEquals("PACK#Y", cusPackageYRelation1.RelatedPacks);
			AssertEquals("PACK#X,PACK#Y", cusPackageYRelation2.RelatedPacks);
		}

		public void TestIsSplit()
		{
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceForTest = decl.Invoices.AddNew();
			var line1 = invoiceForTest.InvoiceLines.AddNew();
			line1.JI_Description = "ITEM 1";
			line1.JI_InvoiceQuantity = 2;
			line1.JI_InvoiceUQ = "PCE";
			var line2 = invoiceForTest.InvoiceLines.AddNew();
			line2.JI_Description = "ITEM 2";
			line2.JI_InvoiceQuantity = 3;
			line2.JI_InvoiceUQ = "PCE";

			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			var packageX = packingList.PackageJob.Packages.AddNew();
			packageX.KP_MarksAndNumbers = "PACK#X";
			var packageY = packingList.PackageJob.Packages.AddNew();
			packageY.KP_MarksAndNumbers = "PACK#Y";
			var cusPackageXRelations = packageX.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			var cusPackageYRelations = packageY.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			var cusPackageXRelation1 = cusPackageXRelations.First();
			var cusPackageXRelation2 = cusPackageXRelations.ElementAt(1);
			var cusPackageYRelation1 = cusPackageYRelations.First();
			var cusPackageYRelation2 = cusPackageYRelations.ElementAt(1);
			var packableItem1 = packingList.PackableItems.First();
			var packableItem2 = packingList.PackableItems.ElementAt(1);

			var newPackableItem = (CusPackableItem)packableItem2.Clone();
			packageY.PackableItemRelataions.Add(new CusPackageCusPackableItemRelation(packageY, newPackableItem));
			var cusPackageYRelation3 = cusPackageYRelations.ElementAt(2);
			Assert("Should be false", !cusPackageXRelation1.IsSplit);
			Assert("Should be true", cusPackageXRelation2.IsSplit);
			Assert("Should be false", !cusPackageYRelation1.IsSplit);
			Assert("Should be true", cusPackageYRelation2.IsSplit);
			Assert("Should be true", cusPackageYRelation3.IsSplit);

			packableItem1.CUI_CUL = ZGuid.Empty;
			Assert("Should be false", !cusPackageXRelation1.IsSplit);
		}

		CusPackingList packingList;
		CusPackage cusPackage1;
		CusPackage cusPackage2;
		CusPackableItem cusPackableItem;
		CusPackageCusPackableItemRelation relation;
		BaseJobComInvoiceHeader invoice;
		BaseJobComInvoiceLine invoiceLine;

		protected override BusinessObject GetNewBusinessObject()
		{
			var testItem = relation.PackableItem;
			testItem.CUI_PackableQty = 3m;
			cusPackage1.CustomsPackItem(testItem, 3m);
			return relation;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			packingList = declaration.LoadOrCreateCusPackingList(Factory);
			cusPackage1 = packingList.PackageJob.Packages.AddNew();
			cusPackage2 = packingList.PackageJob.Packages.AddNew();
			relation = cusPackage1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			cusPackableItem = packingList.PackableItems.First();
		}
	}
}
