using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPackageCusPackableItemRelationCollection))]
	sealed class CusPackageCusPackableItemRelationCollectionBaseOnlyTest : NonPersistentBusinessObjectCollectionTestCase<CusPackageCusPackableItemRelationCollection>
	{
		protected override CusPackageCusPackableItemRelationCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var package = cusPackingList.PackageJob.Packages.AddNew();
			return package.PackableItemRelataions;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = cusPackingList.PackableItems.AddNew();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			return new CusPackageCusPackableItemRelation(cusPackage, cusPackableItem);
		}

		public void TestBuildElementForDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var cusPackage1 = cusPackingList.PackageJob.Packages.AddNew();
			var cusPackage2 = cusPackingList.PackageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();

			var cusPackableItem1 = cusPackingList.PackableItems.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			cusPackableItem1.CUI_JI = invoiceLine1.PK;
			cusPackableItem1.CUI_PackableQty = 5;

			var cusPackableItem2 = cusPackingList.PackableItems.AddNew();
			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			cusPackableItem2.CUI_JI = invoiceLine2.PK;
			cusPackableItem2.CUI_PackableQty = 7;

			cusPackage1.CustomsPackItem(cusPackableItem1, 2);

			cusPackage2.CustomsPackItem(cusPackableItem1, 3);
			cusPackage2.CustomsPackItem(cusPackableItem2, 6);

			var relations1 = new CusPackageCusPackableItemRelationCollection(cusPackage1);
			relations1.Load();
			AssertEquals(2, relations1.Count);
			AssertEquals(1, relations1.Cast<CusPackageCusPackableItemRelation>().Count(x => x.IsPacked));

			var relations2 = new CusPackageCusPackableItemRelationCollection(cusPackage2);
			relations2.Load();
			AssertEquals(2, relations2.Count);
			AssertEquals(2, relations2.Cast<CusPackageCusPackableItemRelation>().Count(x => x.IsPacked));
		}

		public void TestBuildElementAddsNewPackableItemsForNewInvoiceLine()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			Factory.Save();
			var cusPackingList = declaration.CreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();
			var relations = new CusPackageCusPackableItemRelationCollection(cusPackage);
			relations.Load();

			var packableItems = cusPackingList.PackableItems;
			AssertEquals(1, relations.Count);
			AssertEquals(1, packableItems.Count);
			AssertEquals(1, packableItems.Count(x => x.CUI_JI == invoiceLine1.PK));

			var invoiceLine2 = invoice.InvoiceLines.AddNew();
			relations = new CusPackageCusPackableItemRelationCollection(cusPackage);
			relations.Load();

			AssertEquals(2, relations.Count);
			AssertEquals(2, packableItems.Count);
			AssertEquals(1, packableItems.Count(x => x.CUI_JI == invoiceLine2.PK));
		}

		public void TestSortByInvoiceNumberAscending()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();

			var relations = cusPackage.PackableItemRelataions;
			relations.Sort(CusPackageCusPackableItemRelation.Schema.InvoiceNumber, ListSortDirection.Ascending);
			AssertEquals("1", relations[0].InvoiceNumber);
			AssertEquals("1", relations[1].InvoiceNumber);
			AssertEquals("2", relations[2].InvoiceNumber);
			AssertEquals("2", relations[3].InvoiceNumber);

			AssertEquals((short)1, relations[0].InvoiceLineNumber);
			AssertEquals((short)2, relations[1].InvoiceLineNumber);
			AssertEquals((short)1, relations[2].InvoiceLineNumber);
			AssertEquals((short)2, relations[3].InvoiceLineNumber);
		}

		public void TestSortByInvoiceNumberDescending()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();

			var relations = cusPackage.PackableItemRelataions;
			relations.Sort(CusPackageCusPackableItemRelation.Schema.InvoiceNumber, ListSortDirection.Descending);
			AssertEquals("2", relations[0].InvoiceNumber);
			AssertEquals("2", relations[1].InvoiceNumber);
			AssertEquals("1", relations[2].InvoiceNumber);
			AssertEquals("1", relations[3].InvoiceNumber);

			AssertEquals((short)2, relations[0].InvoiceLineNumber);
			AssertEquals((short)1, relations[1].InvoiceLineNumber);
			AssertEquals((short)2, relations[2].InvoiceLineNumber);
			AssertEquals((short)1, relations[3].InvoiceLineNumber);
		}

		public void TestSortByInvoiceLineNumberAscending()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();

			var relations = cusPackage.PackableItemRelataions;
			relations.Sort(CusPackageCusPackableItemRelation.Schema.InvoiceLineNumber, ListSortDirection.Descending);
			AssertEquals("2", relations[0].InvoiceNumber);
			AssertEquals("2", relations[1].InvoiceNumber);
			AssertEquals("1", relations[2].InvoiceNumber);
			AssertEquals("1", relations[3].InvoiceNumber);

			AssertEquals((short)2, relations[0].InvoiceLineNumber);
			AssertEquals((short)1, relations[1].InvoiceLineNumber);
			AssertEquals((short)2, relations[2].InvoiceLineNumber);
			AssertEquals((short)1, relations[3].InvoiceLineNumber);
		}

		public void TestSortByInvoiceLineNumberDescending()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();

			var relations = cusPackage.PackableItemRelataions;
			relations.Sort(CusPackageCusPackableItemRelation.Schema.InvoiceLineNumber, ListSortDirection.Descending);
			AssertEquals("2", relations[0].InvoiceNumber);
			AssertEquals("2", relations[1].InvoiceNumber);
			AssertEquals("1", relations[2].InvoiceNumber);
			AssertEquals("1", relations[3].InvoiceNumber);

			AssertEquals((short)2, relations[0].InvoiceLineNumber);
			AssertEquals((short)1, relations[1].InvoiceLineNumber);
			AssertEquals((short)2, relations[2].InvoiceLineNumber);
			AssertEquals((short)1, relations[3].InvoiceLineNumber);
		}

		public void TestSortByPackableItemSequence()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();

			var relations = cusPackage.PackableItemRelataions;
			relations.Sort(CusPackageCusPackableItemRelation.Schema.Sequence, ListSortDirection.Ascending);
			AssertEquals((short)1, relations[0].Sequence);
			AssertEquals((short)2, relations[1].Sequence);
			AssertEquals((short)3, relations[2].Sequence);
			AssertEquals((short)4, relations[3].Sequence);

			relations.Sort(CusPackageCusPackableItemRelation.Schema.Sequence, ListSortDirection.Descending);
			AssertEquals((short)4, relations[0].Sequence);
			AssertEquals((short)3, relations[1].Sequence);
			AssertEquals((short)2, relations[2].Sequence);
			AssertEquals((short)1, relations[3].Sequence);
		}

		public void TestSortWithPackableItemDeleted()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();

			var relations = cusPackage.PackableItemRelataions;
			relations[2].PackableItem.CUI_CUL = ZGuid.Empty;
			relations[2].PackableItem.Delete();
			AssertNoExceptionThrown(() =>
			{
				relations.Sort(CusPackageCusPackableItemRelation.Schema.Sequence, ListSortDirection.Ascending);
				Assert(relations[0].PackableItem.IsDeleted);
				AssertEquals((short)1, relations[1].Sequence);
				AssertEquals((short)2, relations[2].Sequence);
				AssertEquals((short)3, relations[3].Sequence);

				relations.Sort(CusPackageCusPackableItemRelation.Schema.Sequence, ListSortDirection.Descending);
				AssertEquals((short)3, relations[0].Sequence);
				AssertEquals((short)2, relations[1].Sequence);
				AssertEquals((short)1, relations[2].Sequence);
				Assert(relations[3].PackableItem.IsDeleted);
			});
		}

		public void TestErrorIsReportedWhenApplySort()
		{
			ErrorReporter.Clear();

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
			var packages1 = packingList.PackageJob.Packages.AddNew();
			var packages2 = packingList.PackageJob.Packages.AddNew();
			var packagesR1 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			packagesR1.PackableQuantity = 10;
			packagesR1.PackableUQ = "PCE";
			var packagesR2 = packages1.PackableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(1);
			packagesR2.PackableQuantity = 10;
			packagesR2.PackableUQ = "PCE";
			Factory.Save();

			var invoiceLine3 = invoice.InvoiceLines.AddNew();
			invoiceLine3.JI_Description = "Line 3";
			invoiceLine3.JI_CustomsQuantity = 300;
			invoiceLine3.JI_InvoiceUQ = "PCE";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var cusPackingListWithNewFactory = declaration.LoadOrCreateCusPackingList(newFactory);
			var cusPackageWithNewFactory = (CusPackage)cusPackingListWithNewFactory.PackageJob.Packages.First();
			newFactory.Save();
			AssertEquals(3, cusPackageWithNewFactory.PackableItemRelataions.Count);

			invoiceLine3.Delete();
			Factory.Save();
			cusPackageWithNewFactory.PackableItemRelataions.Sort(CusPackageCusPackableItemRelation.Schema.Sequence, ListSortDirection.Ascending);
			Assert("MessageReported is empty.", !ErrorReporter.LastMessageReported.Contains("Should not be accessing a property on a deleted business object"));
			var exceptionMessage = ErrorReporter.LastExceptionReported?.Message ?? ZString.Empty;
			Assert("ExceptionReported message is empty", !exceptionMessage.Contains("Deleted row information cannot be accessed through the row."));
			ErrorReporter.Clear();
		}

		public void TestDeletePackableItemRelationByPackableItem()
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
			var packages1 = packingList.PackageJob.Packages.AddNew();
			var packages2 = packingList.PackageJob.Packages.AddNew();
			var packableItemRelataions = packages1.PackableItemRelataions;
			var packagesR1 = packableItemRelataions.Cast<CusPackageCusPackableItemRelation>().First();
			packagesR1.PackableQuantity = 10;
			packagesR1.PackableUQ = "PCE";
			var packagesR2 = packableItemRelataions.Cast<CusPackageCusPackableItemRelation>().ElementAt(1);
			packagesR2.PackableQuantity = 10;
			packagesR2.PackableUQ = "PCE";
			AssertEquals(2, packableItemRelataions.Count);

			packableItemRelataions.DeletePackableItemRelationByPackableItem(packagesR1.PackableItem);
			Assert(packagesR1.IsDeleted);
			AssertEquals(1, packableItemRelataions.Count);
		}

		public void TestSortInvoiceLinesOnInvoiceLineNumberWhenGeneratePackingListItem()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			invoice1.JZ_InvoiceDisplaySequence = 1;
			var line1 = invoice1.InvoiceLines.AddNew();
			line1.JI_Description = "Line 1";
			line1.JI_LineNo = 2;
			var line2 = invoice1.InvoiceLines.AddNew();
			line2.JI_Description = "Line 2";
			line2.JI_LineNo = 1;

			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();
			var relations = cusPackage.PackableItemRelataions;

			CombineAssertions(() =>
			{
				AssertEquals((short)1, relations[0].Sequence);
				AssertEquals("Line 2", relations[0].GoodsDescription);

				AssertEquals((short)2, relations[1].Sequence);
				AssertEquals("Line 1", relations[1].GoodsDescription);
			});
		}

		public void TestSortInvoiceLinesOnInvoiceNumberWhenGeneratePackingListItem()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			invoice1.JZ_InvoiceDisplaySequence = 2;
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice2.JZ_InvoiceDisplaySequence = 1;
			var line1 = invoice1.InvoiceLines.AddNew();
			line1.JI_Description = "Line 1";
			line1.JI_LineNo = 1;
			var line2 = invoice2.InvoiceLines.AddNew();
			line2.JI_Description = "Line 2";
			line2.JI_LineNo = 1;

			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();
			var relations = cusPackage.PackableItemRelataions;

			CombineAssertions(() =>
			{
				AssertEquals((short)1, relations[0].Sequence);
				AssertEquals("Line 1", relations[0].GoodsDescription);

				AssertEquals((short)2, relations[1].Sequence);
				AssertEquals("Line 2", relations[1].GoodsDescription);
			});
		}

		public void TestGrouping()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			Factory.Save();
			var cusPackingList = declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			var cusPackableItem = cusPackingList.PackableItems.AddNew();
			cusPackableItem.CUI_JI = invoiceLine.PK;
			cusPackableItem.Grouping = "test group";
			var relation = new CusPackageCusPackableItemRelation(cusPackage, cusPackableItem);
			CombineAssertions(() =>
			{
				AssertEquals("test group", relation.Grouping);
				relation.Grouping = "group 2";
				AssertEquals("group 2", cusPackableItem.Grouping);
			});
		}

		public void TestBuildElementWithoutDeclaration()
		{
			var cusPackingList = Factory.NewWithValidTestData<CusPackingList>();
			var cusPackage = cusPackingList.PackageJob.Packages.AddNew();
			var relations = new CusPackageCusPackableItemRelationCollection(cusPackage);
			relations.Load();
			AssertNull(cusPackingList.Declaration);
			AssertEquals(0, relations.Count);
		}
	}
}
