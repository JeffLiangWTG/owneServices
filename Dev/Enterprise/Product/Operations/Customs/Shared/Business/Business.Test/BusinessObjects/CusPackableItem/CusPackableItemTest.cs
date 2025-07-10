using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackableItem))]
	sealed class CusPackableItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCUI_PackableUQList()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(CusPackableItem), CusPackableItem.Schema.CUI_PackableUQ, true, attrib => attrib.ListDataSourceMember == "Lookups.PackTypes");
		}

		public void TestRecalculateOldPackingListItemsSequence()
		{
			var dec1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var dec2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var packingList1 = dec1.LoadOrCreateCusPackingList(Factory);
			var packingList2 = dec2.LoadOrCreateCusPackingList(Factory);
			var packableItemA1 = packingList1.PackableItems.AddNew();
			var packableItemA2 = packingList1.PackableItems.AddNew();
			var packableItemA3 = packingList1.PackableItems.AddNew();
			var packableItemB1 = packingList2.PackableItems.AddNew();
			var packableItemB2 = packingList2.PackableItems.AddNew();
			var packableItemB3 = packingList2.PackableItems.AddNew();
			AssertEquals((ZShort)1, packableItemA1.CUI_Sequence);
			AssertEquals((ZShort)2, packableItemA2.CUI_Sequence);
			AssertEquals((ZShort)3, packableItemA3.CUI_Sequence);
			AssertEquals((ZShort)1, packableItemB1.CUI_Sequence);
			AssertEquals((ZShort)2, packableItemB2.CUI_Sequence);
			AssertEquals((ZShort)3, packableItemB3.CUI_Sequence);
			AssertEquals(packingList1.PK, packableItemA2.CUI_CUL);

			packableItemA2.CUI_CUL = packingList2.PK;
			AssertEquals((ZShort)1, packableItemA1.CUI_Sequence);
			AssertEquals((ZShort)2, packableItemA3.CUI_Sequence);
		}

		public void TestCUI_SequenceRecalculateWhenRenumbered()
		{
			var cusPackableItem2 = cusPackingList.PackableItems.AddNew();
			var cusPackableItem3 = cusPackingList.PackableItems.AddNew();
			AssertEquals((ZShort)1, cusPackableItem.CUI_Sequence);
			AssertEquals((ZShort)2, cusPackableItem2.CUI_Sequence);
			AssertEquals((ZShort)3, cusPackableItem3.CUI_Sequence);

			cusPackableItem2.CUI_Sequence = 3;
			AssertEquals((ZShort)1, cusPackableItem.CUI_Sequence);
			AssertEquals((ZShort)3, cusPackableItem2.CUI_Sequence);
			AssertEquals((ZShort)2, cusPackableItem3.CUI_Sequence);
		}

		public void TestSequenceNumberGenerator()
		{
			AssertEquals((ZShort)1, cusPackableItem.CUI_Sequence);

			var cusPackableItem2 = cusPackingList.PackableItems.AddNew();
			AssertEquals((ZShort)2, cusPackableItem2.CUI_Sequence);

			var cusPackableItem3 = cusPackingList.PackableItems.AddNew();
			AssertEquals((ZShort)3, cusPackableItem3.CUI_Sequence);

			cusPackableItem2.Delete();
			AssertEquals((ZShort)2, cusPackableItem3.CUI_Sequence);
		}

		public void TestInvoiceLine()
		{
			AssertEquals(invoiceLine.PK, cusPackableItem.InvoiceLine.PK);
		}

		public void TestCUI_InvoiceNumber()
		{
			invoice.JZ_InvoiceNumber = "12345";
			AssertEquals("12345", cusPackableItem.CUI_InvoiceNumber);
		}

		public void TestCUI_InvoiceLineNumber()
		{
			AssertEquals((short)1, cusPackableItem.CUI_InvoiceLineNumber);
		}

		public void TestOriginalGoodsDescription()
		{
			AssertEquals(ZString.Replicate('A', 525), cusPackableItem.CUI_OriginalGoodsDescription);
		}

		public void TestDeleteDivotWhenDelete()
		{
			cusPackableItem.CUI_PackableQty = 10;
			var package1 = cusPackingList.PackageJob.Packages.AddNew();
			var package2 = cusPackingList.PackageJob.Packages.AddNew();
			package1.CustomsPackItem(cusPackableItem, 5);
			package2.CustomsPackItem(cusPackableItem, 3);
			var divot1 = package1.PackedItemDivots.First(x => x.KI_ParentID == cusPackableItem.PK);
			var divot2 = package2.PackedItemDivots.First(x => x.KI_ParentID == cusPackableItem.PK);

			cusPackableItem.Delete();
			Assert(cusPackableItem.IsDeleted);
			Assert(divot1.IsDeleted);
			Assert(divot2.IsDeleted);
		}

		public void TestSupportsClone()
		{
			Assert(cusPackableItem.SupportsClone());
		}

		public void TestResetValuesFromInvoiceLine()
		{
			cusPackableItem.CUI_GoodsDescription = "GoodsDescription";
			cusPackableItem.CUI_PackableQty = 10m;
			cusPackableItem.CUI_PackableUQ = "BBK";

			AssertEquals("GoodsDescription", cusPackableItem.CUI_GoodsDescription);
			AssertEquals(10m, cusPackableItem.CUI_PackableQty);
			AssertEquals("BBK", cusPackableItem.CUI_PackableUQ);

			cusPackableItem.ResetValuesFromInvoiceLine();
			AssertEquals(ZString.Replicate('A', 525), cusPackableItem.CUI_GoodsDescription);
			AssertEquals(5m, cusPackableItem.CUI_PackableQty);
			AssertEquals("ACR", cusPackableItem.CUI_PackableUQ);
		}

		public void TestDeletePackableItemRelationWhenDeleteCorrespondingPackableItem()
		{
			var package = cusPackingList.PackageJob.Packages.AddNew();
			var packableItemRelataions = package.PackableItemRelataions;
			AssertEquals(1, packableItemRelataions.Count);

			cusPackableItem.Delete();
			AssertEquals(0, packableItemRelataions.Count);
		}

		public void TestShouldRefreshPackableItemRelataionCollectionWhenPackableItemDeleted()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_Description = "Line 1";
			invoiceLine1.JI_CustomsQuantity = 100;
			invoiceLine1.JI_InvoiceUQ = "PCE";
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			invoiceLine2.JI_Description = "Line 2";
			invoiceLine2.JI_CustomsQuantity = 200;
			invoiceLine2.JI_InvoiceUQ = "CTN";
			Factory.Save();

			var packingList = declaration.LoadOrCreateCusPackingList(Factory);
			packingList.CUL_Remarks = "Remarks 1";
			packingList.CUL_PackingListNumber = "PACK#1";
			packingList.CUL_PackingListDate = ZDate.Today;
			var packageJob = packingList.PackageJob;
			var packages1 = packageJob.Packages.AddNew();
			packageJob.Packages.AddNew();
			var packagesR1 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			packagesR1.PackableQuantity = 10;
			packagesR1.PackableUQ = "PCE";
			var packagesR2 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(1);
			packagesR2.PackableQuantity = 10;
			packagesR2.PackableUQ = "PCE";
			Factory.Save();

			var newItem = packingList.PackableItems.AddNew();
			newItem.CUI_JI = invoiceLine2.PK;
			newItem.CUI_PackableQty = 10;
			newItem.CUI_PackableUQ = "PCE";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var cusPackingListWithNewFactory = declaration.LoadOrCreateCusPackingList(newFactory);
			var cusPackageWithNewFactory = (CusPackage)cusPackingListWithNewFactory.PackageJob.Packages.First();
			newFactory.Save();
			AssertEquals(3, cusPackageWithNewFactory.PackableItemRelataions.Count);

			newItem.Delete();
			Factory.Save();
			AssertEquals(2, cusPackageWithNewFactory.PackableItemRelataions.Count);
		}

		public void TestIPackableItem_Quantity()
		{
			cusPackableItem.CUI_PackableQty = 5;
			AssertEquals(5m, packableItem.Quantity);
		}

		public void TestIPackableItem_Key()
		{
			AssertType(typeof(CusPackingGroupingKey), packableItem.Key);
		}

		public void TestIPackableItem_Split()
		{
			AssertNull(packableItem.Split(0));
			AssertNull(packableItem.Split(1));
			AssertNull(packableItem.Split(5));
		}

		public void TestNotPackedQty()
		{
			cusPackableItem.CUI_PackableQty = 10;
			cusPackingList.PackageJob.Packages.AddNew().CustomsPackItem(cusPackableItem, 1m);
			AssertEquals(9m, cusPackableItem.NotPackedQty);

			cusPackingList.PackageJob.Packages.AddNew().CustomsPackItem(cusPackableItem, 3m);
			AssertEquals(6m, cusPackableItem.NotPackedQty);
		}

		public void TestTotalPackedQty()
		{
			cusPackableItem.CUI_PackableQty = 10;
			cusPackingList.PackageJob.Packages.AddNew().CustomsPackItem(cusPackableItem, 1m);
			AssertEquals(1m, cusPackableItem.TotalPackedQty);

			cusPackingList.PackageJob.Packages.AddNew().CustomsPackItem(cusPackableItem, 3m);
			AssertEquals(4m, cusPackableItem.TotalPackedQty);
		}

		public void TestTotalPackedNetWeighty()
		{
			cusPackableItem.CUI_PackableQty = 10m;
			cusPackableItem.CUI_NetWeight = 20m;
			cusPackableItem.CUI_NetWeightUQ = "KG";
			cusPackingList.PackageJob.Packages.AddNew().CustomsPackItem(cusPackableItem, 1m);
			AssertEquals(2m, cusPackableItem.TotalPackedNetWeight);

			cusPackingList.PackageJob.Packages.AddNew().CustomsPackItem(cusPackableItem, 3m);
			AssertEquals(8m, cusPackableItem.TotalPackedNetWeight);
		}

		public void TestGrouping()
		{
			cusPackableItem.Grouping = "test group";
			var groupNote = new HiddenTextNote(cusPackableItem, PredefinedNoteTypes.Instance.PackingListItemGrouping.Description);
			CombineAssertions(() =>
			{
				AssertEquals("test group", groupNote.Text);
				groupNote.Text = "group 2";
				AssertEquals("group 2", cusPackableItem.Grouping);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => cusPackableItem;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => cusPackableItem;

		protected override BusinessObject GetNewBusinessObject() => cusPackableItem;

		BaseJobComInvoiceHeader invoice;
		BaseJobComInvoiceLine invoiceLine;
		CusPackingList cusPackingList;
		CusPackableItem cusPackableItem;
		IPackableItem packableItem;

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = ZString.Replicate('A', BaseJobComInvoiceLine.Schema.JI_DescriptionMaxLength);
			invoiceLine.JI_InvoiceQuantity = 5m;
			invoiceLine.JI_InvoiceUQ = "ACR";
			Factory.Save();
			cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			cusPackableItem = cusPackingList.PackableItems.AddNew();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.CUI_ClusterKey = 1;
			packableItem = cusPackableItem;
		}
	}
}
