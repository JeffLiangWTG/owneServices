using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(CusInBondBillCollectionSynchroniser))]
	sealed class CusInBondBillCollectionSynchroniserInheritedTest : Customs.Business.Testing.ManifestBillCollectionSynchroniserTest
	{
		protected override BusinessObjectCollectionSynchroniser GetManifestBillCollectionSynchroniser(IManifestHeaderForSynchroniser header)
		{
			return new CusInBondBillCollectionSynchroniser((CusInBondHeader)header);
		}

		protected override IManifestHeaderForSynchroniser GetManifestBillHeader(ForwardingConsol consol)
		{
			var header = Factory.NewWithValidTestData<CusInBondHeader>();
			header.BH_ParentID = consol.PK;
			header.BH_ParentTableCode = "JK";
			return header;
		}

		public override void TestManifestBillCollectionSynchroniserHookEvent()
		{
			AssertEquals("Precondition", 0, header.Bills.Count());
			var shipment1 = Consol.Shipments.AddNew();
			AssertEquals("Bill1 added by auto synchroniser due to Collection_CountChanged", 1, header.Bills.Count());
			shipment1.JS_HouseBill = "1111";
			shipment1.JS_ActualWeight = 100.50;
			AssertEquals("Bill1s count", 1, header.Bills.Count());
			var bill1 = header.Bills.FirstOrDefault(x => x.MasterBillNumberInfo.Value.ToString() == "1111");
			AssertNotNull(bill1);
			AssertEquals("synchroniser run when JS_ShipmentType changed", shipment1.JS_ActualWeight, bill1.WeightInfo.Value);
			shipment1.JS_ShipmentType = "PTH";
			AssertEquals("Bill1s count", 1, header.Bills.Count());
			bill1 = header.Bills.FirstOrDefault(x => x.MasterBillNumberInfo.Value.ToString() == "1111");
			AssertEquals("synchroniser run when JS_ShipmentType changed", shipment1.JS_ActualWeight, bill1.WeightInfo.Value);
			shipment1.JS_ActualWeight = 200.50;
			shipment1.CoLoadShipments.AddNew();
			AssertEquals("Bill1s count", 1, header.Bills.Count());
			AssertEquals("synchroniser run when CoLoadShipments changed", shipment1.JS_ActualWeight, bill1.WeightInfo.Value);
			shipment1.JS_ActualWeight = 300.50;
			shipment1.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("synchroniser run when Consignee changed", shipment1.JS_ActualWeight, bill1.WeightInfo.Value);
			shipment1.JS_ActualWeight = 400.50;
			shipment1.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertEquals("synchroniser run when HouseBillIssuingPartyDocumentaryAddress changed", shipment1.JS_ActualWeight, bill1.WeightInfo.Value);
			synchroniser.SetEnabled(false, false);
			var shipment2 = Consol.Shipments.AddNew();
			AssertEquals("Bill won't be added by auto synchroniser due to Enable = false", 1, header.Bills.Count());
			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise(true);
			AssertEquals("Bill2 added", 2, header.Bills.Count());
			Consol.Shipments.RemoveAll();
			AssertEquals("All bills deleted", 0, header.Bills.Count());
		}
	}
}
