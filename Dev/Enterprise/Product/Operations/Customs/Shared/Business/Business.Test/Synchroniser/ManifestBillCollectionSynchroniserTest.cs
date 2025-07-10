using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(ManifestBillCollectionSynchroniser<,>))]
	public abstract class ManifestBillCollectionSynchroniserTest : SynchroniserTestCase
	{
		public void TestManifestBillCollectionSynchroniser()
		{
			AssertEquals("Precondition", 0, header.Bills.Count());

			var shipment1 = Consol.Shipments.AddNew();
			AssertEquals("Bill1 added by auto synchroniser due to Collection_CountChanged", 1, header.Bills.Count());

			shipment1.JS_HouseBill = "1111";
			shipment1.JS_ActualWeight = 100.50;
			synchroniser.Synchronise(true);
			var bill1 = header.Bills.FirstOrDefault(x => x.MasterBillNumberInfo.Value.ToString() == "1111");
			AssertEquals("bill1 MasterBillNumberInfo and WeightInfo synchronised by force", shipment1.JS_ActualWeight, bill1.WeightInfo.Value);

			var shipment2 = Consol.Shipments.AddNew();
			AssertEquals("Bill2 added by auto synchroniser due to Collection_CountChanged", 2, header.Bills.Count());

			shipment2.JS_HouseBill = "2222";
			shipment2.JS_ActualWeight = 200.50;
			synchroniser.Synchronise(true);
			var bill2 = header.Bills.FirstOrDefault(x => x.MasterBillNumberInfo.Value.ToString() == "2222");
			AssertEquals("bill2.WeightInfo", shipment2.JS_ActualWeight, bill2.WeightInfo.Value);

			shipment2.JS_ActualWeight = 500.50;
			synchroniser.Synchronise(true);
			AssertEquals("Find existed bill2 to synchronise", shipment2.JS_ActualWeight, bill2.WeightInfo.Value);
			AssertEquals("bill1 no change", shipment1.JS_ActualWeight, bill1.WeightInfo.Value);

			synchroniser.SetEnabled(false, false);
			var bill3 = header.AddNewBill();
			AssertEquals("Bill", 3, header.Bills.Count());

			synchroniser.SetEnabled(true, false);
			synchroniser.Synchronise(true);
			AssertEquals("Still 2 Bills", 2, header.Bills.Count());
			AssertEquals("Bill3 deleted", true, bill3.IsDeleted);
		}

		public virtual void TestManifestBillCollectionSynchroniserHookEvent()
		{
			AssertEquals("Precondition", 0, header.Bills.Count());

			var shipment1 = Consol.Shipments.AddNew();
			AssertEquals("Bill1 added by auto synchroniser due to Collection_CountChanged", 1, header.Bills.Count());

			shipment1.JS_HouseBill = "1111";
			shipment1.JS_ActualWeight = 100.50;

			AssertEquals("Bill1s count", 1, header.Bills.Count());
			var bill1 = header.Bills.FirstOrDefault(x => x.MasterBillNumberInfo.Value.ToString() == "1111");
			AssertNotNull("synchroniser run when JS_HouseBill changed", bill1);
			AssertEquals("synchroniser run when JS_ShipmentType changed", shipment1.JS_ActualWeight, bill1.WeightInfo.Value);

			shipment1.JS_ActualWeight = 101.50;
			shipment1.JS_ShipmentType = "PTH";
			AssertEquals("Bill1s count", 1, header.Bills.Count());
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

		protected abstract IManifestHeaderForSynchroniser GetManifestBillHeader(ForwardingConsol consol);

		protected abstract BusinessObjectCollectionSynchroniser GetManifestBillCollectionSynchroniser(IManifestHeaderForSynchroniser header);

		protected override void SetUp()
		{
			base.SetUp();
			header = GetManifestBillHeader(Consol);
			synchroniser = GetManifestBillCollectionSynchroniser(header);
			synchroniser.SetEnabled(true, false);
		}

		protected ForwardingConsol Consol
		{
			get
			{
				if (consol == null)
				{
					consol = CreateFCLConsol();
					consol.JK_AgentType = Core.Constants.AgentType.Direct;
				}
				return consol;
			}
		}
		ForwardingConsol consol;

		public Type TestsInstancesOfType => GetManifestBillCollectionSynchroniser(GetManifestBillHeader(Consol)).GetType();

		protected IManifestHeaderForSynchroniser header;
		protected BusinessObjectCollectionSynchroniser synchroniser;
	}
}
