using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Drawback7551ExportDocLineCollection))]
	class Drawback7551ExportDocLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<Drawback7551ExportDocLineCollection>
	{
		[TestDate(2015, 08, 13)]
		public void TestMergeExportDocLinesFromInvoiceLines()
		{
			Declaration.US_DRWEnableMerge = true;

			var invoiceLineOne = Declaration.InvoiceLines.AddNew();
			invoiceLineOne.US_DRWExportDate = ZDate.Today;
			invoiceLineOne.US_DRWExportAction = "D";
			invoiceLineOne.US_DRWExportID = "EXPINVNO";
			invoiceLineOne.US_DRWExportDest = "AU";
			invoiceLineOne.US_DRWClaimAmountOverriden_New = true;
			invoiceLineOne.DRWExportQuantity = 1200m;

			var invoiceLineTwo = Declaration.InvoiceLines.AddNew();
			invoiceLineTwo.US_DRWExportDate = ZDate.Today;
			invoiceLineTwo.US_DRWExportAction = "D";
			invoiceLineTwo.US_DRWExportID = "EXPINVNO";
			invoiceLineTwo.US_DRWExportDest = "AU";
			invoiceLineTwo.US_DRWClaimAmountOverriden_New = true;
			invoiceLineTwo.DRWExportQuantity = 2400m;

			var invoiceLineThree = Declaration.InvoiceLines.AddNew();
			invoiceLineThree.US_DRWExportDate = ZDate.Today;
			invoiceLineThree.US_DRWExportAction = "D";
			invoiceLineThree.US_DRWExportID = "EXPINVNO";
			invoiceLineThree.US_DRWExportDest = "CN";
			invoiceLineThree.US_DRWClaimAmountOverriden_New = true;
			invoiceLineThree.DRWExportQuantity = 1200m;

			var exportDocLineCollection = new Drawback7551ExportDocLineCollection(Declaration.InvoiceLines, Factory);
			exportDocLineCollection.LoadDrawback7551DocLines(Declaration);
			AssertEquals(2, exportDocLineCollection.Count);
			AssertEquals(3600m, exportDocLineCollection[0].ExportQty);
			AssertEquals(1200m, exportDocLineCollection[1].ExportQty);
		}

		JobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration fDeclaration;

		protected override Drawback7551ExportDocLineCollection GetCollectionToTest()
		{
			return new Drawback7551ExportDocLineCollection(Declaration.InvoiceLines, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoiceLine = Declaration.FilteredInvoiceLines.AddNew();
			return new Drawback7551DocLine(invoiceLine);
		}
	}
}
