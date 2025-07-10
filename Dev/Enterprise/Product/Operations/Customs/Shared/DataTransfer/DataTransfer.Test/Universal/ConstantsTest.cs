using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class ConstantsTest : NUnit.Framework.TestCase
	{
		public void TestGetWayBillType()
		{
			AssertEquals(WayBillTypeList.Codes.Master, Constants.GetWayBillType(BillTypeList.Codes.MasterBill));
			AssertEquals(WayBillTypeList.Codes.House, Constants.GetWayBillType(BillTypeList.Codes.HouseBill));
			AssertEquals(WayBillTypeList.Codes.SubHouse, Constants.GetWayBillType(BillTypeList.Codes.SubHouseBill));
			AssertEquals(WayBillTypeList.Codes.House, Constants.GetWayBillType("ZS"));
			AssertEquals(WayBillTypeList.Codes.House, Constants.GetWayBillType(ZString.Empty));
		}
	}
}
