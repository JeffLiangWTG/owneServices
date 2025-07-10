using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(Drawback7551ImportDocLineCollection))]
	class Drawback7551ImportDocLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<Drawback7551ImportDocLineCollection>
	{
		public void TestMergeImportDocLinesFromInvoiceLines()
		{
			Declaration.US_DRWEnableMerge = true;

			var invoiceLineOne = Declaration.InvoiceLines.AddNew();
			invoiceLineOne.US_ImportEntryNo = "XJ500000001";
			invoiceLineOne.JI_InvoiceQuantity = 1200m;
			invoiceLineOne.JI_Description = "AAAAAA";
			invoiceLineOne.US_DRWClaimAmountOverriden_New = true;
			invoiceLineOne.DRWImportQuantity = 1200m;
			invoiceLineOne.DRWExportQuantity = 1200m;
			invoiceLineOne.DeclaredVFD = 2000m;
			invoiceLineOne.LineDuty = 150m;
			invoiceLineOne.LineDutyRateDesc = "10%";

			var invoiceLineTwo = Declaration.InvoiceLines.AddNew();
			invoiceLineTwo.US_ImportEntryNo = "XJ500000001";
			invoiceLineTwo.JI_InvoiceQuantity = 1200m;
			invoiceLineTwo.JI_Description = "AAAAAA";
			invoiceLineTwo.US_DRWClaimAmountOverriden_New = true;
			invoiceLineTwo.DRWImportQuantity = 1200m;
			invoiceLineTwo.DRWExportQuantity = 1200m;
			invoiceLineTwo.DeclaredVFD = 2000m;
			invoiceLineTwo.LineDuty = 200m;
			invoiceLineTwo.LineDutyRateDesc = "10%";

			var invoiceLineThree = Declaration.InvoiceLines.AddNew();
			invoiceLineThree.US_ImportEntryNo = "XJ500000001";
			invoiceLineThree.JI_InvoiceQuantity = 1200m;
			invoiceLineThree.JI_Description = "BBBBBB";
			invoiceLineThree.US_DRWClaimAmountOverriden_New = true;
			invoiceLineThree.DRWImportQuantity = 1200m;
			invoiceLineThree.DRWExportQuantity = 1200m;
			invoiceLineThree.DeclaredVFD = 2000m;
			invoiceLineThree.LineDuty = 200m;
			invoiceLineThree.LineDutyRateDesc = "10%";

			var importDocLineCollection = new Drawback7551ImportDocLineCollection(Declaration.InvoiceLines, Factory);
			importDocLineCollection.LoadDrawback7551DocLines(Declaration);
			AssertEquals(2, importDocLineCollection.Count);
			AssertEquals(346.5m, importDocLineCollection[0].DrawbackClaimDuty);
			AssertEquals(198m, importDocLineCollection[1].DrawbackClaimDuty);
		}

		JobDeclaration Declaration
		{
			get { return fDeclaration ?? (fDeclaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration fDeclaration;

		protected override Drawback7551ImportDocLineCollection GetCollectionToTest()
		{
			return new Drawback7551ImportDocLineCollection(Declaration.InvoiceLines, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var invoiceLine = Declaration.FilteredInvoiceLines.AddNew();
			return new Drawback7551DocLine(invoiceLine);
		}
	}
}
