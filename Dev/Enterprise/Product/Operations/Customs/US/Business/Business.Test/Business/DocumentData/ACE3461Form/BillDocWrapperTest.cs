using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(BillDocWrapper))]
	sealed class BillDocWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBillDocWrapper()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "0012365489";
			bill.US_UI_NKBillIssuerSCAC = "ABC";
			bill.CU_NoOfPacks = 15m;
			bill.CU_PackType = ABIUnitOfMeasureList.Codes.Packs;
			bill.ITNumber = "1111";
			var masterBill = new BillDocWrapper(bill, BillDocWrapper.MasterBill);
			AssertEquals("X", masterBill.IsMaster);
			AssertEquals("", masterBill.IsInBond);
			AssertEquals("", masterBill.InBondNumber);
			AssertEquals("0012365489", masterBill.BillNumber);
			AssertEquals("ABC", masterBill.SCAC);
			AssertEquals(15m, masterBill.Quantity);
			AssertEquals(ABIUnitOfMeasureList.Codes.Packs, masterBill.UnitOfMeasure);
			var inbondBill = new BillDocWrapper(bill, BillDocWrapper.InBondBill);
			AssertEquals("X", inbondBill.IsInBond);
			AssertEquals("", inbondBill.IsMaster);
			AssertEquals("1111", inbondBill.InBondNumber);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.JE_MasterBill = "1234";
			var ftzBill = new BillDocWrapper(bill, BillDocWrapper.MasterBill);
			AssertEquals("FTZ1234", ftzBill.SCAC);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var bill = declaration.Bills.AddNew();
			return new BillDocWrapper(bill, BillDocWrapper.MasterBill);
		}
	}
}
