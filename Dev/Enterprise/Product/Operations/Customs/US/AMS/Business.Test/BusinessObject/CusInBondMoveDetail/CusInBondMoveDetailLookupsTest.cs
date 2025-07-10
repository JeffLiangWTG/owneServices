using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.US.AMS;
using Enterprise.Customs.US.AMS.Messaging.Business;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	class CusInBondMoveDetailLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCustomsStatusList()
		{
			var moveDetail = Factory.New<CusInBondMoveDetail>();
			AssertEquals(Factory.GetCachedValue<AMSBillCustomsStatusList>(), moveDetail.Lookups.CustomsStatusList);

			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			moveDetail.B9_BM = moveHeader.PK;
			moveDetail.MoveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.PermitToTransfer;
			AssertEquals(Factory.GetCachedValue<AMSBillMessageStatusList>(), moveDetail.Lookups.CustomsStatusList);

			moveDetail.MoveHeader.BM_SubApplicationCode = SubApplicationCodeList.Codes.MasterInBond;
			AssertEquals(Factory.GetCachedValue<AMSBillMessageStatusList>(), moveDetail.Lookups.CustomsStatusList);
		}

		public void TestInBondStatusList()
		{
			var moveDetail = Factory.New<CusInBondMoveDetail>();
			AssertEquals(AMSBillMessageStatusList.GetInBondCodeList(Factory), moveDetail.Lookups.InBondStatusList);
		}

		public void TestMessageStatusList()
		{
			var moveDetail = Factory.New<CusInBondMoveDetail>();
			AssertEquals(Factory.GetCachedValue<AMSBillMessageStatusList>(), moveDetail.Lookups.MessageStatusList);
		}

		public void TestRelatedBills()
		{
			var moveDetail = Factory.New<CusInBondMoveDetail>();
			var bills = moveDetail.Lookups.RelatedBills;
			AssertEquals(0, bills.Count);
			AssertEquals(true, bills.CompleteFilter.IsNoResultQuery);

			var header = Factory.New<CusInBondHeader>();
			var moveHeader = header.MovementHeader;
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();
			moveDetail.B9_BM = moveHeader.PK;

			bills = moveDetail.Lookups.RelatedBills;
			AssertEquals(2, bills.Count);
			AssertEquals(header.Bills, bills);
		}
	}
}
