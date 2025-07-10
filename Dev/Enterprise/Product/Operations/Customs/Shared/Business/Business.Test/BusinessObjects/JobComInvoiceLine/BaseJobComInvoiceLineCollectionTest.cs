using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	public class BaseJobComInvoiceLineCollectionTest : CountrySpecificTestCase
	{
		public void TestClonedInvoiceLinesDoesntChangeInvoiceLineNo()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line2.JI_LineNo);
			AssertEquals("Line No", (short)3, line3.JI_LineNo);

			line1.JI_LineNo = (short)3;
			line2.JI_LineNo = (short)1;
			line3.JI_LineNo = (short)2;

			BaseJobDeclaration clonedDec = (BaseJobDeclaration)testDec.Clone();
			AssertEquals("Line1 should be 3", (short)3, line1.JI_LineNo);
			AssertEquals("Line2 should be 1", (short)1, line2.JI_LineNo);
			AssertEquals("Line3 should be 2", (short)2, line3.JI_LineNo);

			clonedDec.ThrowAwayMerge();
			AssertEquals("Line1 should be 3", (short)3, line1.JI_LineNo);
			AssertEquals("Line2 should be 1", (short)1, line2.JI_LineNo);
			AssertEquals("Line3 should be 2", (short)2, line3.JI_LineNo);
		}

		public virtual void TestCloneHasChanges()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			line1.Charges.AddNew();

			BaseJobDeclaration clonedDeclaration = (BaseJobDeclaration)testDec.TemplateCopy();
			AssertEquals("Has one invoice", 1, clonedDeclaration.Invoices[0].JobComInvoiceLines.Count);
			AssertEquals("Has one invoice change", 1, clonedDeclaration.Invoices[0].JobComInvoiceLines[0].Charges.Count);
			AssertEquals("Invoice Has Changes", false, clonedDeclaration.Invoices[0].JobComInvoiceLines.HasChanges);
			AssertEquals("Invoice Charge Has Changes", false, clonedDeclaration.Invoices[0].JobComInvoiceLines[0].Charges.HasChanges);
		}

		[ExpectNoExceptions]
		public void TestAddNew()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine newInvoiceLine = invoice.JobComInvoiceLines.AddNew();
		}

		public void TestSortNotAffectLineNO()
		{
			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line2.JI_LineNo);
			AssertEquals("Line No", (short)3, line3.JI_LineNo);

			line3.JI_Description = "B";
			line2.JI_Description = "A";
			line1.JI_Description = "C";

			jobComInvoiceLines.Sort(BaseJobComInvoiceLine.Schema.JI_Description, ListSortDirection.Descending);

			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line2.JI_LineNo);
			AssertEquals("Line No", (short)3, line3.JI_LineNo);
		}

		public void TestUpdateLineNumbersNewAdded()
		{
			BaseJobComInvoiceLine line4 = jobComInvoiceLines.AddNew();
			AssertEquals("Line No", (short)4, line4.JI_LineNo);
		}

		public void TestUpdateLineNumbersDeleted()
		{
			line2.Delete();
			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line3.JI_LineNo);
		}

		public void TestUpdateLineNumbersDeletedAfterSorting()
		{
			line3.JI_Description = "3";
			line2.JI_Description = "2";
			line1.JI_Description = "1";
			jobComInvoiceLines.Sort(BaseJobComInvoiceLine.Schema.JI_Description, ListSortDirection.Descending);

			line2.Delete();
			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line3.JI_LineNo);
		}

		public void TestAddNewItemToInvoiceLinesOfDeclaration()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			var lines = testDec.FilteredInvoiceLines;

			BaseJobComInvoiceHeader header = testDec.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			header.JZ_JE = testDec.PK;
			BaseJobComInvoiceLine newItem = header.JobComInvoiceLines.AddNew();
			AssertEquals("InvoiceLines has the new item", true, lines.Contains(newItem));
		}

		public void TestItemsAddedToCollectionDuringConstructionAreNotRenumbered()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			var lines = testDec.FilteredInvoiceLines;
			BaseJobComInvoiceLine invoiceLine = lines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			invoiceLine.JI_LineNo = 3;
			Factory.Save();
			AssertEquals("Precondition : Line number after save", (short)1, invoiceLine.JI_LineNo);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			BaseJobDeclaration testDec2 = factory2.Load<BaseJobDeclaration>(testDec.PK);
			BaseJobComInvoiceHeader header2 = factory2.Load<BaseJobComInvoiceHeader>(invoiceHeader.PK);
			BaseJobComInvoiceLineViewCollection collection = new BaseJobComInvoiceLineViewCollection(header2, testDec2.InvoiceLines);
			AssertEquals("Line number in new factory", (short)1, collection[0].JI_LineNo);
		}

		public void TestClone()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoiceHeader = testDec.Invoices.AddNew();
			var lines = testDec.FilteredInvoiceLines;
			BaseJobComInvoiceLine invoiceLine = lines.AddNew();
			invoiceLine.JI_JZ = invoiceHeader.PK;
			invoiceLine.Charges.AddNew(Enterprise.Customs.Common.CustomsChargeTypeList.Codes.OtherCharges, 100m, testDec.LocalCurrencyCode);

			BaseJobDeclaration decCloned = (BaseJobDeclaration)testDec.TemplateCopy();
			AssertEquals("Cloned Line charges", 1, decCloned.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders[0].JobComInvoiceLines[0].Charges.Count);
		}

		public void TestGetByLineNo()
		{
			AssertNotNull("Get Line Number 1", jobComInvoiceLines.GetByLineNo(1));
			AssertEquals("JI_LineNo is 1", line1.JI_LineNo, jobComInvoiceLines.GetByLineNo(1).JI_LineNo);
			AssertNotNull("Get Line Number 2", jobComInvoiceLines.GetByLineNo(2));
			AssertEquals("JI_LineNo is 2", line2.JI_LineNo, jobComInvoiceLines.GetByLineNo(2).JI_LineNo);
			AssertNotNull("Get Line Number 3", jobComInvoiceLines.GetByLineNo(3));
			AssertEquals("JI_LineNo is 3", line3.JI_LineNo, jobComInvoiceLines.GetByLineNo(3).JI_LineNo);
			AssertNull("Get Line Number 4 doesn't exist", jobComInvoiceLines.GetByLineNo(4));
		}

		public void TestRenumberingLines()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line4 = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line2.JI_LineNo);
			AssertEquals("Line No", (short)3, line3.JI_LineNo);
			AssertEquals("Line No", (short)4, line4.JI_LineNo);

			line4.JI_LineNo = (short)2;

			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)3, line2.JI_LineNo);
			AssertEquals("Line No", (short)4, line3.JI_LineNo);
			AssertEquals("Line No", (short)2, line4.JI_LineNo);

			line1.JI_LineNo = 999;

			AssertEquals("Line No", (short)4, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line2.JI_LineNo);
			AssertEquals("Line No", (short)3, line3.JI_LineNo);
			AssertEquals("Line No", (short)1, line4.JI_LineNo);

			line1.JI_LineNo = (short)1;

			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)3, line2.JI_LineNo);
			AssertEquals("Line No", (short)4, line3.JI_LineNo);
			AssertEquals("Line No", (short)2, line4.JI_LineNo);

			line2.JI_LineNo = (short)1;

			AssertEquals("Line No", (short)2, line1.JI_LineNo);
			AssertEquals("Line No", (short)1, line2.JI_LineNo);
			AssertEquals("Line No", (short)4, line3.JI_LineNo);
			AssertEquals("Line No", (short)3, line4.JI_LineNo);
		}

		public void TestInsertingAndRenumberingLines()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			BaseJobComInvoiceHeader invoice = testDec.Invoices.AddNew();
			BaseJobComInvoiceLine line1 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line2 = invoice.JobComInvoiceLines.AddNew();
			BaseJobComInvoiceLine line3 = invoice.JobComInvoiceLines.AddNew();

			BaseJobComInvoiceLine line4 = Factory.New<BaseJobComInvoiceLine>();
			line4.JI_JZ = invoice.PK;

			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)2, line2.JI_LineNo);
			AssertEquals("Line No", (short)3, line3.JI_LineNo);
			AssertEquals("Line No", (short)4, line4.JI_LineNo);

			line4.JI_LineNo = (short)2;

			AssertEquals("Line No", (short)1, line1.JI_LineNo);
			AssertEquals("Line No", (short)3, line2.JI_LineNo);
			AssertEquals("Line No", (short)4, line3.JI_LineNo);
			AssertEquals("Line No", (short)2, line4.JI_LineNo);
		}

		#region Implementation

		protected BaseJobComInvoiceLineViewCollection jobComInvoiceLines;
		protected BaseJobComInvoiceHeader masterBizO;
		protected BaseJobComInvoiceLine line1;
		protected BaseJobComInvoiceLine line2;
		protected BaseJobComInvoiceLine line3;

		protected override void SetUp()
		{
			base.SetUp();
			BaseJobDeclaration declaration = BaseJobDeclaration.New(Factory);
			masterBizO = declaration.Invoices.AddNew();
			jobComInvoiceLines = masterBizO.JobComInvoiceLines;
			line1 = jobComInvoiceLines.AddNew();
			line2 = jobComInvoiceLines.AddNew();
			line3 = jobComInvoiceLines.AddNew();
		}

		protected virtual string ValidTariffNumber
		{
			get { return "00000000"; }
		}

		#endregion
	}
}
