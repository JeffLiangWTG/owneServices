using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	public sealed class N5101HGoodsShipmentTest : TestCaseWithFactory
	{
		public void TestConsignment()
		{
			AssertType<N5101HGoodsShipmentConsignment>(shipment.Consignment);
		}

		public void TestEntryOfficeId()
		{
			CombineAssertions("Entry Office ID", () =>
			{
				AssertType<ZString>(shipment.EntryOfficeId);
				AssertEquals("ddd", shipment.EntryOfficeId);
			});
		}
		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_CustomsOffice = "ddd";
			var masterBill = header.MasterBill;
			masterBill.ABL_GoodsLocation = "臺北";
			masterBill.ABL_BillNumber = "NO0001";
			shipment = new N5101HGoodsShipment(new N5101HGoodsShipmentConsignment(masterBill), header.AMA_CustomsOffice);
		}

		N5101HGoodsShipment shipment;
	}
}
