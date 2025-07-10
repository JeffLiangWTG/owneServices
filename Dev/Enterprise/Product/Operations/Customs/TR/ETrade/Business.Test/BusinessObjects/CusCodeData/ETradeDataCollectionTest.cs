using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(ETradeDataCollection))]
	public class ETradeDataCollectionTest : Customs.Business.Testing.CusCodeDataCollectionTest<ETradeData>
	{
		protected override Type GetExpectedCollectionType()
		{
			return typeof(ETradeDataCollection);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.ETradeBillDatas;
		}
		public override void TestSuspendCountChanged()
		{
			Assert(true);
		}

		protected override Customs.Business.CusCodeDataCollection<ETradeData> GetCusCodeDataCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return new ETradeDataCollection(bill, AsycudaBill.BillCYType);
		}
	}
}
