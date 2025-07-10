using CargoWise.EntityFramework.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DataTransfer.Universal.Testing
{
	sealed class UniversalCommonHelperTest : TestCaseWithFactory
	{
		public void TestIsBillMatched()
		{
			var bill1 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance);
			bill1.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			bill1.BillNumber = "HB1";
			bill1.ParentBillNumber = "MH";
			var bill2 = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance);
			bill2.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			bill2.BillNumber = "HB1";
			bill2.ParentBillNumber = "MH";
			Assert(UniversalCommonHelper.IsBillMatched(bill1, bill2));
			bill2.BillType = new WayBillType() { Code = WayBillTypeList.Codes.SubHouse };
			Assert(!UniversalCommonHelper.IsBillMatched(bill1, bill2));
			bill2.BillType = new WayBillType() { Code = WayBillTypeList.Codes.House };
			bill2.BillNumber = "HB2";
			Assert(!UniversalCommonHelper.IsBillMatched(bill1, bill2));
			bill2.BillNumber = "HB1";
			bill2.ParentBillNumber = "MH1";
			Assert(!UniversalCommonHelper.IsBillMatched(bill1, bill2));
		}
	}
}
