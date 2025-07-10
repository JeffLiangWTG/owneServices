using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class BaseSubInvoiceLineCollectionTest : TestCaseWithFactory
	{
		public void TestIAllInvoiceLinesContains()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceGroupHeader topGroup = testDec.JobComInvoiceGroupHeaders[0];
			BaseJobComInvoiceGroupHeader subGroup1 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice1 = subGroup1.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line1 = invoice1.JobComInvoiceLines.AddNew();

			BaseJobComInvoiceGroupHeader subGroup2 = topGroup.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice2 = subGroup2.JobComInvoiceHeaders.AddNew();
			BaseJobComInvoiceLine line2 = invoice2.JobComInvoiceLines.AddNew();

			AssertEquals("IAllInvoiceLines Contains", true, ((IAllInvoiceLines)topGroup.AllJobComInvoiceLines).Contains(line1));
			AssertEquals("IAllInvoiceLines Contains", true, ((IAllInvoiceLines)topGroup.AllJobComInvoiceLines).Contains(line2));

			AssertEquals("IAllInvoiceLines Contains", true, ((IAllInvoiceLines)subGroup1.AllJobComInvoiceLines).Contains(line1));
			AssertEquals("IAllInvoiceLines Contains", false, ((IAllInvoiceLines)subGroup1.AllJobComInvoiceLines).Contains(line2));

			AssertEquals("IAllInvoiceLines Contains", false, ((IAllInvoiceLines)subGroup2.AllJobComInvoiceLines).Contains(line1));
			AssertEquals("IAllInvoiceLines Contains", true, ((IAllInvoiceLines)subGroup2.AllJobComInvoiceLines).Contains(line2));
		}

		public void TestBuildCollectionForTopGroupHeader()
		{
			BaseGroupHeaderInvoiceLineViewCollection testCollection = topGroupHeader.AllJobComInvoiceLines;
			AssertEquals("Line1 is part of the collection", true, testCollection.Contains(invoiceLine1));
			AssertEquals("Line2 is part of the collection", true, testCollection.Contains(invoiceLine2));
		}

		public void TestBuildCollectionForSubGroupheader1()
		{
			BaseGroupHeaderInvoiceLineViewCollection testCollection = subGroupHeader1.AllJobComInvoiceLines;
			AssertEquals("Line1 is part of the collection", true, testCollection.Contains(invoiceLine1));
			AssertEquals("Line2 is not part of the collection", false, testCollection.Contains(invoiceLine2));
		}

		public void TestBuildCollectionForSubGroupHeader2()
		{
			BaseGroupHeaderInvoiceLineViewCollection testCollection = subGroupHeader2.AllJobComInvoiceLines;
			AssertEquals("Line1 is not part of the collection", false, testCollection.Contains(invoiceLine1));
			AssertEquals("Line2 is part of the collection", true, testCollection.Contains(invoiceLine2));
			AssertEquals("Line3 is part of the collection", true, testCollection.Contains(invoiceLine3));
		}

		public void TestRebuildCollectionWhenANewLineAdded()
		{
			BaseGroupHeaderInvoiceLineViewCollection testCollection = subGroupHeader1.AllJobComInvoiceLines;
			BaseJobComInvoiceLine newLine = testDec.FilteredInvoiceLines.AddNew();
			newLine.JI_Calc_Invoice = invoice1.JZ_InvoiceNumber;
			AssertEquals("New line is part of the collection", true, testCollection.Contains(newLine));
		}

		public void TestRebuildCollectionWhenANewInvoiceAdded()
		{
			BaseGroupHeaderInvoiceLineViewCollection testCollection = subGroupHeader1.AllJobComInvoiceLines;
			BaseJobComInvoiceHeader newHeader = testDec.Invoices.AddNew();
			newHeader.JZ_InvoiceNumber = "3";
			newHeader.JZ_Calc_GroupInvoice = subGroupHeader1.JZ_InvoiceNumber;
			BaseJobComInvoiceLine newLine = testDec.FilteredInvoiceLines.AddNew();
			newLine.JI_Calc_Invoice = newHeader.JZ_InvoiceNumber;

			AssertEquals("New line is part of the collection", true, testCollection.Contains(newLine));
			AssertEquals("New line is part of the collection", true, topGroupHeader.AllJobComInvoiceLines.Contains(newLine));
			AssertEquals("New line is not part of the collection", false, subGroupHeader2.AllJobComInvoiceLines.Contains(newLine));
		}

		BaseJobDeclaration testDec;
		BaseJobComInvoiceGroupHeader topGroupHeader;
		BaseJobComInvoiceGroupHeader subGroupHeader1;
		BaseJobComInvoiceGroupHeader subGroupHeader2;
		BaseJobComInvoiceGroupHeader subGroupHeader3;
		BaseJobComInvoiceHeader invoice1;
		BaseJobComInvoiceHeader invoice2;
		BaseJobComInvoiceHeader invoice3;
		BaseJobComInvoiceLine invoiceLine1;
		BaseJobComInvoiceLine invoiceLine2;
		BaseJobComInvoiceLine invoiceLine3;

		protected override void SetUp()
		{
			base.SetUp();
			testDec = Factory.New<BaseJobDeclaration>();
			topGroupHeader = testDec.JobComInvoiceGroupHeaders[0];
			subGroupHeader1 = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			subGroupHeader1.JZ_InvoiceNumber = "Sub1";
			subGroupHeader2 = topGroupHeader.JobComInvoiceGroupHeaders.AddNew();
			subGroupHeader2.JZ_InvoiceNumber = "Sub2";
			subGroupHeader3 = subGroupHeader2.JobComInvoiceGroupHeaders.AddNew();
			subGroupHeader3.JZ_InvoiceNumber = "Sub3";
			testDec.ActiveGroupHeader.SwapGroup(subGroupHeader1);
			invoice1 = subGroupHeader1.JobComInvoiceHeaders.AddNew();
			invoice1.JZ_InvoiceNumber = "1";
			invoice2 = subGroupHeader2.JobComInvoiceHeaders.AddNew();
			invoice2.JZ_InvoiceNumber = "2";
			invoice3 = subGroupHeader3.JobComInvoiceHeaders.AddNew();
			invoice3.JZ_InvoiceNumber = "2_3";
			invoiceLine1 = invoice1.JobComInvoiceLines.AddNew();
			invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine3 = invoice3.JobComInvoiceLines.AddNew();
		}
	}
}
