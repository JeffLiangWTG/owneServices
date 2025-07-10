using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business.Testing
{
	public class CusDecHouseBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCU_ParentBillList()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			Bill master = declaration.Bills.AddNew();
			master.CU_BillType = BillTypeList.Codes.MasterBill;
			master.CU_BillNum = "2";

			Bill house = declaration.Bills.AddNew();
			house.CU_BillType = BillTypeList.Codes.HouseBill;
			house.CU_BillNum = "3";

			Bill subHouse = declaration.Bills.AddNew();
			subHouse.CU_BillType = BillTypeList.Codes.SubHouseBill;
			subHouse.CU_BillNum = "4";

			CodeDescriptionPairList list = master.Lookups.CU_ParentBillList;
			AssertEquals("Possible parent for master", 0, list.Count);

			list = house.Lookups.CU_ParentBillList;
			AssertEquals("Possible parent for House", 1, list.Count);
			AssertEquals("Possible parent for House", true, list.ContainsCode(master.CU_BillUniqueCode));

			list = subHouse.Lookups.CU_ParentBillList;
			AssertEquals("Possible parent for SubHouse", 1, list.Count);
			AssertEquals("Possible parent for SubHouse", true, list.ContainsCode(house.CU_BillUniqueCode));
		}

		public void TestNoOfPacksPackType_List()
		{
			Bill bill = Factory.New<Bill>();
			AssertNotNull(bill.Lookups.NoOfPacksPackType_List);
		}
	}
}
