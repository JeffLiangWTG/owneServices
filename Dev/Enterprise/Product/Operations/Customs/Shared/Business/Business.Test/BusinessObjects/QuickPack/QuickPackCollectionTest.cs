using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(QuickPackItemCollection))]
	sealed class QuickPackCollectionTest : NonPersistentBusinessObjectCollectionTestCase<QuickPackItemCollection>
	{
		protected override QuickPackItemCollection GetCollectionToTest()
		{
			var packingList = Factory.NewWithValidTestData<CusPackingList>();
			return new QuickPackItemCollection(packingList.PackableItems);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var packingList = Factory.NewWithValidTestData<CusPackingList>();
			var packableItem = packingList.PackableItems.AddNew();
			return new QuickPackItem(packableItem);
		}

		public void TestRebuildElements()
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

			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			var packages = packingList.PackageJob.Packages;
			var package = packages.AddNew();
			package.KP_MarksAndNumbers = "Pack #1";
			var cusPackage1Relations = package.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>();

			var packableItem1 = packingList.PackableItems.First();
			var packableItem2 = packingList.PackableItems.ElementAt(1);

			var quickPackItems = new QuickPackItemCollection(packingList.PackableItems);

			AssertEquals(0, quickPackItems.Count);
			quickPackItems.Load();
			AssertEquals(2, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "TEST 123" && x.InvoiceLineNumber == ZShort.Parse("1") && x.GoodsDesc == "ITEM 1" && x.NotPackedQty == 1m && x.PackableUQ == "PCE"));
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "TEST 123" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 2m && x.PackableUQ == "BAG"));

			invoiceForTest.JZ_InvoiceNumber = "CHANGE 456";

			quickPackItems.Load();
			AssertEquals(2, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 456" && x.InvoiceLineNumber == ZShort.Parse("1") && x.GoodsDesc == "ITEM 1" && x.NotPackedQty == 1m && x.PackableUQ == "PCE"));
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 456" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 2m && x.PackableUQ == "BAG"));

			packableItem1.CUI_GoodsDescription = "ITEM 3";
			quickPackItems.Load();
			AssertEquals(2, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 456" && x.InvoiceLineNumber == ZShort.Parse("1") && x.GoodsDesc == "ITEM 3" && x.NotPackedQty == 1m && x.PackableUQ == "PCE"));
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 456" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 2m && x.PackableUQ == "BAG"));

			package.CustomsPackItem(packableItem2, 1);
			quickPackItems.Load();
			AssertEquals(2, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 456" && x.InvoiceLineNumber == ZShort.Parse("1") && x.GoodsDesc == "ITEM 3" && x.NotPackedQty == 1m && x.PackableUQ == "PCE"));
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 456" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 1m && x.PackableUQ == "BAG"));

			package.CustomsPackItem(packableItem1, 1);
			quickPackItems.Load();
			AssertEquals(1, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 456" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 1m && x.PackableUQ == "BAG"));

			packableItem1.CUI_PackableUQ = "BBG";
			quickPackItems.Load();
			AssertEquals(1, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 456" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 1m && x.PackableUQ == "BAG"));

			packableItem2.CUI_PackableUQ = "BBG";
			quickPackItems.Load();
			AssertEquals(1, quickPackItems.Count);
			Assert(quickPackItems.Cast<QuickPackItem>().Any(x => x.InvoiceNumber == "CHANGE 456" && x.InvoiceLineNumber == ZShort.Parse("2") && x.GoodsDesc == "ITEM 2" && x.NotPackedQty == 1m && x.PackableUQ == "BBG"));
		}
	}
}
