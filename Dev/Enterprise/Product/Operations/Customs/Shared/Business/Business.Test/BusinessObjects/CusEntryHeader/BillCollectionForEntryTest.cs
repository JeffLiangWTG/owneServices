using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(BillCollectionForEntry))]
	public class BillCollectionForEntryTest : BusinessObjectCollectionTestCase
	{
		public void TestCollectionWhenNoBills()
		{
			AssertEquals(0, entryHeader.Bills.Count);
		}

		public void TestCollectionWithOneBill()
		{
			Bill bill = declaration.Bills.AddNew();
			AssertEquals(bill, entryHeader.Bills[0]);
		}

		public void TestBillsCollectionWhenOnlyOneEntry()
		{
			Bill bill1 = declaration.Bills.AddNew();
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_CU_ParentBill = ZGuid.Empty;

			AssertEquals(2, entryHeader.Bills.Count);
			AssertCollectionContains(bill1, entryHeader.Bills);
			AssertCollectionContains(bill2, entryHeader.Bills);
		}

		public void TestBillsReturnsOnlyRelatedBillsWhenMultipleEntries()
		{
			Bill bill1 = declaration.Bills.AddNew();
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_CU_ParentBill = ZGuid.Empty;

			CusEntryHeader entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();
			CusEntryLine entryLine2 = entryHeader2.MergedLines.AddNew();

			BaseJobComInvoiceHeader invoiceHeader = declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			invoiceHeader.JZ_CU_RelatedHouseBill = bill1.PK;
			BaseJobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;

			AssertEquals(1, entryHeader.Bills.Count);
			AssertEquals(bill1, entryHeader.Bills[0]);
		}

		public void TestCollectionWithMultipleBills()
		{
			Bill masterBill1 = declaration.Bills.AddNew();
			masterBill1.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill1.CU_BillNum = "MBL1";

			Bill masterBill2 = declaration.Bills.AddNew();
			masterBill2.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill2.CU_BillNum = "MBL2";

			Bill bill1 = declaration.Bills.AddNew();
			bill1.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill1.CU_HouseBill = "HBL2";
			bill1.CU_MasterBill = "MBL2";
			Bill bill2 = declaration.Bills.AddNew();
			bill2.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill2.CU_HouseBill = "HBL1";
			bill2.CU_MasterBill = "MBL2";
			Bill bill3 = declaration.Bills.AddNew();
			bill3.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill3.CU_HouseBill = "HBL2";
			bill3.CU_MasterBill = "MBL1";
			Bill bill4 = declaration.Bills.AddNew();
			bill4.CU_BillType = Customs.Business.BillTypeList.Codes.HouseBill;
			bill4.CU_HouseBill = "HBL1";
			bill4.CU_MasterBill = "MBL1";

			CusEntryLine entryLine = entryHeader.MergedLines.AddNew();

			AssertEquals("HBL1", entryHeader.Bills[0].CU_HouseBill);
			AssertEquals("MBL1", entryHeader.Bills[0].CU_MasterBill);

			AssertEquals("HBL1", entryHeader.Bills[1].CU_HouseBill);
			AssertEquals("MBL2", entryHeader.Bills[1].CU_MasterBill);

			AssertEquals("HBL2", entryHeader.Bills[2].CU_HouseBill);
			AssertEquals("MBL1", entryHeader.Bills[2].CU_MasterBill);

			AssertEquals("HBL2", entryHeader.Bills[3].CU_HouseBill);
			AssertEquals("MBL2", entryHeader.Bills[3].CU_MasterBill);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			declaration = SetDeclaration();
			entryHeader = declaration.CustomsEntryHeaders[0];
		}
		protected BaseJobDeclaration declaration;
		protected CusEntryHeader entryHeader;

		protected BaseJobDeclaration SetDeclaration()
		{
			declaration = Factory.New<BaseJobDeclaration>();
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			return declaration;
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			declaration = SetDeclaration();
			entryHeader = declaration.CustomsEntryHeaders[0];
			return new BillCollectionForEntry(entryHeader);
		}

		#endregion
	}
}
