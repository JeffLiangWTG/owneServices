using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public sealed class N5101HGoodsShipmentConsignmentTest : TestCaseWithFactory
	{
		public void TestTransportContractDocument()
		{
			AssertType<N5101HTransportContractDocument>(consignment.TransportContractDocument);
			AssertEquals("Transport Contract Document ID", "BILL001", consignment.TransportContractDocument.ID);
		}

		protected override void SetUp()
		{
			base.SetUp();
			bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_BillNumber = "BILL001";
			consignment = new N5101HGoodsShipmentConsignment(bill);
		}

		AsycudaBill bill;
		IN5101HConsignment consignment;
	}
}
