using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(QuickPack))]
	class QuickPackTest : NonPersistentBusinessObjectTestCase
	{
		public void TestQuickPackItems()
		{
			var package = packingList.PackageJob.Packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #1";
			var cusPackage1Relations = package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();

			var packableItems = packingList.PackableItems;
			var packableItem1 = packableItems.First();
			var packableItem2 = packableItems.ElementAt(1);
			var quickPackItems = new QuickPack(packableItems).QuickPackItems;

			AssertEquals(2, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "TEST 123" && x.InvoiceLineNumber == ZShort.Parse("1") && x.GoodsDesc == "ITEM 1" && x.NotPackedQty == 1m && x.PackableUQ == "PCE"));
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "TEST 123" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 2m && x.PackableUQ == "BAG"));

			packableItem1.InvoiceLine.InvoiceHeader.JZ_InvoiceNumber = "CHANGE 123";
			quickPackItems = new QuickPack(packableItems).QuickPackItems;
			AssertEquals(2, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 123" && x.InvoiceLineNumber == ZShort.Parse("1") && x.GoodsDesc == "ITEM 1" && x.NotPackedQty == 1m && x.PackableUQ == "PCE"));
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 123" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 2m && x.PackableUQ == "BAG"));

			packableItem1.CUI_GoodsDescription = "ITEM 3";
			quickPackItems = new QuickPack(packableItems).QuickPackItems;
			AssertEquals(2, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 123" && x.InvoiceLineNumber == ZShort.Parse("1") && x.GoodsDesc == "ITEM 3" && x.NotPackedQty == 1m && x.PackableUQ == "PCE"));
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 123" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 2m && x.PackableUQ == "BAG"));

			package.CustomsPackItem(packableItem2, 1);
			quickPackItems = new QuickPack(packableItems).QuickPackItems;
			AssertEquals(2, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 123" && x.InvoiceLineNumber == ZShort.Parse("1") && x.GoodsDesc == "ITEM 3" && x.NotPackedQty == 1m && x.PackableUQ == "PCE"));
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 123" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 1m && x.PackableUQ == "BAG"));

			package.CustomsPackItem(packableItem1, 1);
			quickPackItems = new QuickPack(packableItems).QuickPackItems;
			AssertEquals(1, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 123" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 1m && x.PackableUQ == "BAG"));

			packableItem2.CUI_PackableUQ = "BBG";
			quickPackItems = new QuickPack(packableItems).QuickPackItems;
			AssertEquals(1, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 123" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 1m && x.PackableUQ == "BBG"));
		}

		public void TestDoQuickPackAction_SameToExisting()
		{
			var packages = packingList.PackageJob.Packages;
			var package1 = packages.AddNew();
			package1.KP_MarksAndNumbers = "Pack #1";
			var cusPackage1Relations = package1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();

			var package2 = packages.AddNew();
			package2.KP_MarksAndNumbers = "Pack #2";
			var cusPackage2Relations = package2.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();

			var packableItems = packingList.PackableItems;
			var quickPack = new QuickPack(packableItems);
			var quickPackItems = quickPack.QuickPackItems.Cast<QuickPackItem>();
			var quickPackItem1 = quickPackItems.First();
			var quickPackItem2 = quickPackItems.ElementAt(1);
			quickPackItem1.PackSeq = "1";
			quickPackItem1.PackedQty = 1;

			quickPackItem2.Pack = "Pack #2";
			quickPackItem2.PackedQty = 2;
			AssertEquals(2, packages.Count);
			Assert(cusPackage1Relations.All(c => !c.IsPacked));
			Assert(cusPackage2Relations.All(c => !c.IsPacked));

			quickPack.DoQuickPackAction();
			AssertEquals(3, packages.Count);
			Assert(cusPackage1Relations.Any(c => c.IsPacked && c.PackedQty == 1m));
			Assert(cusPackage2Relations.All(c => !c.IsPacked));
			var package2New = packages.FirstOrDefault(c => c.KP_MarksAndNumbers == "Pack #2" && c.PK != package2.PK);
			AssertNotNull("should create a new package with 'Package #2'", package2New);
			var cusPackage2RelationsNew = ((CusPackage)package2New).PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			Assert(cusPackage2RelationsNew.Any(c => c.IsPacked && c.PackedQty == 2m));
		}

		public void TestDoQuickPackAction_SameToNew()
		{
			var packages = packingList.PackageJob.Packages;
			var package1 = packages.AddNew();
			package1.KP_MarksAndNumbers = "Pack #1";
			var cusPackage1Relations = package1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();

			var package2 = packages.AddNew();
			package2.KP_MarksAndNumbers = "Pack #2";
			var cusPackage2Relations = package2.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();

			var packableItems = packingList.PackableItems;
			var quickPack = new QuickPack(packableItems);
			var quickPackItems = quickPack.QuickPackItems.Cast<QuickPackItem>();
			var quickPackItem1 = quickPackItems.First();
			var quickPackItem2 = quickPackItems.ElementAt(1);
			quickPackItem1.Pack = "Pack #2";
			quickPackItem1.PackedQty = 1;

			quickPackItem2.Pack = "Pack #2";
			quickPackItem2.PackedQty = 2;
			AssertEquals(2, packages.Count);
			Assert(cusPackage1Relations.All(c => !c.IsPacked));
			Assert(cusPackage2Relations.All(c => !c.IsPacked));

			quickPack.DoQuickPackAction();
			AssertEquals(3, packages.Count);
			Assert(cusPackage1Relations.All(c => !c.IsPacked));
			Assert(cusPackage2Relations.All(c => !c.IsPacked));
			var package3 = packages.Single(c => c.KP_MarksAndNumbers == "Pack #2" && c.KP_Sequence != package2.KP_Sequence);
			AssertNotNull("should create 1 new package for 2 quickPackItems", package3);
			var cusPackage3Relations = ((CusPackage)package3).PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();
			Assert("item 1 should packed to the new package", cusPackage3Relations.Any(c => c.IsPacked && c.PackedQty == 1m));
			Assert("item 2 should packed to the new package", cusPackage3Relations.Any(c => c.IsPacked && c.PackedQty == 2m));
		}

		protected override void SetUp()
		{
			base.SetUp();
			GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var decl = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoiceForTest = decl.Invoices.AddNew();
			invoiceForTest.JZ_InvoiceNumber = "TEST 123";
			var line1 = invoiceForTest.InvoiceLines.AddNew();
			line1.JI_Description = "ITEM 1";
			line1.JI_InvoiceQuantity = 1;
			line1.JI_InvoiceUQ = "PCE";
			line1.JI_LineNo = 1;
			var line2 = invoiceForTest.InvoiceLines.AddNew();
			line2.JI_Description = "ITEM 2";
			line2.JI_InvoiceQuantity = 2;
			line2.JI_InvoiceUQ = "BAG";
			line2.JI_LineNo = 2;
			packingList = decl.LoadOrCreateCusPackingList(Factory);
			return new QuickPack(packingList.PackableItems);
		}

		CusPackingList packingList;
	}
}
