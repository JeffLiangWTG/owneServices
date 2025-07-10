using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DeliveryOrderBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBillList()
		{
			Declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			Declaration.JE_MasterBill = "MW23423";
			Declaration.JE_MasterBillIssuerSCAC = "ABCD";
			Declaration.JE_HouseBill = "HB23432";
			Declaration.JE_HouseBillIssuerSCAC = "BDSD";
			CodeDescriptionPairList list = Bill.Lookups.BillList;
			AssertEquals(Declaration.Bills.Count, list.Count);
			foreach (Bill bill in Declaration.Bills)
			{
				AssertEquals(bill.US_UI_NKBillIssuerSCAC + bill.CU_BillNum, list.GetCodeFromDescription(bill.CU_BillUniqueCode));
			}

			Declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			list = Bill.Lookups.BillList;
			foreach (Bill bill in Declaration.Bills)
			{
				AssertEquals(bill.IsMasterBill ? bill.CU_BillNum : (ZString)(bill.US_UI_NKBillIssuerSCAC + bill.CU_BillNum), list.GetCodeFromDescription(bill.CU_BillUniqueCode));
			}
		}

		public void TestCY_CodeList()
		{
			AssertEquals(typeof(Customs.Business.BillTypeList), Bill.Lookups.CY_CodeList.GetType());
		}

		JobDeclaration declaration;
		JobDeclaration Declaration => declaration ?? (declaration = Factory.New<JobDeclaration>());

		DeliveryOrderBill bill;
		DeliveryOrderBill Bill
		{
			get
			{
				if (bill == null)
				{
					var header = Declaration.DeliveryOrderHeaders.AddNew();
					bill = header.DeliveryOrderBills.AddNew();
				}
				return bill;
			}
		}
	}
}
