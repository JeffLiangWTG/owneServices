using Enterprise.Customs.Business;
using Enterprise.Customs.US.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.US.DataTransfer.Universal.Testing
{
	public abstract class InBondDataObjectWriterHelperTest<THeader> : InBondHelperTest where THeader : CusInBondHeader
	{
		public void TestContainerModeList()
		{
			AssertEquals(typeof(ContainerModeList), Helper.ContainerModeList.GetType());
		}

		public void TestTransportTypeList()
		{
			AssertEquals(typeof(Business.TransportTypeList), Helper.TransportTypeList.GetType());
		}

		public void TestAllocateBillLinkAndSetBillLink()
		{
			var bill1 = (CusInBondBill)InBondHeader.Bills.AddNew();
			var billData = new AdditionalBill(DefaultDataObjectWriterStrategy.TestInstance);
			Helper.AllocateBillLink(bill1, billData);
			AssertEquals(1, billData.Link);
			var bill2 = (CusInBondBill)InBondHeader.Bills.AddNew();
			Helper.AllocateBillLink(bill2, billData);
			AssertEquals(2, billData.Link);
			Helper.AllocateBillLink(bill1, billData);
			AssertEquals(1, billData.Link);
			var moveDetailData = new InBondMoveDetail();
			Helper.SetBillLink(bill1, moveDetailData);
			AssertEquals(1, moveDetailData.AdditionalBillLink);
			Helper.SetBillLink(bill2, moveDetailData);
			AssertEquals(2, moveDetailData.AdditionalBillLink);
		}

		public void TestAllocateContainerLink()
		{
			var bill = (CusInBondBill)InBondHeader.Bills.AddNew();
			var moveDetail = bill.MovementDetail;
			var container1 = (CusInBondContainer)moveDetail.Containers.AddNew();
			var containerData = new Container(DefaultDataObjectWriterStrategy.TestInstance);
			Helper.AllocateContainerLink(container1, containerData);
			AssertEquals(1, containerData.Link);
			var container2 = (CusInBondContainer)moveDetail.Containers.AddNew();
			Helper.AllocateContainerLink(container2, containerData);
			AssertEquals(2, containerData.Link);
			Helper.AllocateContainerLink(container1, containerData);
			AssertEquals(1, containerData.Link);
		}

		THeader inBondHeader;
		protected THeader InBondHeader => inBondHeader ?? (inBondHeader = Factory.New<THeader>());

		InBondDataObjectWriterHelper helper;
		protected InBondDataObjectWriterHelper Helper => helper ?? (helper = CreateHelper(InBondHeader));

		protected abstract InBondDataObjectWriterHelper CreateHelper(THeader header);
	}
}
