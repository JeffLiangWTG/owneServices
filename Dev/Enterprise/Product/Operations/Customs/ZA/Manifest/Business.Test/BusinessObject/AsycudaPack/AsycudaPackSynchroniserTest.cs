using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	sealed class AsycudaPackSynchroniserTest : SynchroniserTestCase
	{
		public void TestAsycudaPackSynchroniserSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 3;
			line1.JL_F3_NKPackType = "UT";
			var line2 = shipment.OuterPackLines.AddNew();
			line2.JL_PackageCount = 7;
			line2.JL_F3_NKPackType = "UT";
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "NO";
			pack1.RP_Type = "GMP";
			pack1.RP_CustomsCountry = "ZA";
			pack1.RP_ConversionFactor = 10;
			pack1.RP_CommercialPack = "DIZ";
			Factory.Save();
			var manifestHeader = Factory.New<AsycudaManifestHeader>();
			manifestHeader.AMA_RN_NKCountry = "ZA";
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
			var manifestBill = manifestHeader.Bills[0];
			AssertEquals("2 Pack Sychronized", 2, manifestBill.Packs.Count);
			AssertEquals("", "UT", manifestBill.Packs[0].APA_PackUQ);
			AssertEquals("", 3, manifestBill.Packs[0].APA_PackQty);
			Assert(!manifestBill.Packs[0].APA_PackUQInfo.ReadOnly);
			AssertEquals("", "UT", manifestBill.Packs[1].APA_PackUQ);
			AssertEquals("", 7, manifestBill.Packs[1].APA_PackQty);
			Assert(!manifestBill.Packs[1].APA_PackUQInfo.ReadOnly);
			line1.JL_F3_NKPackType = "DIZ";
			line2.JL_F3_NKPackType = "DIZ";
			manifestHeader.Synchroniser.Synchronise();
			AssertEquals("", "NO", manifestBill.Packs[0].APA_PackUQ);
			AssertEquals("", 30, manifestBill.Packs[0].APA_PackQty);
			Assert(manifestBill.Packs[0].APA_PackUQInfo.ReadOnly);
			AssertEquals("", "NO", manifestBill.Packs[1].APA_PackUQ);
			AssertEquals("", 70, manifestBill.Packs[1].APA_PackQty);
			Assert(manifestBill.Packs[1].APA_PackUQInfo.ReadOnly);
		}
	}
}
