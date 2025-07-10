using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(FDARelatedBillsGenPivot))]
	public class FDARelatedBillsGenPivotTest : EnterpriseBusinessObjectTestCase
	{
		public void TestBillMasterHouse()
		{
			Bill bill = InvoiceLine.Declaration.Bills.AddNew();
			bill.CU_BillNum = "9999";
			bill.US_UI_NKBillIssuerSCAC = "QF";
			bill.CU_BillType = BillTypeList.Codes.MasterBill;

			Bill bill1 = InvoiceLine.Declaration.Bills.AddNew();
			bill1.CU_BillNum = "8888";
			bill1.US_UI_NKBillIssuerSCAC = "APLU";
			bill1.CU_BillType = BillTypeList.Codes.HouseBill;
			bill1.CU_CU_ParentBill = bill.PK;
			var fdaBillsAvailable = FDA.BillsAvailable;
			AssertEquals(2, fdaBillsAvailable.Count);
			fdaBillsAvailable[1].IsForFDALine = true;

			AssertEquals(1, FDA.BillsForFDALine.Count);
			var billForFDALine = FDA.BillsForFDALine[0];
			AssertEquals("9999", ((IMasterHouse)billForFDALine).MasterBill);
			AssertEquals("QF", ((IMasterHouse)billForFDALine).MasterBillSCAC);
			AssertEquals("8888", ((IMasterHouse)billForFDALine).HouseBill);
			AssertEquals("APLU", ((IMasterHouse)billForFDALine).HouseBillSCAC);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return FDARelatedBillsGenPivot;
		}

		FDARelatedBillsGenPivot FDARelatedBillsGenPivot
		{
			get { return fdaRelatedBillsGenPivot ?? (fdaRelatedBillsGenPivot = Factory.New<FDARelatedBillsGenPivot>()); }
		}
		FDARelatedBillsGenPivot fdaRelatedBillsGenPivot;

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

		FDA FDA
		{
			get { return fda ?? (fda = InvoiceLine.FDAs.AddNew()); }
		}
		FDA fda;
	}
}
