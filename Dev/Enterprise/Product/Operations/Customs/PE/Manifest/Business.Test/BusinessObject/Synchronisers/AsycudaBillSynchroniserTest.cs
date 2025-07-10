using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillSynchroniser))]
	sealed class AsycudaBillSynchroniserTest : ManifestBillSynchroniserTest
	{
		public void TestABL_Incoterm()
		{
			var shipment = CreateAndPopulateShipment();
			var manifestHeader = CreateManifestHeader(shipment);
			var manifestBill = manifestHeader.Bills[0];

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_INCO = "CFI";
			AssertEquals("ABL_Incoterm", "CFI", manifestBill.ABL_Incoterm);

			SynchroniserManifestHeader(manifestHeader);

			shipment.JS_INCO = "CFR";
			AssertEquals("ABL_Incoterm", "CFR", manifestBill.ABL_Incoterm);
		}

		void SynchroniserManifestHeader(AsycudaManifestHeader manifestHeader)
		{
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
		}

		AsycudaManifestHeader CreateManifestHeader(ForwardingShipment shipment)
		{
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(shipment.Consols[0]);
			_ = manifestHeader.Bills.AddNew();
			return manifestHeader;
		}

		ForwardingShipment CreateAndPopulateShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "UYMVD";
			shipment.JS_RL_NKDestination = "COBOG";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			shipment.Consols.Add(consol);
			return shipment;
		}

		protected override ZString GetLastForeignPort(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;

		protected override IManifestBillForSynchroniser GetManifestBill(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_ParentId = Consol.PK;
			header.AMA_ParentTableCode = "JK";
			var bill = header.Bills.AddNew();
			return bill;
		}

		protected override BusinessObjectSynchroniser GetManifestBillSynchroniser(IManifestBillForSynchroniser bill, ForwardingShipment shipment) => new AsycudaBillSynchroniser((AsycudaBill)bill, shipment);

		protected override ZString GetPlaceOfReceipt(ForwardingShipment shipment) => shipment.JS_RL_NKDestination;

		protected override ZString GetPortOfLading(ForwardingShipment shipment) => shipment.JS_RL_NKOrigin;
	}
}
