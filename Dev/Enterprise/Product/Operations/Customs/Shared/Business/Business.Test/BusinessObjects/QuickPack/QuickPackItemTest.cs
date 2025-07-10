using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(QuickPackItem))]
	class QuickPackItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInvoiceNumber()
		{
			AssertEquals("TESTINVOICENUMBER", quickPackItem.InvoiceNumber);
		}

		public void TestInvoiceLineNumber()
		{
			AssertEquals(ZShort.Parse("1"), quickPackItem.InvoiceLineNumber);
		}

		public void TestGoodsDesc()
		{
			AssertEquals("ITEM 1", quickPackItem.GoodsDesc);
		}

		public void TestNotPackedQty()
		{
			AssertEquals(2m, quickPackItem.NotPackedQty);
		}

		public void TestPackableUQ()
		{
			AssertEquals("PCE", quickPackItem.PackableUQ);
		}

		public void TestSequence()
		{
			var packingList = Factory.NewWithValidTestData<CusPackingList>();
			var packableItem1 = packingList.PackableItems.AddNew();
			var packableItem2 = packingList.PackableItems.AddNew();

			AssertEquals(ZShort.Parse("1"), packableItem1.CUI_Sequence);
			AssertEquals(ZShort.Parse("2"), packableItem2.CUI_Sequence);

			var quickPackItem1 = new QuickPackItem(packableItem1);
			var quickPackItem2 = new QuickPackItem(packableItem2);
			AssertEquals(ZShort.Parse("1"), quickPackItem1.Sequence);
			AssertEquals(ZShort.Parse("2"), quickPackItem2.Sequence);
		}

		public void TestPackSeq()
		{
			AssertEquals("default value", "0", quickPackItem.PackSeq);
			quickPackItem.PackSeq = "1";
			AssertEquals("should set pack when PackSeq changed", "Pack #1", quickPackItem.Pack);
			quickPackItem.PackSeq = "0";
			AssertEquals("should set pack to empty when PackSeq changed to 0", ZString.Empty, quickPackItem.Pack);
		}

		public void TestPackReadonly()
		{
			quickPackItem.PackSeq = "1";
			Assert("should be read-only when PackSeq is not 0", quickPackItem.PackInfo.ReadOnly);
			quickPackItem.PackSeq = "0";
			Assert("should not be read-only when PackSeq is 0", !quickPackItem.PackInfo.ReadOnly);
		}

		public void TestDoQuickPackAction()
		{
			var packableItemRelataion1 = package1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().FirstOrDefault();
			var packableItemRelataion2 = package2.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().FirstOrDefault();
			var packableItemRelataion3 = package3.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().FirstOrDefault();
			packableItemRelataion1.PackedQty = 0m;
			packableItemRelataion1.IsPacked = false;
			packableItemRelataion2.PackedQty = 0m;
			packableItemRelataion2.IsPacked = false;
			packableItemRelataion3.PackedQty = 0m;
			packableItemRelataion3.IsPacked = false;

			quickPackItem.PackedQty = 3m;
			quickPackItem.PackSeq = "1";
			AssertEquals(3, packages.Count);
			quickPackItem.DoQuickPackAction(new HashSet<ZString>());
			AssertEquals(3, packages.Count);
			AssertEquals(3m, packableItemRelataion1.PackedQty);
			Assert(packableItemRelataion1.IsPacked);
			AssertEquals(0m, packableItemRelataion2.PackedQty);
			Assert(!packableItemRelataion2.IsPacked);
			AssertEquals(0m, packableItemRelataion3.PackedQty);
			Assert(!packableItemRelataion3.IsPacked);

			quickPackItem.PackedQty = 5m;
			quickPackItem.PackSeq = "3";
			quickPackItem.Pack = "Pack #2";
			AssertEquals(3, packages.Count);
			quickPackItem.DoQuickPackAction(new HashSet<ZString>());
			AssertEquals(3, packages.Count);
			AssertEquals(0m, packableItemRelataion2.PackedQty);
			Assert(!packableItemRelataion2.IsPacked);
			AssertEquals(5m, packableItemRelataion3.PackedQty);
			Assert(packableItemRelataion3.IsPacked);

			quickPackItem.PackedQty = 8m;
			quickPackItem.PackSeq = "0";
			quickPackItem.Pack = "Pack #4";
			quickPackItem.DoQuickPackAction(new HashSet<ZString>());
			AssertEquals(4, packages.Count);
			var packableItemRelataion4 = packages.Cast<CusPackage>().FirstOrDefault(c => c.KP_MarksAndNumbers == "Pack #4").PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().FirstOrDefault();
			AssertEquals(8m, packableItemRelataion4.PackedQty);
			Assert(packableItemRelataion4.IsPacked);

			quickPackItem.PackedQty = 9m;
			quickPackItem.PackSeq = "0";
			quickPackItem.Pack = "Pack #4";
			quickPackItem.DoQuickPackAction(new HashSet<ZString>() { "Pack #4" });
			AssertEquals(4, packages.Count);
			AssertEquals(17m, packableItemRelataion4.PackedQty);
			Assert(packableItemRelataion4.IsPacked);
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
			invoiceForTest.JZ_InvoiceNumber = "TestInvoiceNumber";
			var line = invoiceForTest.InvoiceLines.AddNew();
			line.JI_Description = "ITEM 1";
			line.JI_InvoiceQuantity = 8;
			line.JI_InvoiceUQ = "PCE";
			line.JI_LineNo = 1;

			var packingList = decl.LoadOrCreateCusPackingList(Factory);
			packages = packingList.PackageJob.Packages;
			package1 = packages.AddNew();
			package1.KP_MarksAndNumbers = "Pack #1";
			package2 = packages.AddNew();
			package2.KP_MarksAndNumbers = "Pack #2";
			package3 = packages.AddNew();
			package3.KP_MarksAndNumbers = "Pack #2";

			var packableItem = line.CreateNewCusPackableItem();
			packableItem.CUI_CUL = packingList.PK;
			package1.CustomsPackItem(packableItem, 1);
			package2.CustomsPackItem(packableItem, 2);
			package3.CustomsPackItem(packableItem, 3);

			quickPackItem = new QuickPackItem(packableItem);
			return quickPackItem;
		}

		CusPackageCollection packages;
		CusPackage package1;
		CusPackage package2;
		CusPackage package3;
		QuickPackItem quickPackItem;
	}
}
