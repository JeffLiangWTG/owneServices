using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBillSynchroniser))]
	sealed class AsycudaBillSynchroniserTest : ManifestBillSynchroniserTest
	{
		public void TestABL_PrepaidCollect()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = "GBSOU";
			shipment.JS_RL_NKDestination = "LKCMB";
			Factory.Save();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Factory.Save();

			shipment.Consols.Add(consol);
			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			var manifestBill = manifestHeader.Bills.AddNew();
			AssertEquals("Prereq", "", manifestBill.ABL_GoodsDescription);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			shipment.JS_PaymentTermAutoratingOverride = "CCX";
			AssertEquals("Collect", "CLT", manifestBill.ABL_PrepaidCollect);

			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			shipment.JS_PaymentTermAutoratingOverride = "PPD";
			AssertEquals("Prepaid", "PPD", manifestBill.ABL_PrepaidCollect);
		}

		public void TestSynchroniseManifestUQ()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "UT";

			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "NO";
			pack1.RP_Type = "GMB";
			pack1.RP_CustomsCountry = "UY";
			pack1.RP_ConversionFactor = 10;
			pack1.RP_CommercialPack = "DIZ";

			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			AssertEquals("Precondition", 1, manifestHeader.Bills.Count);
			var manifestBill = manifestHeader.Bills[0];
			manifestBill.Header.AMA_RN_NKCountry = "UY";

			AssertEquals("", "UT", manifestBill.ABL_ManifestUQ);
			AssertEquals("", 10, manifestBill.ABL_ManifestQty);
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
