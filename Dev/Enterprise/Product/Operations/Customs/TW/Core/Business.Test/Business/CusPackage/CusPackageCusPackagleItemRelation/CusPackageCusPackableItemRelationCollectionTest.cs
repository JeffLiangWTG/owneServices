using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business
{
	[TestedType(typeof(CusPackageCusPackableItemRelationCollection))]
	sealed class CusPackageCusPackableItemRelationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CusPackageCusPackableItemRelationCollection>
	{
		protected override CusPackageCusPackableItemRelationCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			Factory.Save();
			var cusPackingList = (CusPackingList)declaration.LoadOrCreateCusPackingList(Factory);
			Factory.Save();
			var package = cusPackingList.PackageJob.Packages.AddNew();
			return (CusPackageCusPackableItemRelationCollection)package.PackableItemRelataions;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
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

		[ExpectNoExceptions]
		public void TestBuildElementForInvoiceHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			var invoice2 = declaration.Invoices.AddNew();
			var invoiceLine1_1 = invoice1.InvoiceLines.AddNew();
			var invoiceLine1_2 = invoice1.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			invoice2.InvoiceLines.AddNew();
			Factory.Save();
			var cusPackingList = invoice1.CreateCusPackingList(Factory);
			Factory.Save();
			var cusPackage1 = (CusPackage)cusPackingList.PackageJob.Packages.AddNew();
			var relations1 = new CusPackageCusPackableItemRelationCollection(cusPackage1);
			relations1.Load();

			NUnit.Framework.Assert.That(cusPackingList.PackableItems.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(relations1.Count, NUnit.Framework.Is.EqualTo(2));
			NUnit.Framework.Assert.That(relations1.Cast<CusPackageCusPackableItemRelation>().Count(x => x.PackableItem.InvoiceLine.PK == invoiceLine1_1.PK), NUnit.Framework.Is.EqualTo(1));
			NUnit.Framework.Assert.That(relations1.Cast<CusPackageCusPackableItemRelation>().Count(x => x.PackableItem.InvoiceLine.PK == invoiceLine1_2.PK), NUnit.Framework.Is.EqualTo(1));
		}

		[ExpectNoExceptions]
		public void TestSortInvoiceLinesOnInvoiceLineNumberWhenGeneratePackingListItem()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
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
				NUnit.Framework.Assert.That(relations[0].Sequence, NUnit.Framework.Is.EqualTo((short)1).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(relations[0].GoodsDescription, NUnit.Framework.Is.EqualTo("Line 2").Using(CustomComparers.TypeComparison));

				NUnit.Framework.Assert.That(relations[1].Sequence, NUnit.Framework.Is.EqualTo((short)2).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(relations[1].GoodsDescription, NUnit.Framework.Is.EqualTo("Line 1").Using(CustomComparers.TypeComparison));
			});
		}

		[ExpectNoExceptions]
		public void TestSortInvoiceLinesOnInvoiceNumberWhenGeneratePackingListItem()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
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
				NUnit.Framework.Assert.That(relations[0].Sequence, NUnit.Framework.Is.EqualTo((short)1).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(relations[0].GoodsDescription, NUnit.Framework.Is.EqualTo("Line 2").Using(CustomComparers.TypeComparison));

				NUnit.Framework.Assert.That(relations[1].Sequence, NUnit.Framework.Is.EqualTo((short)2).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(relations[1].GoodsDescription, NUnit.Framework.Is.EqualTo("Line 1").Using(CustomComparers.TypeComparison));
			});
		}
	}
}
