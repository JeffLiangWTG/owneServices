using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	sealed class AsycudaManifestHeaderSynchroniserTest : SynchroniserTestCase
	{
		public void TestSyncCarrier()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "Carrier";
			carrier.OH_RL_NKClosestPort = "AUMEL";
			carrier.MainAddress.Address1 = "Unit 000";
			carrier.MainAddress.Address2 = "Hypocrea astronidii";
			carrier.MainAddress.City = "Mel";
			carrier.MainAddress.Postcode = "2019";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "TW";

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("should sync carrier", carrier.MainAddress.PK, manifestHeader.AMA_OA_Carrier);
		}
	}
}
