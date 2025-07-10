using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.ZA.Manifest.Business
{
	class AsycudaManifestHeaderSynchroniserTest : SynchroniserTestCase
	{
		public void TestCustomsOfficeSynchroniser()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Road;
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_CustomsOffice = "TLD";
			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			Factory.Save();
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = "ZA";
			AssertEquals("TLD", manifestHeader.AMA_CustomsOffice);
		}
	}
}
