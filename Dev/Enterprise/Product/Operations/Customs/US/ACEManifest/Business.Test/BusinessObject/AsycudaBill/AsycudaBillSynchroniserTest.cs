using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillSynchroniser))]
	sealed class AsycudaBillSynchroniserTest : ManifestBillSynchroniserTest
	{
		public void TestFreightAndGoodsValue()
		{
			var shipment = CreateAndPopulateShipment();
			var manifestHeader = CreateManifestHeader(shipment);
			var manifestBill = manifestHeader.Bills[0];

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_GoodsValue = 100;
			AssertEquals("Goods value is 100", 100m, manifestBill.ABL_GoodsValue);
			AssertEquals("Freight value is 0", 0m, manifestBill.ABL_FreightValue);

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_GoodsValue = 0;
			AssertEquals("Goods value is 0", 0m, manifestBill.ABL_GoodsValue);
			AssertEquals("Freight value is 0", 0m, manifestBill.ABL_FreightValue);
		}

		public void TestFreightAndGoodsCurrenciesValue()
		{
			var shipment = CreateAndPopulateShipment();
			var manifestHeader = CreateManifestHeader(shipment);
			var manifestBill = manifestHeader.Bills[0];

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_RX_NKGoodsValueCurr = "ARS";
			AssertEquals("Goods value currency is ARS", "ARS", manifestBill.ABL_RX_NKGoodsValueCurrency);
			AssertEquals("Freight value currency is empty by default", "", manifestBill.ABL_RX_NKFreightValueCurrency);

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_RX_NKGoodsValueCurr = ZString.Empty;
			AssertEquals("Goods value currency is USD", "USD", manifestBill.ABL_RX_NKGoodsValueCurrency);
			AssertEquals("Freight value currency is empty by default", "", manifestBill.ABL_RX_NKFreightValueCurrency);
		}

		ForwardingShipment CreateAndPopulateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			Factory.Save();

			shipment.Consols.Add(consol);
			return shipment;
		}

		AsycudaManifestHeader CreateManifestHeader(ForwardingShipment shipment)
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.AMA_ParentId = shipment.Consols[0].PK;
			manifestHeader.AMA_ParentTableCode = "JK";
			_ = manifestHeader.Bills.AddNew();
			Factory.Save();
			return manifestHeader;
		}

		void SynchroniserManifestHeader(AsycudaManifestHeader manifestHeader)
		{
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
		}

		protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = Consol.PK;
			header.AMA_ParentTableCode = "JK";
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment) => new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);

		protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

		protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;
	}
}
