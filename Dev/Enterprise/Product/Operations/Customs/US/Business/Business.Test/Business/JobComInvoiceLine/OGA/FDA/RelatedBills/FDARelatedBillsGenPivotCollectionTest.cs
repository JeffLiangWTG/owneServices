using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDARelatedBillsGenPivotCollection))]
	public class FDARelatedBillsGenPivotCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetRelatedPivotAndContains()
		{
			var billsForFDALine = FDA.BillsForFDALine;
			Bill bill1 = GetBill("99999");
			billsForFDALine.AddPivotFor(bill1);

			Bill bill2 = GetBill("88888");
			billsForFDALine.AddPivotFor(bill2);

			AssertEquals(billsForFDALine[0], billsForFDALine.GetRelatedPivot(bill1));
			AssertEquals(true, billsForFDALine.Contains(bill1));
		}

		public void TestAddPivotFor()
		{
			var billsForFDALine = FDA.BillsForFDALine;
			FDARelatedBill relatedBill1 = SetRelatedBill("99999");
			FDA fda = InvoiceLine.FDAs[0];
			AssertEquals(0, fda.BillsForFDALine.Count);
			fda.BillsAvailable.Add(relatedBill1);
			relatedBill1.IsForFDALine = true;
			AssertEquals(1, fda.BillsForFDALine.Count);

			Bill bill2 = GetBill("888888");
			billsForFDALine.AddPivotFor(bill2);
			AssertEquals(2, billsForFDALine.Count);
		}

		public void TestDeletePivotFor()
		{
			var billsForFDALine = FDA.BillsForFDALine;
			Bill bill1 = GetBill("9999");
			billsForFDALine.AddPivotFor(bill1);

			Bill bill2 = GetBill("88888");
			billsForFDALine.AddPivotFor(bill2);

			Bill bill3 = GetBill("77777");
			billsForFDALine.AddPivotFor(bill3);
			AssertEquals(3, billsForFDALine.Count);

			billsForFDALine.DeletePivotFor(bill2);
			AssertEquals(2, billsForFDALine.Count);
			AssertEquals(false, billsForFDALine.Contains(bill2));
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return FDA.BillsForFDALine;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<FDARelatedBillsGenPivot>();
		}

		FDA FDA
		{
			get { return fda ?? (fda = InvoiceLine.FDAs.AddNew()); }
		}
		FDA fda;

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					JobDeclaration declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.US_EnableCRL = true;

					JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}
				return invoiceLine;
			}
		}
		JobComInvoiceLine invoiceLine;

		FDARelatedBill SetRelatedBill(ZString billNumber)
		{
			FDARelatedBill result = new FDARelatedBill(InvoiceLine.FDAs[0]);
			result.SetBill(GetBill(billNumber));

			return result;
		}

		Bill GetBill(ZString billNumber)
		{
			Bill result = Factory.New<Bill>();
			result.CU_BillNum = billNumber;
			return result;
		}

		#endregion
	}
}
