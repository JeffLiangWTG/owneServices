using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPackSynchroniser))]
	sealed class AsycudaPackSynchroniserTest : SynchroniserTestCase
	{
		public void TestSyncGoodsDescription()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_Description = "DESC 1";

			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_Description = "DESC 2";

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "NO";
			pack1.RP_Type = "GMP";
			pack1.RP_CustomsCountry = "AU";
			pack1.RP_ConversionFactor = 10;
			pack1.RP_CommercialPack = "DIZ";

			Factory.Save();

			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "TW";

			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			var manifestBill = manifestHeader.Bills[0];
			AssertEquals("should sync first line", "DESC 1", manifestBill.GoodsDescription);
		}
	}
}
